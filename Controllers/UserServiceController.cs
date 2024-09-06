using Behaviour.Interface;
using Core.DataModel;
using Core.KeyStore;
using Core.QueryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Encryption;
using System.Security.Claims;
using Testproject.Models;

namespace Testproject.Controllers
{
    public class UserServiceController:Controller
    {
        private AppDbContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;
        private IUsersBehaviour _usersBehaviour;
        public UserServiceController(AppDbContext context, JwtTokenGenerator tokenGenerator, IUsersBehaviour usersBehaviour) { 
            _context = context;
            _tokenGenerator = tokenGenerator;
            _usersBehaviour = usersBehaviour;

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
        //[Authorize(Policy =AppKeyStore.UserOnly)]
        public IActionResult AccountDetail()
        {
            ViewBag.Title = "Account detail";
            var email=User.FindFirst(ClaimTypes.Email)?.Value;
            var user =  _usersBehaviour.GetUserDetails(new UserQueryModel() { Email = email });
            ViewBag.Users = new List<UserDetails> { user };

            return View("/Views/Home/UsersDetail.cshtml");
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
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError("", "Password is not matching;");
                else
                {
                    var status = _usersBehaviour.RegisterUser(model);
                    if (!status.Status)
                        ModelState.AddModelError("", status.StatusMessage);
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
                var result = _usersBehaviour.isLoginedIn(model.Email, model.Password);
                if (result.Status)
                {
                    var user = _usersBehaviour.GetUserDetails(new UserQueryModel { Email=model.Email});
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name,user.Name),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
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

        [Authorize(Roles = AppKeyStore.Admin)]
        public List<UserDetails> getAllUsers()
        {
            return _usersBehaviour.GetUserDetails();

        }
    }
}
