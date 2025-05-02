using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class SchemeDetails(
        ILogger<SchemeDetails> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        protected override string PageName => "SchemeDetails";

        [BindProperty] public Extensions.Permissions.SoiV2.SchemeDetails Permissions { get; set; } = new();

        public async Task OnGet(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.SchemeDetails(HttpContextAccessor, HttpClientFactory, TempData);

            await SetPageData(statementOfIntentId);
        }

        public async Task<IActionResult> OnPostSaveContinue(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveContinue", PageName);

            Permissions = new Extensions.Permissions.SoiV2.SchemeDetails(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidDateSchemeDetailsPage())
            {
                return Page();
            }

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            if (IsProxy5PartOfRoute2 is false)
            {
                return RedirectToPage(
                    LafPages.SoiV2.StatusSetting.ROUTE,
                    LafPages.SoiV2.StatusSetting.METHOD_GET_BY_ID,
                    new { StatementOfIntentId = statementOfIntentId });
            }


            return RedirectToPage(
                LafPages.SoiV2.EligibilityRoute2Proxy5.ROUTE,
                LafPages.SoiV2.EligibilityRoute2Proxy5.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        public async Task<IActionResult> OnPostSaveExit(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveExit", PageName);

            Permissions = new Extensions.Permissions.SoiV2.SchemeDetails(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidDateSchemeDetailsPage())
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

                // updating with new values of schemeDetails page
                statementOfIntent.ForScheme = ForScheme;
                statementOfIntent.IsPublishedDateCorrect = IsPublishedDateCorrect;
                statementOfIntent.IsProxy5PartOfRoute2 = IsProxy5PartOfRoute2;

                // Update the latest Soi
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpPutResponseMessage =
                    await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RouteSchemeDetails, statementOfIntent);

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
                Message = $"An issue occurred saving the data by Statement of Intent Id {statementOfIntentId}.";
                throw;
            }
        }
    }
}

