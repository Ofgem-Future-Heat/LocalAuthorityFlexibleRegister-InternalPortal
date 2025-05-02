using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;
using System.Data;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    public class Logs : PermissionBase
    {
        public bool CanDownLoadErrors { get; }

        public Logs() { }

        public Logs(IHttpContextAccessor httpContextAccessor,
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
