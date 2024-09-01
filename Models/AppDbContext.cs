using Microsoft.EntityFrameworkCore;
using Testproject.Models.EntityModel;

namespace Testproject.Models
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) 
        {

        }
        public DbSet<UserDetails> users { get; set; }
    }
}
