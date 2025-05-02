using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    public class SoiSummaryView : PermissionBase
    {
        public bool CanDoSomething { get; }

        public SoiSummaryView() { }

        public SoiSummaryView(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanDoSomething = Role switch
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
