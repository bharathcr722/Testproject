using Testproject.Models.EntityModel;
using Testproject.Models.KeyStore;

namespace Testproject.Models
{
    public static class DataClass
    {
        public static List<Roles> Roles = new List<Roles>()
        {
            new Roles{Id=1,Name=AppKeyStore.Admin},
            new Roles{Id=2,Name=AppKeyStore.User}
        };
        public static List<UserDetails> Details = new List<UserDetails>();
        public static List<UserRoles> UserRoles = new List<UserRoles>();

    }
}
