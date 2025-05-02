
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    public static class TempDataKeys
    {
        public static string DuplicateData { get; } = "DuplicateData";
        public static string SubmissionNote { get; } = "SubmissionNote";
        public static string DuplicateDataToAction { get; } = "DuplicatesToAction";
        public static string UploadsAwaitingDecision { get; } = "UploadsAwaitingDecision";
        public static string UploadToDelete { get; } = "UploadToDelete";
        public static string DashboardDeclarationData { get; } = "DashboardDeclarationData";
        public static string DashboardFilterData { get; } = "DashboardFilterData";
        public static string DashboardDeclarationCount { get; } = "DashboardDeclarationCount";
        public static string DeclarationEdit { get; } = "DeclarationEdit";
        public static string DeclarationEditOriginal { get; } = "DeclarationEditOriginal";
        public static string LocalAuthorities { get; } = "LocalAuthorities";
        public static string StatementOfIntentFilterData { get; } = "StatementOfIntentFilterData";
        public static string StatementOfIntentCount { get; } = "StatementOfIntentCount";

    }

    public static class TempData
    {

        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class?
        {

            tempData[key] = System.Text.Json.JsonSerializer.Serialize(value);


            tempData.Keep(key);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class?
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            object? data = tempData.Peek(key);
#pragma warning disable CS8603 // Possible null reference return.
            return data == null ? null : System.Text.Json.JsonSerializer.Deserialize<T>((string)data, options);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
