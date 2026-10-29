using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CodeLearn.Helpers
{
    public class AppUserClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<AppUser, IdentityRole<int>>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email  ?? ""));
            identity.AddClaim(new Claim("picture",         user.AvatarUrl));
            identity.AddClaim(new Claim("nickname",        user.NickName));
            return identity;
        }
    }
}
