using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PowerPredictor.Models
{
    public class User : IdentityUser
    {
        [EmailAddress]
        override public string Email { get; set; }
    }
}
