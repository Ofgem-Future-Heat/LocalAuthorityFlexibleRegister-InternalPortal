#pragma warning disable CA1707
namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {
        public static class UploadTemplate
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Declarations/UploadTemplate";

            public const string METHOD_SAVE_AND_CONTINUE_WITH_DUPLICATES = "DuplicateSaveAndContinue";
            public const string METHOD_DUPLICATE_CANCEL = "DuplicateCancel";
            public const string METHOD_SAVE_AND_CONTINUE_IGNORE_DUPLICATES = "SaveAndContinueIgnoreDuplicates";
            public const string METHOD_VALIDATE = "Validate";
        }

        public static class ResolveUploads
        {
            public const string ROUTE = "/Uploads/ResolveUploads";

        }

        public static class DeleteUpload
        {
            public const string ROUTE = "/Uploads/ConfirmDelete";
            public const string METHOD_DELETE_UPLOAD = "DeleteUpload";

        }

        public static class DuplicateEntries
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Declarations/DuplicateEntries";

            public const string METHOD_DUPLICATE_DETECTED_ACTION = "DuplicateDetected";

            public const string METHOD_DUPLICATE_SAVE_AND_CONTINUE = "DuplicateSaveAndContinue";
            public const string METHOD_SAVE_AND_CONTINUE_IGNORE_DUPLICATES = "SaveAndContinueIgnoreDuplicates";
            public const string METHOD_POST_DUPLICATE_CANCEL = "DuplicateCancel";
        }

        public static class Logs
        {
            public const string ROUTE = "/Uploads/Logs";
        }

        public static class UploadLog
        {
            public const string ROUTE = "/Uploads/ListUploads";
            public const string METHOD_DOWNLOAD_ERRORS = "DownloadErrors";
        }

        public static class StatementOfIntentLog
        {
            public const string ROUTE = "/Uploads/ListStatementOfIntents";
        }
    }
}
