using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class ExternalUser : PermissionBase
    {
        public bool CanAdd { get; }

        public bool CanDeactivate { get; }

        public bool CanEdit { get; }
        public bool CanReactivate { get; }

        public bool CanView { get; }

        public ExternalUser() { }

        public ExternalUser(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Admin:
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanAdd = true;
                    CanDeactivate = true;
                    CanEdit = true;
                    CanReactivate = true;
                    CanView = true;
                    break;
                default:
                    CanAdd = false;
                    CanDeactivate = false;
                    CanEdit = false;
                    CanReactivate = false;
                    CanView = false;
                    break;
            }
        }
    }
}
