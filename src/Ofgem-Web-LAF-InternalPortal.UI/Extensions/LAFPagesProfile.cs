#pragma warning disable CA1707
namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {
        public static class Profiles
        {
            /// <summary>
            /// Routes need the correct razor page authorisation setting
            /// </summary>
            public const string ROUTE = "/Profiles/Profiles";
            public const string METHOD_SUCCESSFUL_CREATE = "SuccessfulCreate";
            public const string METHOD_APPLY_FILTERS = "ApplyFilters";
            public const string METHOD_CLEAR_FILTERS = "ClearFilters";
        }

        public static class AddProfile
        {
            public const string ROUTE = "/Profiles/AddProfile";
            public const string METHOD_ADD = "Add";
        }

        public static class EditProfile
        {
            public const string ROUTE = "/Profiles/EditProfile";
            public const string METHOD_GET_BY_ONS = "ByOns";
            public const string METHOD_SAVE = "Save";
        }

        public static class Profile
        {
            public const string ROUTE = "/Profiles/Profile";
            public const string METHOD_GET_BY_ONS = "ByOns";
            public const string METHOD_SUCCESSFUL_EDIT = "SuccessfulEdit";
            public const string METHOD_FAILED_SOI_CREATE = "FailedSoiCreate";
        }

        public static class ProfileV2
        {
            public const string ROUTE = "/Profiles/ProfileV2";
            public const string METHOD_GET_BY_ONS = "ByOns";
            public const string METHOD_SUCCESSFUL_EDIT = "SuccessfulEdit";
            public const string METHOD_FAILED_SOI_CREATE = "FailedSoiCreate";
        }
    }
}
