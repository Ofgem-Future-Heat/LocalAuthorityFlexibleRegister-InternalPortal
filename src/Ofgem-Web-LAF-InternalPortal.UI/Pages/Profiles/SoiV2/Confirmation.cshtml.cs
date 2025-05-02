namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    public class Confirmation(
        Services.ISoiService soiService,
        ILogger<InitialAssessmentChecklist> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        protected override string PageName => "Confirmation";

        public async Task OnGetById(string statementOfIntentId, string localAuthorityName)
        {
            StatementOfIntentId = statementOfIntentId;

            await SetPageData(new Guid(statementOfIntentId));

            if (SoiStatus is 
                Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment or 
                Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment)
            {
                await soiService.RerunRules(
                    OnsCode, 
                    localAuthorityName, 
                    SoiVersion, 
                    SoiStatus, 
                    DatePublished.Year, 
                    DatePublished.Month, 
                    DatePublished.Day);
            }
        }
    }
}
