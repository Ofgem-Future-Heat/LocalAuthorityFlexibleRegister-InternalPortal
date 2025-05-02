using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;
using System.Data;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles
{
    [AutoValidateAntiforgeryToken]
    public class ProfileV2(
        ILogger<ProfileV2> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty(SupportsGet = true)] public string Name { get; set; } = string.Empty;

        [BindProperty] public string? OnsCode { get; set; }

        [BindProperty(SupportsGet = true)] public string? Email { get; set; }

        [BindProperty] public List<StatementOfIntentModel>? StatementOfIntents { get; set; }

        [BindProperty] public Extensions.Permissions.Profile Permissions { get; set; } = new();

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty]
        public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty]
        public string? AssessmentNotesError { get; set; }

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public bool HasAssessmentNoteError => AssessmentNotesError?.Length > 0;

        public async Task OnGetSuccessfulEdit(string name, string onsCode)
        {
            logger.LogInformation("Profile - OnGetSuccessfulEdit");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = $"Profile for '{name}' successfully updated";
            ShowNotification = true;

            await RefreshData(onsCode);
        }

        public async Task OnGetFailedSoiCreate(string onsCode)
        {
            logger.LogInformation("Profile - OnGetFailedSoiCreate");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = "Failed to create Statement Of Intent.";
            ShowNotification = true;

            await RefreshData(onsCode);
        }



        public async Task OnPostCreateAssessmentNote(Guid soiId, int index, string onsCode)
        {
            logger.LogInformation("Profile - OnPostCreateAssessmentNote");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            if (!string.IsNullOrEmpty(StatementOfIntents![index].AssessmentNoteText))
            {
                if (!TextValidation.IsValidCharacterCount(StatementOfIntents![index].AssessmentNoteText))
                {
                    AssessmentNotesError = "Assessment note must be 200 characters or less";
                    await RefreshData(onsCode);
                    return;
                }
                var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var request = new CreateAssessmentNoteRequest()
                {
                    StatementOfIntentId = soiId,
                    Text = StatementOfIntents![index].AssessmentNoteText
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteCreateAssessmentNote,
                        request);

                if (!httpResponseMessage.IsSuccessStatusCode)
                    throw new DataException("CreateAssessmentNoteRequest - OnPostCreateAssessmentNote : Failed");
            }

            await RefreshData(onsCode);

            ModelState.Clear();
        }


        public async Task OnGetDeleteAssessmentNote(Guid assessmentNoteId, string onsCode)
        {
            logger.LogInformation("Profile - OnPostDeleteAssessmentNote");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            var httpResponseMessage =
                await httpClient.DeleteAsync(Services.LocalAuthorityApi.RouteDeleteAssessmentNote + $"/{assessmentNoteId}");

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new DataException("Profile - OnPostDeleteAssessmentNote : Failed");

            await RefreshData(onsCode);
        }

        public async Task OnGetDeleteInternalNote(Guid internalNoteId, string onsCode)
        {
            logger.LogInformation("Profile - OnPostDeleteInternalNote");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            var httpResponseMessage =
                await httpClient.DeleteAsync(Services.LocalAuthorityApi.RouteDeleteInternalNote + $"/{internalNoteId}");

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new DataException("Profile - OnPostDeleteInternalNote : Failed");

            await RefreshData(onsCode);

        }

        public async Task OnGetUpdateSoiSignOffCheckList(string onsCode)
        {
            logger.LogInformation("Profile - OnGetUpdateSoiSignOffCheckList");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = "Failed to update Sign Off checklist.";
            ShowNotification = true;

            await RefreshData(onsCode);
        }

        public async Task OnGetUpdateSoiAssessmentCheckList(string onsCode)
        {
            logger.LogInformation("Profile - OnGetUpdateSoiAssessmentCheckList");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = "Failed to update Assessment checklist.";
            ShowNotification = true;

            await RefreshData(onsCode);
        }

        public async Task OnGetUpdateSoiRoutes(string onsCode)
        {
            logger.LogInformation("Profile - OnGetUpdateSoIRoutes");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = "Failed to update Statement of Intent Routes.";
            ShowNotification = true;

            await RefreshData(onsCode);
        }

        public async Task OnGetByOns(string onsCode, string versionNumber = "", string publishedDate = "")
        {
            logger.LogInformation("Profile - OnGetByOnsCode");

            Permissions = new Extensions.Permissions.Profile(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData(onsCode);

            // select the first one as the default tab selected
            if (StatementOfIntents != null && StatementOfIntents.Count != 0)
            {
                if (string.IsNullOrWhiteSpace(versionNumber) || string.IsNullOrWhiteSpace(publishedDate))
                {
                    StatementOfIntents![0].IsSelectedTab = true;
                }
                else
                {
                    var soi = StatementOfIntents!.First(x => x.VersionNumber == versionNumber && x.PublishedDate.ToString("yyyyMMdd") == publishedDate);

                    if (soi is null)
                    {
                        StatementOfIntents![0].IsSelectedTab = true;
                    }
                    else
                    {
                        soi.IsSelectedTab = true;
                    }
                }
            }
        }

        private async Task RefreshData(string onsCode)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage = await httpClient.GetAsync(Services.LocalAuthorityApi.RouteOnsExists + $"{onsCode}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                    if (result != null)
                    {
                        OnsCode = result.OnsCode;
                        Name = result.Name!;
                        Email = result.Email!;
                        StatementOfIntents = GetSoiModel(result.StatementOfIntents!);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Profile - RefreshData, Error");
                DisplayMessage = "An issue occurred retrieving the data.";
                throw;
            }
        }

        private static List<StatementOfIntentModel> GetSoiModel(ICollection<StatementOfIntent> soiIn)
        {
            var soiOut = new List<StatementOfIntentModel>();

            foreach (var soi in soiIn)
            {
                var soiModel = new StatementOfIntentModel
                {
                    OnsCode = soi.OnsCode,
                    Status = soi.Status,
                    LocalAuthorityId = soi.LocalAuthorityId,
                    CanSubmit = soi.CanSubmit,
                    Category = soi.Category,
                    IsOfgemLogoPresent = soi.IsOfgemLogoPresent,
                    IsRoute1Accurate = soi.IsRoute1Accurate,
                    IsRoute1SapBandsCorrect = soi.IsRoute1SapBandsCorrect,
                    IsRoute2Accurate = soi.IsRoute2Accurate,
                    IsProxy5excluded = soi.IsProxy5excluded,
                    IsProxy5notexcluded = soi.IsProxy5notexcluded,
                    IsProxy5Named = soi.IsProxy5Named,
                    IsProxy1nad3CannotUsedTogether = soi.IsProxy1nad3CannotUsedTogether,
                    IsProxy7CannotCombi5or6 = soi.IsProxy7CannotCombi5or6,
                    IsRoute3Accurate = soi.IsRoute3Accurate,
                    IsRoute3SapBandsCorrect = soi.IsRoute3SapBandsCorrect,
                    IsRoute4Accurate = soi.IsRoute4Accurate,
                    IsRoute4SapBandsCorrect = soi.IsRoute4SapBandsCorrect,
                    IsRoute4JointSoIOnlyUseECO4 = soi.IsRoute4JointSoIOnlyUseECO4,
                    IsSignOffLaOfficerResponsibleStatement = soi.IsSignOffLaOfficerResponsibleStatement,
                    IsSignOffResponsiblePersonSigned = soi.IsSignOffResponsiblePersonSigned,
                    IsUserCombinedTemplate = soi.IsUserCombinedTemplate,
                    IsVersionClear = soi.IsVersionClear,
                    StatementOfIntentId = soi.StatementOfIntentId,
                    StatementOfIntentLink = soi.StatementOfIntentLink,
                    VersionNumber = soi.VersionNumber,
                    PublishedDate = soi.PublishedDate,
                    DesignatedLas = [],
                    AssessmentNotes = [],
                    HasDeclarations = soi.HasDeclarations
                };

                AddDesignatedLas(soi, soiModel);

                AddAssessmentNotes(soi, soiModel);

                // reset the text in the text box
                soiModel.AssessmentNoteText = "";
                soiOut.Add(soiModel);
            }

            return soiOut;
        }


        private static void AddAssessmentNotes(StatementOfIntent soi, StatementOfIntentModel soiModel)
        {
            if (soi.AssessmentNotes == null) return;

            foreach (var note in soi.AssessmentNotes)
            {
                soiModel.AssessmentNotes!.Add(note);
            }
        }

        private static void AddDesignatedLas(StatementOfIntent soi, StatementOfIntentModel soiModel)
        {
            if (soi.DesignatedLas == null) return;

            foreach (var designatedLa in soi.DesignatedLas!.OrderBy(o => o.LocalAuthority!.Name))
            {
                var la = new DesignatedLA
                {
                    StatementOfIntentId = designatedLa.StatementOfIntentId,
                    LocalAuthorityId = designatedLa.LocalAuthorityId,
                    LocalAuthority = designatedLa.LocalAuthority,
                    DesignatedLAId = designatedLa.DesignatedLAId,
                    StatementOfIntent = designatedLa.StatementOfIntent
                };

                soiModel.DesignatedLas!.Add(la);
            }
        }
    }
}
