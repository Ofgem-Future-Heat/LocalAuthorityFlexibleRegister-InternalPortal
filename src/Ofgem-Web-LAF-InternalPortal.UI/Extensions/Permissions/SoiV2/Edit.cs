using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions.SoiV2
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class Edit : PermissionBase
    {
        public bool CanContinue { get; init; }
        public bool CanViewValidSoiStatus { get; init; }

        public Edit() { }

        public Edit(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {

                case UserRoles.Advanced:
                    CanContinue = true;
                    CanViewValidSoiStatus = false;
                    break;

                case UserRoles.Expert:
                    CanContinue = true;
                    CanViewValidSoiStatus = true;
                    break;

                default:
                    CanContinue = false;
                    CanViewValidSoiStatus = false;
                    break;
            }
        }
    }
}
