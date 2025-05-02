using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    public class StatusSetting(
        ILogger<StatusSetting> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        [BindProperty] public Extensions.Permissions.SoiV2.StatusSetting Permissions { get; set; } = new();

        protected override string PageName => "StatusSetting";

        public async Task OnGet(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.StatusSetting(HttpContextAccessor, HttpClientFactory, TempData);

            await SetPageData(statementOfIntentId);
        }

        public async Task<IActionResult> OnPostSaveContinue(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveContinue", PageName);

            Permissions = new Extensions.Permissions.SoiV2.StatusSetting(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidStatusSettingPage())
            {
                return Page();
            }

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            return RedirectToPage(
                LafPages.SoiV2.SoiSummaryView.ROUTE,
                LafPages.SoiV2.SoiSummaryView.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        public async Task<IActionResult> OnPostSaveExit(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveExit", PageName);

            Permissions = new Extensions.Permissions.SoiV2.StatusSetting(HttpContextAccessor, HttpClientFactory, TempData);

            if (!ValidStatusSettingPage())
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

                // updating with new values of Soi Status Setting page
                statementOfIntent.Status = SoiStatus;

                if (!string.IsNullOrWhiteSpace(AdditionalInformationText))
                {
                    if (statementOfIntent.AssessmentNotes?.Count == 0 || statementOfIntent.AssessmentNotes == null)
                    {
                        // Initialize the collection if it's null
                        var assessmentNotes = new List<Ofgem.LAF.SharedLibrary.Models.AssessmentNote>();

                        // Create a new note
                        var newNote = new Ofgem.LAF.SharedLibrary.Models.AssessmentNote()
                        {
                            StatementOfIntentId = statementOfIntent.StatementOfIntentId,
                            Text = AdditionalInformationText,
                        };

                        // Add the new note to the collection
                        assessmentNotes.Add(newNote);

                        statementOfIntent.AssessmentNotes = assessmentNotes;
                    }
                    else
                    {
                        var note = statementOfIntent.AssessmentNotes.FirstOrDefault(x => x.StatementOfIntentId == statementOfIntentId);

                        if (note != null)
                        {
                            note.Text = AdditionalInformationText;
                        }
                    }
                }

                // update the latest soi
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpPutResponseMessage =
                    await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RoutePutStatusSetting,
                        statementOfIntent);

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
