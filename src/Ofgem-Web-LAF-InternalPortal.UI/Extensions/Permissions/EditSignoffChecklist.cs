using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class EditSignoffChecklist : PermissionBase
    {
        public bool CanSave { get; }

        public EditSignoffChecklist() { }

        public EditSignoffChecklist(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanSave = Role switch
            {
                UserRoles.Basic => false,
                UserRoles.Standard => false,
                UserRoles.Advanced => true,
                UserRoles.Expert => true,
                UserRoles.Admin => false,
                _ => false
            };
        }
    }
}
