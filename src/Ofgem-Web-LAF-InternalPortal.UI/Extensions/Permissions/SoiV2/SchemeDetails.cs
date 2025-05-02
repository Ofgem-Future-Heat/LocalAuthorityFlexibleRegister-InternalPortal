using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions.SoiV2
{
    public class SchemeDetails : PermissionBase
    {
        public bool CanContinue { get; }

        public SchemeDetails() { }

        public SchemeDetails(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData) : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanContinue = Role switch
            {
                UserRoles.Basic => false,
                UserRoles.Standard => false,
                UserRoles.Advanced => true,
                UserRoles.Expert => true,
                UserRoles.Admin => false,
                _ => false
            };
        }
    }
}
