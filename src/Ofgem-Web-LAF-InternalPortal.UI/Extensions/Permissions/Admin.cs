using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class Admin : PermissionBase
    {
        public bool CanAddAnnouncement { get; }

        public Admin() { }

        public Admin(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanAddAnnouncement = Role switch
            {
                UserRoles.Basic => false,
                UserRoles.Standard => false,
                UserRoles.Advanced => true,
                UserRoles.Expert => true,
                UserRoles.Admin => true,
                _ => false,
            };
        }
    }
}
