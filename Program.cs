using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Testproject.MiddleWare;
using Testproject.Models;
using Testproject.Models.Encryption;
using Testproject.Models.KeyStore;

namespace Testproject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);

            var multiSchemePolicy = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme)
                      .RequireAuthenticatedUser()
                      .Build();

            builder.Services.AddAuthorization(o => o.DefaultPolicy = multiSchemePolicy);
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<CustomLoggingMiddleware>();
            app.UseRouting();
            app.UseCors(options =>
            {
                options.WithOrigins("https://localHost:7125");
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.AllowCredentials();
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=UserService}/{action=Login}/{id?}");

            app.Run();
        }
        public static void ConfigureServices(WebApplicationBuilder builder)
        {

            //Configure of DB context
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("myConnection"))
            );

            //Configure of identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Configure JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };

                })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "AuthToken";
                options.LoginPath = "/UserService/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie expiration
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            // Add authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AppKeyStore.AdminOnly, policy => policy.RequireRole(AppKeyStore.Admin));
                options.AddPolicy(AppKeyStore.UserOnly, policy => policy.RequireRole(AppKeyStore.User));
            });
            // Dependency injection for the JWT Token generate class
            builder.Services.AddSingleton<JwtTokenGenerator>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new JwtTokenGenerator(
                    configuration["Jwt:Key"],
                    configuration["Jwt:Issuer"],
                    configuration["Jwt:Audience"]
                );
            });
            builder.Services.AddCors();
        }
    }
}
