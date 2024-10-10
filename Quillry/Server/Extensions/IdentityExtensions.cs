using System.Security.Claims;

namespace Quillry.Server.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;
        }
    }
}
