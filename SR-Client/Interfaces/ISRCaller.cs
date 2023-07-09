using ServiceResource.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_Client.Interfaces
{
    public interface ISRCaller
    {
        public Task<SRResponse> CallSR(SRRequest request, TimeSpan timeout);
    }
}
