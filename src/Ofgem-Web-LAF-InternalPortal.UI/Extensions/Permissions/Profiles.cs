using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class Profiles : PermissionBase
    {
        public bool CanAddProfile { get; }
        public bool CanEditProfile { get; }

        public Profiles() { }

        public Profiles(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory, 
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanAddProfile = true;
                    CanEditProfile = true;
                    break;

                default:
                    CanAddProfile = false;
                    CanEditProfile = false;
                    break;
            }
        }
    }
}
