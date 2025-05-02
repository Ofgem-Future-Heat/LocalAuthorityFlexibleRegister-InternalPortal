using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class AddAnnouncement : PermissionBase
    {
        public bool CanPublishAndContinue { get; }

        public AddAnnouncement() { }

        public AddAnnouncement(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, 
            ITempDataDictionary tempData) : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanPublishAndContinue = Role switch
            {
                UserRoles.Basic => false,
                UserRoles.Standard => false,
                UserRoles.Advanced => false,
                UserRoles.Expert => true,
                UserRoles.Admin => true,
                _ => false,
            };
        }
    }
}
