using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamsCX.WFM.API.Services
{
    public interface IUpToNowService
    {
        Task<Models.UpToNowResponse> GetUpToNowDataAsync(List<string> callQueues);
    }
}