using Taledynamic.DAL.Models.Internal;

namespace Taledynamic.DAL.Models.Requests.TelegramRequests
{
    public class TelegramRevokeRequest: BaseRequest
    {
        public int Id { get; set; }
        public override ValidateState IsValid()
        {
            throw new System.NotImplementedException();
        }
    }
}