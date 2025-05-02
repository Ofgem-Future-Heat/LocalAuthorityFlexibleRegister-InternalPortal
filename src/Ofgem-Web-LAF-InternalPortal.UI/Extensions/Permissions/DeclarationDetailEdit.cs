using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class DeclarationDetailEdit : PermissionBase
    {
        public bool CanSubmit { get; }

        public DeclarationDetailEdit() { }

        public DeclarationDetailEdit(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanSubmit = true;
                    break;

                default:
                    CanSubmit = false;
                    break;
            }
        }
    }
}
