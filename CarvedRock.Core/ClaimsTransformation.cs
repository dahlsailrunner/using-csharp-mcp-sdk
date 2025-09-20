using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace CarvedRock.Core;

public class AdminClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {        
        var emailClaim = principal.Claims.FirstOrDefault(c => 
            c.Type == ClaimTypes.Email || 
            c.Type == "email");
            
        if (emailClaim != null &&
            emailClaim.Value.Split('@')[0].Equals("bobsmith", StringComparison.OrdinalIgnoreCase))
        {
            var clone = principal.Clone();
            var newIdentity = (ClaimsIdentity)clone.Identity!;
            
            newIdentity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            
            return Task.FromResult(clone);
        }

        return Task.FromResult(principal);
    }
}