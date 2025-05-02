using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{ 
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class DuplicateEntries : PermissionBase
    {
        public bool CanSaveAndContinue { get; }

        public DuplicateEntries(){}

        public DuplicateEntries(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            CanSaveAndContinue = Role switch
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
