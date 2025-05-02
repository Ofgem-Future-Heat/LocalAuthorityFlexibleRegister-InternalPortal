
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class ExternalUsers : PermissionBase
    {
        public bool CanAdd { get; }
        public bool CanDelete { get; }
        public bool CanEdit { get; }
        public bool CanView { get; }

        public ExternalUsers() { }

        public ExternalUsers(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Admin:
                case UserRoles.Expert:
                case UserRoles.Advanced:
                    CanAdd = true;
                    CanDelete = true;
                    CanEdit = true;
                    CanView = true;
                    break;

                default:
                    CanAdd = false;
                    CanDelete = false;
                    CanEdit = false;
                    CanView = false;
                    break;
            }
        }
    }
}
