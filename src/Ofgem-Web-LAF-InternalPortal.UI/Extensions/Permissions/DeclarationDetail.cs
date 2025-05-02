using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    /// <summary>
    /// Defines the actions allowed based upon the user
    /// </summary>
    public class DeclarationDetail : PermissionBase
    {
        public bool CanEditAssessment { get; }
        public bool CanSaveNote { get; }
        public bool CanDeleteNote { get; }
        public bool CanFixErrors { get; set; }

        public DeclarationDetail() { }

        public DeclarationDetail(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanEditAssessment = true;
                    CanSaveNote = true;
                    CanDeleteNote = true;
                    CanFixErrors = true;
                    break;

                default:
                    CanEditAssessment = false;
                    CanSaveNote = false;
                    CanDeleteNote = false;
                    CanFixErrors = false;
                    break;
            }
        }
    }
}
