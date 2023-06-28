using ServiceResource.Enums;
using ServiceResource.Interfaces;
using YouRest;
using YouRest.Interface.Body;

namespace ServiceResource.Services
{
    public class Service1_Method1 : BaseSRService
    {
        public Service1_Method1()
        {
                
        }
        public override async Task<object> GetResponse(object input , int timeout)
        {
            RestCaller restCaller = new RestCaller(new RestStaticProperties
            {
                BaseAddress = "http://localhost:5005/",
                Timeout = TimeSpan.FromSeconds(timeout)
            });
            var caller = restCaller.CallRestService<object>(new RestRequest_VM
            {
                Body = new JsonBody(input),
                ReletiveAddress = "/api/TestSR/GetSalamData",
                HttpMethod = HttpMethod.Post,
                EnsureSuccessStatusCode = true
            });
            return caller.GetResponse();
        }
    }
}
