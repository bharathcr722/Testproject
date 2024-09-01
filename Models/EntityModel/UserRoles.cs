using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Testproject.Models.EntityModel
{
    public class UserRoles
    {
        [Key]
        public int RoleId { get; set; }
        [Key]
        public int UserId { get; set; }
    }
}
