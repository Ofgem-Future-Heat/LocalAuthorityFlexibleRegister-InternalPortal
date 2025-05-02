using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class WithdrawnConfirmation(
        ILogger<WithdrawnConfirmation> logger) : PageModel
    {
        [BindProperty] public string StatementOfIntentId { get; set; } = string.Empty;
        [BindProperty] public string LocalAuthorityName { get; set; } = string.Empty;
        [BindProperty] public string Version { get; set; } = string.Empty;

        public async Task OnGetById(Guid statementOfIntentId, string localAuthorityName, string version)
        {
           await Task.Run(() =>
            {
                logger.LogInformation("WithdrawnConfirmation - OnGetById");

                StatementOfIntentId = statementOfIntentId.ToString();
                LocalAuthorityName = localAuthorityName;
                Version = version;
            });
        }
    }
}
