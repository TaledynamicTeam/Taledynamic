using System.ComponentModel.DataAnnotations;

namespace Taledynamic.Core.Models.Requests.UserRequests
{
    public class CreateUserRequest: BaseRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}