using Microsoft.AspNetCore.Mvc;
using ServiceResource.Dto;
using ServiceResource.Enums;

namespace ServiceResource.Interfaces
{
    public interface IQueueHandler
    {
        public Task<bool> InsertInQueue(SRRequest request, int callCount);
        public Task<bool> InsertInQueueCallBack(string serilizedRequest, MethodName methodName, int callCount = 0);
        public Task<bool> DeleteAllQueueMembers(DeleteQueueMembersRequest request);
        public Task<bool> DeleteQueueMember(DeleteQueueMemberRequest request);
    }
}
