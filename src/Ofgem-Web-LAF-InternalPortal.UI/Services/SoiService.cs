using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Services
{
    public interface ISoiService
    {
        Task RerunRules(string onsCode, string localAuthorityName, string version, SoiStatusV2 soiStatus, int soiDateYear, int soiDateMonth,
            int soiDateDay);
    }


    public class SoiService(IHttpClientFactory httpClientFactory,
        ILogger<SoiService> logger)
        : ISoiService
    {
        public async Task RerunRules(string onsCode, string localAuthorityName, string version, SoiStatusV2 soiStatus, int soiDateYear,
            int soiDateMonth, int soiDateDay)
        {
            logger.LogInformation(
                "RerunRules processing for onsCode {OnsCode}, localAuthorityName {LocalAuthorityName}, version {Version}, soiStatus {Status}, soiDateYear {Year}, soiDateMonth {Month}, soiDateDay {Day}",
                onsCode, localAuthorityName, version, soiStatus, soiDateYear, soiDateMonth, soiDateDay);

            // Rerun the processing of any declarations that target this SOI
            if (soiStatus == SoiStatusV2.PassedAssessment)
            {
                logger.LogInformation("RerunRules - started");

                var httpClientDeclaration = httpClientFactory.CreateClient(DeclarationApi.ApiName);

                var requestDeclarationRulesRerun1 = await httpClientDeclaration.GetAsync(
                    $"{DeclarationApi.RouteProfileSoiRerunRules}/{onsCode}/{soiDateYear}/{soiDateMonth}/{soiDateDay}");

                if (requestDeclarationRulesRerun1.IsSuccessStatusCode)
                {
                    logger.LogInformation("RerunRules - completed");
                }
                else
                {
                    logger.LogInformation("RerunRules - failed");
                    logger.LogError(
                        "RerunRules processing FAILED for onsCode {OnsCode}, localAuthorityName {LocalAuthorityName}, version {Version}, soiStatus {Status}, soiDateYear {Year}, soiDateMonth {Month}, soiDateDay {Day}",
                        onsCode, localAuthorityName, version, soiStatus, soiDateYear, soiDateMonth, soiDateDay);
                }
            }

            // Email the users to let them know the status of the SOI
            if (soiStatus is 
                SoiStatusV2.PassedAssessment or 
                SoiStatusV2.Withdrawn or 
                SoiStatusV2.FailedAssessment)
            {
                logger.LogInformation("RerunRules - Emailing - started");

                var httpClientUser = httpClientFactory.CreateClient(UserApi.ApiName);

                var requestSoiStatusEmails = await httpClientUser.PostAsJsonAsync(
                    $"{UserApi.RouteGetExternalUser}/{onsCode}/{localAuthorityName}/{version}/{UserApi.RouteNotifyByEmailSoiStatusChange}?status={(int)soiStatus}",
                    "{}");

                if (requestSoiStatusEmails.IsSuccessStatusCode)
                {
                    logger.LogInformation("RerunRules - Emailing - completed");
                }
                else
                {
                    logger.LogInformation("RerunRules - Emailing - failed");
                    logger.LogError(
                        "RerunRules Emailing processing FAILED for onsCode {OnsCode}, localAuthorityName {LocalAuthorityName}, version {Version}, soiStatus {Status}, soiDateYear {Year}, soiDateMonth {Month}, soiDateDay {Day}",
                        onsCode, localAuthorityName, version, soiStatus, soiDateYear, soiDateMonth, soiDateDay);
                }
            }
        }
    }
}
