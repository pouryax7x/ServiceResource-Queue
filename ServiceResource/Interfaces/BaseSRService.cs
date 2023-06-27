using ServiceResource.Enums;

namespace ServiceResource.Interfaces
{
    public abstract class BaseSRService
    {
        private ServiceType ServiceType { get; set; }
        protected BaseSRService()
        {

        }
        private string serviceUrl { get; set; }
        public async Task<bool> CheckServiceAvailibility()
        {
            if (ServiceType == ServiceType.Soap)
            {
                return await CheckSoapServiceAvailability();
            }
            return await CheckRestServiceAvailability();
        }

        private async Task<bool> CheckRestServiceAvailability()
        {
            // TODO
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, serviceUrl);
                HttpResponseMessage response = await client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
        }

        private async Task<bool> CheckSoapServiceAvailability()
        {
            // TODO
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(serviceUrl + "?wsdl");
                return response.IsSuccessStatusCode;
            }
        }

        public abstract Task<object> GetResponse(object input);
    }
}
