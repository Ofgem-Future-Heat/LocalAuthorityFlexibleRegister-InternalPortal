using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions.SoiV2
{
    public class SoiDetails : PermissionBase
    {
        public bool CanAssess { get; }

        public bool CanWithdraw { get; }

        public SoiDetails() { }

        public SoiDetails(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanWithdraw = Role switch
            {
                UserRoles.Basic => false,
                UserRoles.Standard => false,
                UserRoles.Advanced => true,
                UserRoles.Expert => true,
                UserRoles.Admin => false,
                _ => false,
            };

            CanAssess = Role switch
            {
                UserRoles.Basic => false,
                UserRoles.Standard => false,
                UserRoles.Advanced => true,
                UserRoles.Expert => true,
                UserRoles.Admin => false,
                _ => false,
            };
        }
    }
}
