using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;


namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
    )]
    public class DeclarationDetailEditCompletedModel : PageModel
    {
        private readonly ILogger<DeclarationDetailEditCompletedModel> _logger;

        [BindProperty] public string DeclarationId { get; set; }
        [BindProperty] public bool HasDeclarationId => !string.IsNullOrEmpty(DeclarationId);

        [BindProperty] public string Urn { get; set; }

        public DeclarationDetailEditCompletedModel(
            ILogger<DeclarationDetailEditCompletedModel> logger)
        {
            _logger = logger;

            DeclarationId = string.Empty;
            Urn = string.Empty;
        }

        public Task OnGetWithIdAndUrn(string declarationId, string urn)
        {
            _logger.LogInformation("DeclarationDetailEditCompletedModel - OnGetWithId");

            DeclarationId = declarationId;
            Urn = urn;
            
            return Task.CompletedTask;
        }
    }
}
