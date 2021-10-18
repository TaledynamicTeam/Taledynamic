using System.ComponentModel.DataAnnotations;

namespace Taledynamic.Core.Models.Requests.UserRequests
{
    public class IsEmailUsedRequest
    {
        [Required]
        public string Email { get; set; }
    }
}