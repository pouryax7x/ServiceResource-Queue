using Microsoft.AspNetCore.Mvc;
using ServiceResource.Business;
using ServiceResource.Dto;
using ServiceResource.Interfaces;

namespace ServiceResource.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QueueController : ControllerBase
    {
        public QueueController(IQueueHandler queueService)
        {
            QueueService = queueService;
        }
        public IQueueHandler QueueService { get; }

        [HttpPost]
        public async Task<IActionResult> DeleteAllQueueMembers(DeleteQueueMembersRequest request)
        {
            return Ok(await QueueService.DeleteAllQueueMembers(request));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQueueMember(DeleteQueueMemberRequest request)
        {
            return Ok(await QueueService.DeleteQueueMember(request));
        }
    }
}