using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions.SoiV2
{
    public class EditDesignatedLas : PermissionBase
    {
        public bool CanUpdate { get; init; }

        public EditDesignatedLas() { }

        public EditDesignatedLas(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {

                case UserRoles.Advanced:
                    CanUpdate = true;
                    break;

                case UserRoles.Expert:
                    CanUpdate = true;
                    break;

                default:
                    CanUpdate = false;
                    break;
            }
        }
    }
}
