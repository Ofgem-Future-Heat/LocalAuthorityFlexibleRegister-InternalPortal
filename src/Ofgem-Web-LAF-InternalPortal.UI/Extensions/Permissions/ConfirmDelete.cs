using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class ConfirmDelete : PermissionBase
    {
        public bool CanDelete { get; set; }

        public ConfirmDelete() { }

        public ConfirmDelete(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, 
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Admin:
                case UserRoles.Standard:
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanDelete = true;
                    break;

                default:
                    break;
            }
        }
    }
}
