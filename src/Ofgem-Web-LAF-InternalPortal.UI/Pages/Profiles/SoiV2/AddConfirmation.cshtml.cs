using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem_Web_LAF_InternalPortal.Services;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class AddSoiV2Confirmation(
        ILogger<AddSoiV2Confirmation> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILaManagementService laManagementService) : PageModel
    {
        [BindProperty] public StatementOfIntentV2? StatementOfIntentModel { get; set; }

        [BindProperty] public Extensions.Permissions.SoiV2.AddConfirmation Permissions { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        public async Task OnGetById(Guid statementOfIntentId)
        {
            logger.LogInformation("AddSoiV2Confirmation - OnGet");

            Permissions = new Extensions.Permissions.SoiV2.AddConfirmation(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData(statementOfIntentId);
        }

        private async Task RefreshData(Guid statementOfIntentId)
        {
            try
            {
                StatementOfIntentModel = await laManagementService.GetSoiById(statementOfIntentId)
                                         ?? throw new ArgumentNullException($"{statementOfIntentId} does not exist");
            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving SOI data, {ex.Message}";
                logger.LogLafError(LogEvents.GetSoi, DisplayMessage);
            }
        }
    }
}
