using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class EligibilityRoute2Proxy5(
        ILogger<EligibilityRoute2Proxy5> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        [BindProperty] public Extensions.Permissions.SoiV2.EligibilityRoute2Proxy5 Permissions { get; set; } = new();

        protected override string PageName => "EligibilityRoute2Proxy5";

        public async Task OnGet(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.EligibilityRoute2Proxy5(HttpContextAccessor, HttpClientFactory, TempData);

            await SetPageData(statementOfIntentId);
        }

        public async Task<IActionResult> OnPostSaveContinue(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveContinue", PageName);

            Permissions = new Extensions.Permissions.SoiV2.EligibilityRoute2Proxy5(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidRoute2Proxy5Page())
            {
                return Page();
            }

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            return RedirectToPage(
                LafPages.SoiV2.StatusSetting.ROUTE,
                LafPages.SoiV2.StatusSetting.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        public async Task<IActionResult> OnPostSaveExit(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveExit", PageName);

            Permissions = new Extensions.Permissions.SoiV2.EligibilityRoute2Proxy5(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidRoute2Proxy5Page())
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

                // updating with new values of eligibility route 2 page
                statementOfIntent.IsProxy5SchemePresent = IsProxy5SchemePresent;
                statementOfIntent.IsDescriptionNiceNg6Recomendation2 = IsProxy5SchemePresent == false ? null : IsDescriptionNiceNg6Recommendation2;



                // update the latest soi
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpPutResponseMessage =
                    await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RoutePutEligibilityRoute2Proxy5, statementOfIntent);

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
                Message = $"An issue occurred retrieving the data local authority by onsCode {statementOfIntentId}.";
                throw;
            }
        }
    }
}
