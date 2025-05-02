using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class Dashboard : PermissionBase
    {
        public bool CanUpload { get; }
        public bool CanSelect { get; }
        public bool CanDownload { get; }

        public Dashboard() { }

        public Dashboard(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, ITempDataDictionary tempData)
        : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Admin:
                case UserRoles.Standard:
                    CanUpload = false;
                    CanSelect = true;
                    CanDownload = true;
                    break;

                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanUpload = true;
                    CanSelect = true;
                    CanDownload = true;
                    break;
                    
                default:
                    CanUpload = false;
                    CanSelect = false;
                    CanDownload = false;
                    break;
            }
        }
    }
}
