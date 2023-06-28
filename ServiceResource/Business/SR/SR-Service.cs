using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Log.Entities;
using System.Reflection;
using System.Text.Json.Serialization;
using YouRest;
using YouRest.Interface.Body;

namespace ServiceResource.Business.SR;

public class SR_Service : ISR_Service
{
    public Dictionary<ServiceCallingMode, Func<SRRequest, Task<SRResponse>>> CallingMode = new Dictionary<ServiceCallingMode, Func<SRRequest, Task<SRResponse>>>();
    //public Interfaces.ILogger Logger { get; }
    public IQueueHandler QueueRepository { get; }
    public ILogRepository Logger { get; }

    public SR_Service(IQueueHandler queueRepository, ILogRepository logger)
    {
        QueueRepository = queueRepository;
        Logger = logger;
        CallingMode.Add(ServiceCallingMode.Immediate, CallImmediate);
        CallingMode.Add(ServiceCallingMode.QueueOnFaild, CallQueueOnFaild);
        CallingMode.Add(ServiceCallingMode.DirectlyToQueue, CallDirectlyToQueue);
        CallingMode.Add(ServiceCallingMode.ImmediateWithCheckResult, ImmediateWithCheckResult);
    }

    public async Task<SRResponse> CallProcessAsync(SRRequest request)
    {
        SRResponse response = new SRResponse();
        Exception exeptio = null;
        SuccessInfo Success = SuccessInfo.Success;
        //Insert Request Log
        var RequestLog = new RequestLog
        {
            Input = JsonConvert.SerializeObject(TryRemoveSensitiveDataFromJson(JsonConvert.SerializeObject(request.Input), request.InputSensitiveData)),
            PointerId = request.PointerId,
            MethodName = request.MethodName,
            CallTime = DateTime.Now,
        };
        try
        {
            await Logger.Log(RequestLog);
        }
        catch (Exception ex)
        {
        }


        try
        {
            //Check For Mock
            if (request.Mock != null)
            {
                 response = await MockResponse(request);
                Success = response.Success;
                return response;
            }

            //Call Based On ServiceCallingMode
             response = await CallingMode[request.CallingMode](request);
            Success = response.Success;
            return response;
        }
        catch (Exception ex)
        {
            exeptio = ex;
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            Success = SuccessInfo.Faild;
            return new SRResponse
            {
                ErrorCode = -100,
                Exception = new ExceptionDto
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                },
                Message = "مشکل",
                Response = null,
                Success = Success
            };
        }
        finally
        {
            try
            {//Insert ResponseLog
                var responselog = new ResponseLog
                {
                    Input = JsonConvert.SerializeObject(TryRemoveSensitiveDataFromJson(JsonConvert.SerializeObject(request.Input), request.InputSensitiveData)),
                    Output = JsonConvert.SerializeObject(TryRemoveSensitiveDataFromJson(response.Response, request.OutputSensitiveData)),
                    PointerId = request.PointerId,
                    MethodName = request.MethodName,
                    CallTime = RequestLog.CallTime,
                    ErrorCode = -100,
                    ResponseTime = DateTime.Now,
                    Exception = exeptio != null ? JsonConvert.SerializeObject(exeptio) : JsonConvert.SerializeObject(response.Exception),
                    RequestId = RequestLog.Id,
                    SummeryData = Success.ToString()
                };

                await Logger.Log(responselog);
            }
            catch (Exception ex)
            {

            }
        }


    }

    private static object? TryRemoveSensitiveData(object input, List<string>? sensitiveData)
    {
        if (input == null || sensitiveData == null)
            return null;

        var properties = sensitiveData;

        foreach (var property in properties)
        {
            var propertyParts = property.Split('.');
            object currentObject = input;

            foreach (var part in propertyParts)
            {
                var propertyInfo = currentObject.GetType().GetProperty(part);

                if (propertyInfo == null)
                    break;

                if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(string))
                {
                    currentObject = propertyInfo.GetValue(currentObject);
                }
                else
                {
                    propertyInfo.SetValue(currentObject, "***");
                }
            }
        }

        return input;
    }
    private static object? TryRemoveSensitiveDataFromJson(string jsonString, List<string>? inputSensitiveData)
    {
        if (inputSensitiveData == null || string.IsNullOrWhiteSpace(jsonString))
            return jsonString;

        var jObject = JObject.Parse(jsonString);
        ReplaceSensitiveFields(jObject, inputSensitiveData);

        return jObject;
    }

    private static void ReplaceSensitiveFields(JObject jObject, List<string> sensitiveFields)
    {
        foreach (var fieldPath in sensitiveFields)
        {
            var fieldNames = fieldPath.Split('.');
            var token = jObject;
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];

                string fieldNameToLower = fieldName.ToLower(); // Convert fieldName to lowercase

                // Loop through the properties of the jObject
                foreach (var property in token.Properties())
                {
                    string propertyNameToLower = property.Name.ToLower(); // Convert property name to lowercase

                    // Compare the lowercase property name with the lowercase fieldName
                    if (propertyNameToLower == fieldNameToLower)
                    {
                        if (i == fieldNames.Length - 1)
                        {
                            property.Value = "***";
                        }
                        else if (property.Value is JObject nestedObject)
                        {
                            token = nestedObject;
                        }
                        else
                        {
                            break; // Stop replacing if intermediate field is not an object
                        }
                        break; // Exit the loop if a match is found
                    }
                }
            }
        }
    }

    private async Task<SRResponse> CallImmediate(SRRequest request)
    {
        var ClassInstance = GetClassInstance(request);
        var result = await ClassInstance.GetResponse(request.Input , request.SendTimeoutSecounds);
        return new SRResponse
        {
            ErrorCode = 0,
            Exception = null,
            Message = "Ok.",
            Response = JsonConvert.SerializeObject(result),
            Success = SuccessInfo.Success
        };
    }
    private async Task<SRResponse> CallQueueOnFaild(SRRequest request)
    {
        SuccessInfo successInfo = SuccessInfo.Success;
        var ClassInstance = GetClassInstance(request);
        try
        {
            var result = await ClassInstance.GetResponse(request.Input , request.SendTimeoutSecounds);
            if (request.CheckResult != null)
            {
                RestResponse_VM<CheckResultResponse> CheckResult = CallCheckResponse(new CheckResultRequest { Success = true, Exception = null, Response = result, MethodName = request.MethodName });

                if (!CheckResult.IsSuccess) throw new Exception(); //TODO

                if (CheckResult.GetResponse().Success)
                {
                    goto Success;
                }

                await InsertInQueue(request);
                successInfo = SuccessInfo.Faild_But_Inserted_In_Queue;
            }
        //Send For Check Result
        Success:
            return new SRResponse
            {
                ErrorCode = 0,
                Exception = null,
                Message = "Ok.",
                Response = JsonConvert.SerializeObject(result),
                Success = successInfo
            };
        }
        catch (Exception ex)
        {
            if (request.CheckResult != null)
            {
                RestResponse_VM<CheckResultResponse> CheckResult = CallCheckResponse(new CheckResultRequest { Success = false, Exception = ex, Response = null });

                if (!CheckResult.IsSuccess) throw new Exception(); //TODO
                if (CheckResult.GetResponse().Success)
                {
                    goto Success;
                }
            }
            //Insert In Queue
            await InsertInQueue(request);
            successInfo = SuccessInfo.Faild_But_Inserted_In_Queue;

        Success:
            return new SRResponse
            {
                ErrorCode = 0,
                Exception = new ExceptionDto
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                },
                Message = "Ok.",
                Response = null,
                Success = successInfo
            };
        }

    }
    private async Task<SRResponse> CallDirectlyToQueue(SRRequest request)
    {
        await InsertInQueue(request);
        return new SRResponse();
    }
    private async Task<SRResponse> ImmediateWithCheckResult(SRRequest request)
    {
        var ClassInstance = GetClassInstance(request);
        var result = await ClassInstance.GetResponse(request.Input, request.SendTimeoutSecounds);
        if (request.CheckResult != null)
        {
            RestResponse_VM<CheckResultResponse> CheckResult = CallCheckResponse(new CheckResultRequest { Success = true, Exception = null, Response = result, MethodName = request.MethodName });
            if (!CheckResult.IsSuccess) throw new Exception(); //TODO
            if (CheckResult.GetResponse().Success)
            {
                goto Success;
            }
            else
            {
                throw new Exception("CheckResult.GetResponse().Faild");
            }
        }
    Success: return new SRResponse
    {
        ErrorCode = 0,
        Exception = null,
        Message = "Ok.",
        Response = JsonConvert.SerializeObject(result),
        Success = SuccessInfo.Success
    };
    }
    private async Task InsertInQueue(SRRequest request)
    {
        await QueueRepository.InsertInQueue(request, 0);
    }
    private static RestResponse_VM<CheckResultResponse> CallCheckResponse(CheckResultRequest checkResultRequest)
    {
        RestCaller restCaller = new RestCaller(new RestStaticProperties
        {
            BaseAddress = "",
            Timeout = new TimeSpan(0, 0, 90)
        });
        var CheckResult = restCaller.CallRestService<CheckResultResponse>(new RestRequest_VM
        {
            Body = new JsonBody(checkResultRequest),
            EnsureSuccessStatusCode = true,
            HttpMethod = HttpMethod.Post,
        });
        return CheckResult;
    }
    private async Task<SRResponse> MockResponse(SRRequest request)
    {
        if (request.Mock.ExpectedAnswer == ExpectedAnswer.Success && request.Mock.Response != null)
        {
            return new SRResponse
            {
                ErrorCode = 0,
                Exception = null,
                Message = "Ok.",
                Response = JsonConvert.SerializeObject(request.Mock.Response),
                Success = SuccessInfo.Success,
            };
        }
        if (request.Mock.ExpectedAnswer == ExpectedAnswer.Faild && request.Mock.Exception != null)
        {
            return new SRResponse
            {
                ErrorCode = -1000,
                Exception = new ExceptionDto
                {
                    StackTrace = request.Mock.Exception.StackTrace,
                    Message = request.Mock.Exception.Message,
                },
                Message = "Exception.",
                Response = null,
                Success = SuccessInfo.Faild,
            };
        }
        throw new Exception(); //TODO
    }
    private static BaseSRService GetClassInstance(SRRequest request)
    {
        string assemblyQualifiedName = $"ServiceResource.Services.{request.MethodName.ToString()}, ServiceResource";
        Type type = Type.GetType(assemblyQualifiedName);
        if (type == null || !typeof(BaseSRService).IsAssignableFrom(type))
        {
            throw new ArgumentException("Invalid class name or class does not inherit from BaseSRService.");
        }

        return (BaseSRService)Activator.CreateInstance(type);
    }
}



