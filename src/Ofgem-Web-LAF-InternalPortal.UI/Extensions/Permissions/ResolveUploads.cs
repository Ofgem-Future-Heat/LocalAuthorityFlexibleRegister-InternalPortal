using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class ResolveUploads : PermissionBase
    {
        public bool CanResolveUploads { get; set; }

        public ResolveUploads() { }

        public ResolveUploads(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Basic:
                    break;
                case UserRoles.Admin:
                case UserRoles.Standard:
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanResolveUploads = true;
                    break;

                default:
                    break;
            }
        }
    }
}
