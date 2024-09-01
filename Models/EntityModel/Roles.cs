using System.ComponentModel.DataAnnotations;

namespace Testproject.Models.EntityModel
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
