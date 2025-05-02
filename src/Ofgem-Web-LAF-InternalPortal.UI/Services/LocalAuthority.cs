using Microsoft.Net.Http.Headers;

namespace Ofgem_Web_LAF_InternalPortal.Services
{
    public static class LocalAuthorityApi
    {
        public const string ApiName = "LAF.Api.LocalAuthority";
        public const string Route = "api/LocalAuthorities";
        public const string RouteAdd = "api/LocalAuthorities";
        public const string RouteOnsExists = "api/LocalAuthorities/by-ons-code/";
        public const string RouteNameExists = "api/LocalAuthorities/by-name/";
        public const string RoutePutByOnsCode = "api/LocalAuthorities/by-ons-code/";
        public const string RouteGetFiltered = "api/LocalAuthorities/GetFiltered";
        public const string RouteGetSoiCount = "api/soi/count-at-status";

        public const string RouteCreateSoi = "api/soi";
        public const string RoutePutUpdateSOISignOffChecklist = "api/soi/updateSignOffCheckList";
        public const string RoutePutUpdateSOIAssessmentChecklist = "api/soi/updateAssessmentCheckList";
        public const string RoutePutUpdateSOIRoutes = "api/soi/updateRoutes";
        public const string RoutePutUpdateSoi = "api/soi/updateStatementOfIntent";
        public const string RoutePutUpdateBySoiId = "api/soi/update";

        public const string RouteCreateAssessmentNote = "api/assessment-notes";
        public const string RouteDeleteAssessmentNote = "api/assessment-notes";

        public const string RouteCreateInternalNote = "api/internal-notes";
        public const string RouteDeleteInternalNote = "api/internal-notes";

        public const string RouteGetByOnsCode = "api/LocalAuthorities/by-ons-code/";
        public const string RouteSoiList = "api/soi/soiList";
        public const string RouteSoiById = "api/soi/by-soi-id/";

        // Soi V2
        public const string RouteGetSoiV2 = "api/Soi/V2/getSoiV2/";
        public const string RouteCreateSoiV2 = "api/Soi/V2";
        public const string RouteUpdateSoiV2 = "api/Soi/V2/updateStatementOfIntent";
        public const string RoutePutInitialAssessmentChecklist = "api/Soi/V2/initialAssessmentCheckList";
        public const string RoutePutEligibilityRoute2Proxy5 = "api/Soi/V2/eligibilityRoute2Proxy5";
        public const string RoutePutStatusSetting = "api/Soi/V2/soiStatusSetting";
        public const string RouteSchemeDetails = "api/Soi/V2/schemeDetails";

        // log
        public const string RouteAddSoiLog = "api/Soi/V2/addSoiLog";
        public const string RouteGetFilteredSoiLog = "api/Soi/V2/getFilteredSoiLog";


        public static void ConfigureHttpClient(WebApplicationBuilder builder)
        {
            var apiEndpoint = builder.Configuration["LocalAuthorityServiceApiUrl"]
                              ?? throw new ArgumentNullException(nameof(builder), @"LocalAuthorityServiceApiUrl Api endpoint is not configured");

            builder.Services.AddHttpClient(ApiName, httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiEndpoint);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "HttpRequestsSample");
            }).AddHttpMessageHandler<HeaderHandler>();
        }
    }
}
