using Taledynamic.Core.Models.Internal;

namespace Taledynamic.Core.Models.Requests.WorkspaceRequests
{
    public class GetWorkspacesByUserRequest: BaseRequest
    {
        public override ValidateState IsValid()
        {
            return new ValidateState(true, "Success.");
        }
    }
}