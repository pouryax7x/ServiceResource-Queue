using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using ServiceResource.Dto;
using SR_Client.Interfaces;
using YouRest;
using YouRest.Interface.Body;

namespace SR_Client
{
    public static class SRClient
    {
        public static string SRAddress = "";
        public static IServiceCollection AddSR(this IServiceCollection services, string srAddress)//Action<SROptionBuilder>? srOption = null
        {
            services.AddScoped<ISRCaller, SRCaller>();
            if (string.IsNullOrWhiteSpace(srAddress))
            {
                throw new Exception("Address cannout be empty.");
            }
            SRAddress = srAddress;
            return services;
        }
    }
    public class SRCaller : ISRCaller
    {
        private string SRAddress = "";
        public SRCaller()
        {
            SRAddress = SRClient.SRAddress;
        }
        public async Task<SRResponse> CallSR(SRRequest request, TimeSpan timeout)
        {
            RestCaller restCaller = new RestCaller(new RestStaticProperties
            {
                BaseAddress = SRAddress,
                Timeout = timeout
            });

            return restCaller.CallRestService<SRResponse>(new RestRequest_VM
            {
                Body = new JsonBody(request),
                EnsureSuccessStatusCode = true,
            }).GetResponse();
        }
    }
}