using Microsoft.AspNetCore.Authorization;

namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Takes an array of strings containing role names
        /// </summary>
        /// <param name="roles"></param>
        public AuthorizeRolesAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
