using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    public class PermissionBase
    {
        public Guid UserId { get; protected init; }
        public string Name { get; set; } = "** UNKNOWN **";
        public string Role { get; } = string.Empty;

        protected PermissionBase() { }

        /// <summary>
        /// Gets the USER details and sets the appropriate values
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="tempData">The ROLE is added to the TEMPDATA which is used to set the navigation menus</param>
        /// <exception cref="ArgumentException"></exception>
        protected PermissionBase(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, ITempDataDictionary tempData)
        {
            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            if (httpContextAccessor.HttpContext.User is null) throw new ArgumentException("Unknown user");
            if (httpContextAccessor.HttpContext.User.Identity is null) throw new ArgumentException("Unknown user");

            var userData = new Models.User(httpContextAccessor.HttpContext.User, httpClientFactory).Data;

            UserId = userData.UserId;
            Role = userData.Role ?? string.Empty;
            Name = httpContextAccessor.HttpContext.User.Identity.Name ?? "** UNKNOWN **";

            tempData[LafPageMenus.TEMP_DATA_ROLE_KEY] = Role;

            if (httpContextAccessor.HttpContext.User.Identity is not ClaimsIdentity claimsIdentity) return;

            foreach (var claim in claimsIdentity.Claims)
            {
                if (claim.Type.EndsWith("name"))
                {
                    Name = claim.Value;
                }
            }
        }
    }
}
