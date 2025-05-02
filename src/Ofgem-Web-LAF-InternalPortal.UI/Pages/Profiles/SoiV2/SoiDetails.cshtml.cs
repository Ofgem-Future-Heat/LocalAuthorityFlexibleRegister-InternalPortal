using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem_Web_LAF_InternalPortal.Services;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]

    public class SoiDetails(
        ILogger<SoiDetails> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILaManagementService laManagementService) : PageModel
    {

        [BindProperty] public StatementOfIntentV2? StatementOfIntentModel { get; set; }
        [BindProperty] public List<StatementOfIntentV2> PriorStatementOfIntentList { get; set; } = [];

        [BindProperty] public bool HasError { get; set; }

        [BindProperty] public Extensions.Permissions.SoiV2.SoiDetails Permissions { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;


        [BindProperty]
        public bool CanAssessSoi => StatementOfIntentModel != null &&
                                    Permissions.CanAssess &&
                                    StatementOfIntentModel.SoiStatus != SoiStatusV2.Withdrawn &&
                                    StatementOfIntentModel.SoiStatus != SoiStatusV2.PassedAssessment &&
                                    StatementOfIntentModel.SoiStatus != SoiStatusV2.FailedAssessment;

        [BindProperty]
        public bool CanWithdrawnSoi => StatementOfIntentModel != null &&
                                       Permissions.CanWithdraw &&
                                       StatementOfIntentModel.SoiStatus != SoiStatusV2.Withdrawn;

        [BindProperty]
        public bool CanContinueSchemeDetail => StatementOfIntentModel is
        {
            IsInitialCheckListComplete: true, IsSchemeDetailComplete: false, IsEligibilityForRoute2Proxy5Complete: false
        };

        [BindProperty]
        public bool CanContinueEligibilityRoute => StatementOfIntentModel is
        {
            IsInitialCheckListComplete: true, IsSchemeDetailComplete: true, IsEligibilityForRoute2Proxy5Complete: false
        }
            and { IsProxy5PartOfRoute2: true };

        [BindProperty]
        public bool CanContinueStatusSetting => StatementOfIntentModel is
        {
            IsInitialCheckListComplete: true, IsSchemeDetailComplete: true
        } && 
            (StatementOfIntentModel.IsProxy5PartOfRoute2 == false ||
              StatementOfIntentModel.IsEligibilityForRoute2Proxy5Complete);


        public async Task OnGetById(Guid statementOfIntentId)
        {
            logger.LogInformation("SoiDetails - OnGetById");

            Permissions = new Extensions.Permissions.SoiV2.SoiDetails(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData(statementOfIntentId);
        }

        public async Task OnGetByOns(string onsCode)
        {
            logger.LogInformation("SoiDetails - OnGetByOnsCode");

            Permissions = new Extensions.Permissions.SoiV2.SoiDetails(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData(onsCode);
        }

        private async Task RefreshData(Guid statementOfIntentId)
        {
            try
            {
                StatementOfIntentModel = await laManagementService.GetSoiById(statementOfIntentId)
                                         ?? throw new ArgumentNullException($"{statementOfIntentId} does not exist");

                if (StatementOfIntentModel.BaseLaOnsCode is null) return;

                var localAuthority = await laManagementService.GetByOnsCodeAsync(StatementOfIntentModel.BaseLaOnsCode);

                if (localAuthority?.StatementOfIntents != null)
                {
                    foreach (var item in localAuthority.StatementOfIntents)
                    {
                        if (item.PublishedDate >= StatementOfIntentModel.DateAdded) continue;

                        var target = StatementOfIntentV2.MapFromDtoStatementOfIntent(item);

                        PriorStatementOfIntentList.Add(target);
                    }

                    PriorStatementOfIntentList = PriorStatementOfIntentList.OrderByDescending(e => e.DateAdded).ToList();
                }
            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving SOI data, {ex.Message}";
                logger.LogLafError(LogEvents.GetSoi, DisplayMessage);
            }
        }

        private async Task RefreshData(string onsCode)
        {
            try
            {
                var localAuthority = await laManagementService.GetByOnsCodeAsync(onsCode);

                if (localAuthority?.StatementOfIntents != null)
                {
                    foreach (var item in localAuthority.StatementOfIntents)
                    {

                        StatementOfIntentModel ??= StatementOfIntentV2.MapFromDtoStatementOfIntent(item);

                        if (item.PublishedDate >= StatementOfIntentModel.DateAdded) continue;

                        var target = StatementOfIntentV2.MapFromDtoStatementOfIntent(item);

                        PriorStatementOfIntentList.Add(target);
                    }

                    PriorStatementOfIntentList = PriorStatementOfIntentList.OrderByDescending(e => e.DateAdded).ToList();
                }
            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving SOI data, {ex.Message}";
                logger.LogLafError(LogEvents.GetSoi, DisplayMessage);
            }
        }
    }
}
