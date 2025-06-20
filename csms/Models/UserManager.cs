using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Collections;

namespace csms.Models
{
    public class UserManager
    {
        private IConfiguration Configuration;

        public UserManager(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task SignIn(HttpContext httpContext, UserModel user, bool isPersistent = false)
        {
            try
            {
                IEnumerable cfgUsers = Configuration.GetSection("Users").GetChildren();

                foreach (ConfigurationSection cfgUser in cfgUsers)
                {
                    if (cfgUser.GetValue<string>("Username") == user.Username &&
                        cfgUser.GetValue<string>("Password") == user.Password)
                    {
                        user.IsAdmin = cfgUser.GetValue<bool>(Models.Constants.AdminRoleName);
                        ClaimsIdentity identity = new ClaimsIdentity(this.GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
                        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        break;
                    }
                }

            }
            catch //(Exception exp)
            {
            }
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync();
        }

        private IEnumerable<Claim> GetUserClaims(UserModel user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Username));
            claims.Add(new Claim(ClaimTypes.Name, user.Username));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }

        private IEnumerable<Claim> GetUserRoleClaims(UserModel user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Username));
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, Models.Constants.AdminRoleName));
            }
            return claims;
        }
    }
}
