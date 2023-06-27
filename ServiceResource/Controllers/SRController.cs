using Microsoft.AspNetCore.Mvc;
using ServiceResource.Business;
using ServiceResource.Dto;
using ServiceResource.Interfaces;

namespace ServiceResource.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SRController : ControllerBase
    {
        public SRController(ISR_Service srService)
        {
            SrService = srService;
        }

        public ISR_Service SrService { get; }

        [HttpPost]
        public async Task<IActionResult> CallSR(SRRequest request)
        {
            return Ok(await SrService.CallProcessAsync(request));
        }
    }
}