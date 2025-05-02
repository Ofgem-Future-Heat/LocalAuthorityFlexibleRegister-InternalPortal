using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Services;
using System.Reflection.Emit;
using System;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class Withdraw(
        ILogger<Withdraw> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        [BindProperty] public Extensions.Permissions.SoiV2.Withdraw Permissions { get; set; } = new();

        protected override string PageName => "Withdraw";

        public async Task OnGetById(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Withdraw(HttpContextAccessor, HttpClientFactory, TempData);

            await SetPageData(statementOfIntentId);
        }

        public async Task<IActionResult> OnPostWithdraw(Guid statementOfIntentId, string localAuthorityName, string version)
        {
            Logger.LogInformation("{PageName} -  OnPostWithdraw", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Withdraw(HttpContextAccessor, HttpClientFactory, TempData);

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            return RedirectToPage(
                LafPages.SoiV2.WithdrawnConfirmation.ROUTE,
                LafPages.SoiV2.WithdrawnConfirmation.METHOD_GET_BY_ID,
                new
                {
                    statementOfIntentId,
                    localAuthorityName,
                    version = version
                });
        }

        private async Task<bool> SaveData(Guid statementOfIntentId)
        {
            try
            {
                var statementOfIntent = await GetSoi(statementOfIntentId);

                if (statementOfIntent == null) return false;


                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpPutResponseMessage = await UpdateSoiStatusToWithdrawn(statementOfIntent, httpClient);

                if (httpPutResponseMessage.IsSuccessStatusCode)
                {
                    await SendEmails(statementOfIntent);

                    await AuditSoiStatusChange(statementOfIntent, httpClient);

                    return true;
                }

                var problem = await httpPutResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

                if (problem is not { Detail: not null }) return false;

                DisplayMessage = problem.Detail;
                ShowNotification = true;
                ShowForm = false;
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

        private async Task<bool> AuditSoiStatusChange(StatementOfIntent statementOfIntent, HttpClient httpClient)
        {
            var soiLogModel = new Ofgem.LAF.SharedLibrary.Models.StatementOfIntentLog
            {
                StatementOfIntentId = new Guid(StatementOfIntentId),
                LocalAuthorityName = LocalAuthorityName ?? string.Empty,
                CreatedDate = DateTime.Now,
                Comment = $"SOI status changed to {statementOfIntent.Status}"
            };

            HttpResponseMessage httpSoiLogResponseMessage =
                await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteAddSoiLog, soiLogModel);

            if (!httpSoiLogResponseMessage.IsSuccessStatusCode)
            {
                Logger.LogInformation("{PageName} -  Unable to save log for Withdrawn status set", PageName);
                ProblemDetails? logProblem = await httpSoiLogResponseMessage.Content
                    .ReadFromJsonAsync<ProblemDetails>();

                if (logProblem != null)
                {
                    ShowNotification = true;

                    if (logProblem.Detail != null)
                    {
                        DisplayMessage = logProblem.Detail;
                    }
                }
            }

            return true;
        }

        private async Task SendEmails(StatementOfIntent statementOfIntent)
        {
            logger.LogInformation("Withdrawn - started");

            var httpClientUser = httpClientFactory.CreateClient(UserApi.ApiName);

            var requestSoiStatusEmails = await httpClientUser.PostAsJsonAsync(
                $"{UserApi.RouteGetExternalUser}/{statementOfIntent.OnsCode}/{statementOfIntent.LocalAuthority.Name}/{statementOfIntent.VersionNumber}/{UserApi.RouteNotifyByEmailSoiStatusChange}?status={(int)SoiStatusV2.Withdrawn}",
                "{}");

            if (requestSoiStatusEmails.IsSuccessStatusCode)
            {
                logger.LogInformation("Withdrawn - Emailing - completed");
            }
            else
            {
                logger.LogInformation("Withdrawn - Emailing - failed");
                logger.LogError(
                    "WithdrawnConfirmation - Emailing processing FAILED for onsCode {OnsCode}, localAuthorityName {LocalAuthorityName}, version {Version}, soiStatus Withdrawn",
                    statementOfIntent.OnsCode, statementOfIntent.LocalAuthority.Name, statementOfIntent.VersionNumber);
            }
        }

        private async Task<HttpResponseMessage> UpdateSoiStatusToWithdrawn(StatementOfIntent statementOfIntent,
            HttpClient httpClient)
        {
            // updating with new values of Soi Status Setting page
            statementOfIntent.Status = SoiStatusV2.Withdrawn;


            var httpPutResponseMessage =
                await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RoutePutStatusSetting, statementOfIntent);
            return httpPutResponseMessage;
        }
    }
}
