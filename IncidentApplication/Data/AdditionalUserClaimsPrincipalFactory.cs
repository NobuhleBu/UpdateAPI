using IdentityModel;
using IncidentApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IncidentApplication.Data
{
    public class AdditionalUserClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IOptions<IdentityOptions> options) 
            : base(userManager, roleManager, options)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Role, user.Role)
            };
            identity.AddClaims(claims);
            return principal;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Role, user.Role)
            };
            identity.AddClaims(claims);
            return await base.GenerateClaimsAsync(user);
        }
    }
}
