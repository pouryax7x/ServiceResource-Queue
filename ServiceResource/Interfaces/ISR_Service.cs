using ServiceResource.Dto;

namespace ServiceResource.Interfaces
{
    public interface ISR_Service
    {
        public Task<SRResponse> CallProcessAsync(SRRequest request , string InitialSource = "Application");
    }
}
