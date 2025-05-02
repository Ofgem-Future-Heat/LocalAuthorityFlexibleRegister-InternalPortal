#pragma warning disable CA1707
namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {
        /// <summary>
        /// Routes need the correct razor page authorisation setting
        /// </summary>
        public const string LANDING = "/Index";

        public const string METHOD_APPLY_PAGINATION = "ApplyPagination";

        public static class Dashboard
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Dashboard";
            public const string METHOD_SUCCESSFUL_UPLOAD = "SuccessfulUpload";
            public const string METHOD_APPLY_FILTERS = "ApplyFilters";
            public const string METHOD_CLEAR_FILTERS = "ClearFilters";
            public const string METHOD_DOWNLOAD_SELECTED = "DownloadSelected";
            public const string METHOD_DELETE_UPLOAD = "DeleteUpload";
            public const string METHOD_RESOLVE_UPLOAD_WAITING = "ResolveUpload";
            public const string METHOD_APPLY_SORTING = "ApplySorting";
        }

        public static class EditAssessmentDetails
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Declarations/EditAssessmentDetails";

            public const string METHOD_SAVE_AND_CONTINUE = "SaveAndContinue";
            public const string METHOD_GET_WITH_ID_ACTION = "WithId";
        }

        public static class DeclarationDetail
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Declarations/DeclarationDetail";

            public const string METHOD_GET_WITH_ID_ACTION = "WithId";
            public const string METHOD_CREATE_DECLARATION_NOTE = "CreateDeclarationNote";
            public const string METHOD_DELETE_DECLARATION_NOTE = "DeleteDeclarationNote";
        }

        public static class DeclarationDetailEdit
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Declarations/DeclarationDetailEdit";

            public const string METHOD_GET_WITH_ID_ACTION = "WithId";
            public const string METHOD_GET_FOR_CHANGE = "ForChange";
            public const string METHOD_VALIDATE = "Validate";
        }

        public static class DeclarationDetailEditSummary
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Declarations/DeclarationDetailEditSummary";

            public const string METHOD_GET_WITH_ID_ACTION = "WithId";
        }

        public static class DeclarationDetailEditCompleted
        {
            public const string ROUTE = "/Declarations/DeclarationDetailEditCompleted";

            public const string METHOD_GET_WITH_ID_AND_URN_ACTION = "WithIdAndUrn";
        }

        public static class SupersededDetails
        {
            public const string ROUTE = "/Declarations/SupersededDeclarationDetail";
            public const string METHOD_GET_WITH_URN_AND_VERSION = "WithUrnAndVersion";
        }

    }
}
