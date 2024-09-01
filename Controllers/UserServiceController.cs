using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Testproject.Models;
using Testproject.Models.Encryption;
using Testproject.Models.EntityModel;
using Testproject.Models.KeyStore;
namespace Testproject.Controllers
{
    public class UserServiceController:Controller
    {
        private AppDbContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;
        public UserServiceController(AppDbContext context, JwtTokenGenerator tokenGenerator) { 
            _context = context;
            _tokenGenerator = tokenGenerator;

        }
        public IActionResult Home(string returnUrl,string AuthToken)
        {
            ViewBag.Title = "Home";
            ViewData["AuthToken"] = AuthToken;
            return View("/Views/Home/HomePage.cshtml");
        }
        //[Authorize(Policy =AppKeyStore.AdminOnly)]
        public IActionResult Users()
        {
            ViewBag.Title = "Users";
            //ViewBag.Users = getAllUsers();
            return View("/Views/Home/UsersDetail.cshtml");
        }
        [Authorize(Policy =AppKeyStore.UserOnly)]
        public IActionResult AccountDetail()
        {
            ViewBag.Title = "Account detail";
            var user = new UserModel();
            user.Name=User.Identity.Name;
            user.Email=User.FindFirst(ClaimTypes.Email)?.Value;
            user.RoleName=User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.Users = new List<UserModel> { user };
            return View("/Views/Home/UserDetail.cshtml");
        }
        public IActionResult Login(string returnUrl)
        {
            ViewBag.Title = "Login";
            return View("/Views/Home/Login.cshtml");
        }
        public IActionResult Register(string returnUrl)
        {
            ViewBag.Title = "Register";
            return View("/Views/Home/Register.cshtml");
        }
        [HttpPost]
        public IActionResult Register(UserModel model)
        {
            ViewBag.Title = "Register";
            model.RoleName = AppKeyStore.User;
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError("", "Password is not matching;");
                else
                {
                    bool status = RegisterUser(model);
                    if (!status)
                        ModelState.AddModelError("", "User already exists");
                    else
                        return RedirectToAction("Login");
                }

            }
            return View("/Views/Home/Register.cshtml",model);
        }
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            ViewBag.Title = "Login";
            // model checked all the condition is satisfied
            if (ModelState.IsValid)
            {
                if (isLoginIn(model.Email, model.Password))
                {
                    var user = UpdateUser(model);
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name,user.Name),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role,user.RoleName)
                    };
                    var token = _tokenGenerator.GenerateToken(claims);
                    // Set token in a cookie
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(2),
                        SameSite = SameSiteMode.None
                    };

                    Response.Cookies.Append("AuthToken", token, cookieOptions);
                    // login sucess redirected to home page
                    return RedirectToAction("Home", new { AuthToken = token});
                }
                else
                {
                    // adding custom error message when the login failed
                    ModelState.AddModelError(string.Empty, "User name and password not matched...");
                }
            }
            // redirected to view and model with error message to redisplay
            return View("/Views/Home/Login.cshtml",model);
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok();
        }
        private bool isLoginIn(string email,string password) {
         
            var data= DataClass.Details.SingleOrDefault(a => a.Email == email);
            if (data==null)
            {
                return false;
            }
            // to verify the password same salt should be passed so taking user data and checking password by encrypting
            return Argon2Utils.VerifyPassword(password, data.PasswordHash, data.Salt);
        }
        private bool RegisterUser(UserModel model) {

            if (DataClass.Details.Any(a => a.Email == model.Email))
                return false;
            
            var hasPassWord = Argon2Utils.HashPassword(model.Password);
            DataClass.Details.Add(new UserDetails()
            {
                Id= DataClass.Details.Count()+1,
                Name = model.Name,
                Email = model.Email,
                PasswordHash = hasPassWord.hashPassword,
                Salt = hasPassWord.salt
            });

            // Role should be decide in different way here for practice role as been decided
            string role = DataClass.Details.Count() % 2 == 0 ? AppKeyStore.User : AppKeyStore.Admin;

            int roleId = DataClass.Roles.FirstOrDefault(f=> f.Name.Equals(role)).Id;
            

            DataClass.UserRoles.Add(new UserRoles
            {
                UserId = DataClass.Details.Count(),
                RoleId = roleId
            });
            return true;
        }
        private UserModel UpdateUser(LoginModel model)
        {
            var userData = from detail in DataClass.Details
                           join role in DataClass.UserRoles
                           on detail.Id equals role.UserId
                           join roles in DataClass.Roles
                           on role.RoleId equals roles.Id
                           where detail.Email  == model.Email
                           select new UserModel
                           {
                               RoleName = roles.Name,
                               Id = detail.Id,
                               Name = detail.Name,
                               Email = detail.Email
                           };

            var user = DataClass.Details
                .Join(DataClass.UserRoles,
                d=> d.Id,
                r=> r.UserId,
                (d,r)=> new { d, r })
                .Join(DataClass.Roles,
                combine=> combine.r.RoleId,
                roles=> roles.Id, 
                (combine, roles) =>
                new UserModel {
                    Id=combine.r.UserId,
                    Name =combine.d.Name,
                    Email =combine.d.Email,
                    RoleName =roles.Name
                })
                .ToList();
              
            return userData.FirstOrDefault();
        }

        [Authorize(Roles = AppKeyStore.Admin)]
        public List<UserModel> getAllUsers()
        {
            var users = from user in DataClass.Details
                        join userRoles in DataClass.UserRoles
                        on user.Id equals userRoles.UserId
                        join roles in DataClass.Roles
                        on userRoles.RoleId equals roles.Id
                        select new UserModel
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            RoleName = roles.Name
                        };
            return users.ToList();

        }
    }
}
