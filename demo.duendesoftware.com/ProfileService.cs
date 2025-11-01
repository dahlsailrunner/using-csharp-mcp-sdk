using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityModel;
using IdentityServerHost;

namespace Duende.IdentityServer.Demo;

public class ProfileService : IProfileService
{
    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subjectId = context.Subject.Identity.GetSubjectId();
        var subjectClaims = TestUsers.Users.FirstOrDefault(a => a.SubjectId == subjectId);

        foreach (var req in context.RequestedClaimTypes)
        {
            var claim = subjectClaims?.Claims.FirstOrDefault(c => c.Type == req);
            if (claim != null)
            {
                context.IssuedClaims.Add(claim);
            }
        }

        // add actor claim if needed
        if (context.Subject.GetAuthenticationMethod() == OidcConstants.GrantTypes.TokenExchange)
        {
            var act = context.Subject.FindFirst(JwtClaimTypes.Actor);
            if (act != null)
            {                
                context.IssuedClaims.Add(act);
            }
        }        

        return Task.CompletedTask;
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}