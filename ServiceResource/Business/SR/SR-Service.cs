using Newtonsoft.Json;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Interfaces;
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

    public SR_Service(IQueueHandler queueRepository)
    {
        // Logger = logger;
        QueueRepository = queueRepository;
        CallingMode.Add(ServiceCallingMode.Immediate, CallImmediate);
        CallingMode.Add(ServiceCallingMode.QueueOnFaild, CallQueueOnFaild);
        CallingMode.Add(ServiceCallingMode.DirectlyToQueue, CallDirectlyToQueue);
        CallingMode.Add(ServiceCallingMode.ImmediateWithCheckResult, ImmediateWithCheckResult);
    }

    public async Task<SRResponse> CallProcessAsync(SRRequest request)
    {
        SRResponse response = new SRResponse();
        //Insert Request Log
        //Logger.Log(new Persistence.Log.Entities.RequestLog
        //{
        //    //TODO : Sensetive Data Must Be Removed
        //    Input = JsonConvert.SerializeObject(request.Input),
        //    PointerId = request.PointerId,
        //    Service_MethodName = request.MethodName.ToString(),
        //});

        try
        {
            //Check For Mock
            if (request.Mock != null)
            {
                return response = await MockResponse(request);
            }

            //Call Based On ServiceCallingMode
            return response = await CallingMode[request.CallingMode](request);

        }
        catch (Exception ex)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            return new SRResponse
            {
                ErrorCode = -100,
                Exception = new ExceptionDto
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                    // Set any other necessary properties
                },
                Message = "مشکل",
                Response = null,
                Success = false
            };
        }
        finally
        {
            //Insert ResponseLog
            //Logger.Log(new Persistence.Log.Entities.ResponseLog
            //{
            //    //TODO : Sensetive Data Must Be Removed
            //    Input = JsonConvert.SerializeObject(request.Input),
            //    Output = JsonConvert.SerializeObject(response),
            //    PointerId = request.PointerId,
            //    Service_MethodName = request.MethodName.ToString(),
            //});
        }


    }

    private async Task<SRResponse> CallImmediate(SRRequest request)
    {
        var ClassInstance = GetClassInstance(request);
        var result = await ClassInstance.GetResponse(request.Input);
        return new SRResponse
        {
            ErrorCode = 0,
            Exception = null,
            Message = "Ok.",
            Response = JsonConvert.SerializeObject(result),
            Success = true
        };
    }
    private async Task<SRResponse> CallQueueOnFaild(SRRequest request)
    {
        var ClassInstance = GetClassInstance(request);
        try
        {
            var result = await ClassInstance.GetResponse(request.Input);
            if (request.CheckResult != null)
            {
                RestResponse_VM<CheckResultResponse> CheckResult = CallCheckResponse(new CheckResultRequest { Success = true, Exception = null, Response = result, MethodName = request.MethodName });

                if (!CheckResult.IsSuccess) throw new Exception(); //TODO

                if (CheckResult.GetResponse().Success)
                {
                    goto Success;
                }

                await InsertInQueue(request);
            }
        //Send For Check Result
        Success:
            return new SRResponse
            {
                ErrorCode = 0,
                Exception = null,
                Message = "Ok.",
                Response = JsonConvert.SerializeObject(result),
                Success = true
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

        Success:
            return new SRResponse
            {
                ErrorCode = 0,
                Exception = null,
                Message = "Ok.",
                Response = JsonConvert.SerializeObject(ex),
                Success = true
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
        var result = await ClassInstance.GetResponse(request.Input);
        if (request.CheckResult != null)
        {
            RestResponse_VM<CheckResultResponse> CheckResult = CallCheckResponse(new CheckResultRequest { Success = true, Exception = null, Response = result, MethodName = request.MethodName });
            if (!CheckResult.IsSuccess) throw new Exception(); //TODO
            if (CheckResult.GetResponse().Success)
            {
                goto Success;
            }
        }
    Success: return new SRResponse
    {
        ErrorCode = 0,
        Exception = null,
        Message = "Ok.",
        Response = JsonConvert.SerializeObject(result),
        Success = true
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
                Success = true,
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
                Success = false,
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



