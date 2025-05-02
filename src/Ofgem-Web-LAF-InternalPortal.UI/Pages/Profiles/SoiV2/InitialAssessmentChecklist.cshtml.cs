using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class InitialAssessmentChecklist(
    ILogger<InitialAssessmentChecklist> logger,
        IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor) : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        [BindProperty] public Extensions.Permissions.SoiV2.InitialAssessmentChecklist Permissions { get; set; } = new();

        protected override string PageName => "InitialAssessmentChecklist";

        public async Task OnGet(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.InitialAssessmentChecklist(HttpContextAccessor, HttpClientFactory, TempData);

            await SetPageData(statementOfIntentId);
        }

        public async Task<IActionResult> OnPostSaveContinue(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveContinue", PageName);

            Permissions = new Extensions.Permissions.SoiV2.InitialAssessmentChecklist(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidInitialAssessmentChecklistPage())
            {
                return Page();
            }

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            return RedirectToPage(
                LafPages.SoiV2.SchemeChecklist.ROUTE,
                LafPages.SoiV2.SchemeChecklist.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        public async Task<IActionResult> OnPostSaveExit(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveExit", PageName);

            Permissions = new Extensions.Permissions.SoiV2.InitialAssessmentChecklist(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidInitialAssessmentChecklistPage())
            {
                return Page();
            }

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }


            return RedirectToPage(
                LafPages.SoiV2.SoiDetails.ROUTE,
                LafPages.SoiV2.SoiDetails.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        private async Task<bool> SaveData(Guid statementOfIntentId)
        {
            try
            {
                var statementOfIntent = await GetSoi(statementOfIntentId);

                if (statementOfIntent == null) return false;

                // updating with new values of initial assessment page
                statementOfIntent.IsSoiSameAsWebsite = IsSoiSameAsWebsite;
                statementOfIntent.HasMostRecentTemplate = HasMostRecentTemplate;
                statementOfIntent.IsPreviousVersionStatusClear = IsPreviousVersionStatusClear;
                statementOfIntent.IsLocalAuthorityNamed = IsLocalAuthorityNamed;
                statementOfIntent.IsDelegatedAuthorityNamed = IsDelegatedAuthorityNamed;

                // update the latest soi
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpPutResponseMessage =
                    await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RoutePutInitialAssessmentChecklist, statementOfIntent);

                if (httpPutResponseMessage.IsSuccessStatusCode)
                {
                    return true;
                }

                var problem = await httpPutResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

                if (problem is not { Detail: not null }) return false;

                DisplayMessage = problem.Detail;
                ShowNotification = true;
                Logger.LogInformation(message: "{PageName} -  OnPostSave - {DisplayMessage}", PageName, DisplayMessage);

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{PageName} -  SaveData, Error", PageName);
                Message = $"An issue occurred retrieving the soi data soi id:{statementOfIntentId}.";
                throw;
            }
        }
    }
}
