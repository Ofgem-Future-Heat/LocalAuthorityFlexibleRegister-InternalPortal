using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class SupersededDeclarationDetail : PermissionBase
    {
        public SupersededDeclarationDetail() { }

        public SupersededDeclarationDetail(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            
        }
    }
}
