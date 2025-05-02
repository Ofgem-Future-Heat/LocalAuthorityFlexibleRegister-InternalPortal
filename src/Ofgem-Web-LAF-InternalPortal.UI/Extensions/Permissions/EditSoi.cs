using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class EditSoi : PermissionBase
    {
        public bool CanSaveSoi { get; init; }
        public bool CanViewValidSoiStatus { get; init; }

        public EditSoi() { }

        public EditSoi(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {

                case UserRoles.Advanced:
                    CanSaveSoi = true;
                    CanViewValidSoiStatus = false;
                    break;

                case UserRoles.Expert:
                    CanSaveSoi = true;
                    CanViewValidSoiStatus = true;
                    break;

                default:
                    CanSaveSoi = false;
                    CanViewValidSoiStatus = false;
                    break;
            }
        }
    }
}
