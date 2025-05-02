using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class ListUploads : PermissionBase
    {
        public bool CanDownLoadErrors { get; }

        public ListUploads() { }

        public ListUploads(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, 
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Basic:
                    CanDownLoadErrors = false;
                    break;
                case UserRoles.Admin:
                case UserRoles.Standard:
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanDownLoadErrors = true;
                    break;

                default:
                    CanDownLoadErrors = false;
                    break;
            }
        }
    }
}
