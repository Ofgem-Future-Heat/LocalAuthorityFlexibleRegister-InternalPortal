#pragma warning disable CA1707
namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {

        // Profiles/Soi
        public static class SoiV2
        {
            public static class Dashboard
            {
                public const string ROUTE = "/Profiles/SoiV2/Dashboard";
                public const string METHOD_APPLY_FILTERS = "ApplyFilters";
                public const string METHOD_CLEAR_FILTERS = "ClearFilters";
            }

            public static class SoiDetails
            {
                public const string ROUTE = "/Profiles/SoiV2/SoiDetails";
                public const string METHOD_GET_BY_ID = "ById";
                public const string METHOD_GET_BY_ONS = "ByOns";
            }

            public static class Add
            {
                public const string ROUTE = "/Profiles/SoiV2/Add";
                public const string METHOD_ADD = "Add";
                public const string METHOD_ADD_IN_LA = "AddInLa";
                public const string METHOD_TAKE_OUT_LA = "TakeOutLa";
                public const string METHOD_CAN_SUBMIT_ON_BEHALF_OF = "CanSubmitOnBehalfOf";
                public const string METHOD_CANNOT_SUBMIT_ON_BEHALF_OF = "CannotSubmitOnBehalfOf";
            }

            public static class AddConfirmation
            {
                public const string ROUTE = "/Profiles/SoiV2/AddConfirmation";
                public const string METHOD_GET_BY_ID = "ById";
            }

            public static class Edit
            {
                public const string ROUTE = "/Profiles/SoiV2/Edit";
                public const string METHOD_GET_BY_ONS = "ByOns";
                public const string METHOD_SAVE_CONTINUE = "SaveContinue";
                public const string METHOD_SAVE_EXIT = "SaveExit";
                public const string METHOD_ADD_IN_LA = "AddInLa";
                public const string METHOD_TAKE_OUT_LA = "TakeOutLa";
                public const string METHOD_CAN_SUBMIT_ON_BEHALF_OF = "CanSubmitOnBehalfOf";
                public const string METHOD_CANNOT_SUBMIT_ON_BEHALF_OF = "CannotSubmitOnBehalfOf";
            }

            public static class EditDesignatedLas
            {
                public const string ROUTE = "/Profiles/SoiV2/EditDesignatedLas";
                public const string METHOD_UPDATE_LA = "UpdateLa";
                public const string METHOD_ADD_IN_LA = "AddInLa";
                public const string METHOD_TAKE_OUT_LA = "TakeOutLa";
              }

            public static class SoiSummary
            {
                public const string ROUTE = "/Profiles/SoiV2/Summary";
                public const string METHOD_SAVE = "Save";
                public const string METHOD_CANCEL = "Cancel";
            }

            public static class SoiSummaryView
            {
                public const string ROUTE = "/Profiles/SoiV2/Summary";
                public const string METHOD_GET_BY_ID = "ById";
            }

            public static class Withdraw
            {
                public const string ROUTE = "/Profiles/SoiV2/Withdraw";
                public const string METHOD_GET_BY_ID = "ById";
                public const string METHOD_WITHDRAW = "Withdraw";
            }

            public static class WithdrawnConfirmation
            {
                public const string ROUTE = "/Profiles/SoiV2/WithdrawnConfirmation";
                public const string METHOD_GET_BY_ID = "ById";
            }

            public static class SoiConfirmationV2
            {
                public const string ROUTE = "/Profiles/SoiV2/Confirmation";
                public const string METHOD_GET_BY_ID = "ById";
            }

            public static class InitialAssessmentChecklist
            {
                public const string ROUTE = "/Profiles/SoiV2/InitialAssessmentChecklist";
                public const string METHOD_GET_BY_ID = "ById";
                public const string METHOD_SAVE_CONTINUE = "SaveContinue";
                public const string METHOD_SAVE_EXIT = "SaveExit";
            }

            public static class SchemeChecklist
            {
                public const string ROUTE = "/Profiles/SoiV2/SchemeDetails";
                public const string METHOD_GET_BY_ID = "ById";
                public const string METHOD_SAVE_CONTINUE = "SaveContinue";
                public const string METHOD_SAVE_EXIT = "SaveExit";
            }

            public static class EligibilityRoute2Proxy5
            {
                public const string ROUTE = "/Profiles/SoiV2/EligibilityRoute2Proxy5";
                public const string METHOD_GET_BY_ID = "ById";
                public const string METHOD_SAVE_CONTINUE = "SaveContinue";
                public const string METHOD_SAVE_EXIT = "SaveExit";
            }

            public static class StatusSetting
            {
                public const string ROUTE = "/Profiles/SoiV2/StatusSetting";
                public const string METHOD_GET_BY_ID = "ById";
                public const string METHOD_SAVE_CONTINUE = "SaveContinue";
                public const string METHOD_SAVE_EXIT = "SaveExit";
            }
        }
    }
}
