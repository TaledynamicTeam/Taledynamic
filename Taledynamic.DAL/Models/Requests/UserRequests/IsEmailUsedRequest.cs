using System.ComponentModel.DataAnnotations;
using System.Text;
using Taledynamic.DAL.Models.Internal;

namespace Taledynamic.DAL.Models.Requests.UserRequests
{
    public class IsEmailUsedRequest: BaseRequest
    {
        [Required]
        public string Email { get; set; }

        public override ValidateState IsValid()
        {
            StringBuilder sb = new StringBuilder();
            if (Email == null)
            {
                sb.Append("Email is default.");
            }

            if (sb.Length != 0)
            {
                return new ValidateState(false, sb.ToString());
            }

            return new ValidateState(true, "Success");
        }
    }
}