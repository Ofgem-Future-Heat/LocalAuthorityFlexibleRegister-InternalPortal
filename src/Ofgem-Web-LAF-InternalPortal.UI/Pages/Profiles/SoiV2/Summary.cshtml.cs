using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class SoiSummary(
        ILogger<SoiSummary> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        [BindProperty] public Extensions.Permissions.SoiV2.SummaryView Permissions { get; set; } = new();

        protected override string PageName => "SoiSummary";

        public async Task OnGetById(Guid statementOfIntentId)
        {
            Permissions = new Extensions.Permissions.SoiV2.SummaryView(HttpContextAccessor, HttpClientFactory, TempData);

            await SetPageData(statementOfIntentId);
        }

        public async Task<IActionResult> OnPostSave()
        {
            Logger.LogInformation("{PageName} -  OnPostSave", PageName);

            Permissions = new Extensions.Permissions.SoiV2.SummaryView(HttpContextAccessor, HttpClientFactory, TempData);

            var soiLogModel = new Ofgem.LAF.SharedLibrary.Models.StatementOfIntentLog
            {
                StatementOfIntentId = new Guid(StatementOfIntentId),
                LocalAuthorityName = LocalAuthorityName ?? string.Empty,
                CreatedDate = DateTime.Now,
                Comment = $"SOI status changed to {SoiStatus}"
            };

            var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            var httpSoiLogResponseMessage =
                await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteAddSoiLog, soiLogModel);

            if (!httpSoiLogResponseMessage.IsSuccessStatusCode)
            {
                Logger.LogInformation("{PageName} -  Unable to save log for Status set at Summary page.", PageName);
                var problem = await httpSoiLogResponseMessage.Content
                    .ReadFromJsonAsync<ProblemDetails>();

                if (problem != null)
                {
                    ShowNotification = true;

                    if (problem.Detail != null)
                    {
                        DisplayMessage = problem.Detail;
                    }
                }
            }

            return await Task.FromResult<IActionResult>(RedirectToPage(
                LafPages.SoiV2.SoiConfirmationV2.ROUTE,
                LafPages.SoiV2.SoiConfirmationV2.METHOD_GET_BY_ID,
                new
                {
                    statementOfIntentId = StatementOfIntentId,
                    localAuthorityName = LocalAuthority.Name ?? LocalAuthorityName ?? string.Empty,
                }));
        }


        public async Task<IActionResult> OnPostCancel(string statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostCancel", PageName);

            Permissions = new Extensions.Permissions.SoiV2.SummaryView(HttpContextAccessor, HttpClientFactory, TempData);

            var success = await this.SaveFullResetData(new Guid(statementOfIntentId));

            if (!success)
            {
                return Page();
            }


            return RedirectToPage(
                LafPages.SoiV2.SoiDetails.ROUTE,
                LafPages.SoiV2.SoiDetails.METHOD_GET_BY_ID,
                new { StatementOfIntentId = StatementOfIntentId });
        }




        private async Task<bool> SaveFullResetData(Guid statementOfIntentId)
        {
            try
            {
                var statementOfIntent = await GetSoi(statementOfIntentId);

                if (statementOfIntent == null) return false;

                // updating defaults
                statementOfIntent.InternalNotes = null;
                statementOfIntent.Category = 0;
                statementOfIntent.IsRoute4SapBandsCorrect = null;

                // Statement of Intent details
                statementOfIntent.IsPreviousVersionStatusClear = null;
                statementOfIntent.IsPublishedDateCorrect = null;

                // Initial assessment checklist
                statementOfIntent.IsSoiSameAsWebsite = null;
                statementOfIntent.HasMostRecentTemplate = null;
                statementOfIntent.IsPreviousVersionStatusClear = null;
                statementOfIntent.IsLocalAuthorityNamed = null;
                statementOfIntent.IsDelegatedAuthorityNamed = null;

                // Scheme details checklist
                statementOfIntent.ForScheme = ForSchemeEnum.NoScheme;
                statementOfIntent.IsPublishedDateCorrect = null;
                statementOfIntent.IsProxy5PartOfRoute2 = null;

                // Eligibility for Route 2 Proxy 5
                statementOfIntent.IsProxy5SchemePresent = null;
                statementOfIntent.IsDescriptionNiceNg6Recomendation2 = null;

                // Recommended SOI status and additional comments
                statementOfIntent.Status = SoiStatusV2.ToBeAssessed;
                statementOfIntent.AssessmentNotes = [];

                // reset the soi
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpPutResponseMessage =
                    await httpClient.PutAsJsonAsync(
                        Services.LocalAuthorityApi.RoutePutUpdateBySoiId,
                        statementOfIntent);

                if (httpPutResponseMessage.IsSuccessStatusCode)
                {
                    return true;
                }

                var problem = await httpPutResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

                if (problem is not { Detail: not null }) return false;

                DisplayMessage = problem.Detail;
                ShowNotification = true;
                Logger.LogInformation(message: "{PageName} -  OnPostCancel - {DisplayMessage}", PageName, DisplayMessage);

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{PageName} -  OnPostCancel, Error", PageName);
                Message = $"An issue occurred cancelling the changes to soi: {statementOfIntentId}.";
                throw;
            }
        }
    }
}
