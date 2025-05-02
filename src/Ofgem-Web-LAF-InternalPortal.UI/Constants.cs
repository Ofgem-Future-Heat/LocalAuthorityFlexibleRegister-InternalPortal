#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Ofgem_Web_LAF_InternalPortal;

public static class Constants
{

    public const string NotApplicable = "N/A";

    public const string RM_OUT_001_Referral_Made_Outside_Of_LAs_Remit_Is_InValid = "Enter yes or no in the column ‘Referral Made Outside of LA’s Remit";

    public const string LA_ACODE_001_LA_Area_Code_Has_A_Value = "The upload has an empty LA area code.";
    public const string LA_ACODE_002_LA_Area_Code_Has_Correct_Format = "The upload has an incorrect La Area code.";
    public const string LA_ACODE_003_LA_Area_Code_Must_Exist_Error = "Ensure declaration notification area code is an existing area code";
    public const string LA_ACODE_004_Area_Code_Entry_Is_Valid_Error = "The declaration notification indicates it cannot be submit for other local authorities, the area code does not match the ONS code";
    public const string LA_ACODE_005_Referral_Made_Outside_LA_Remit_Error = "The file cannot be processed because the declaration notification indicates it is a referral made outside of the LA's remit. Therefore the ONS code should not equal the area code";

    public const string LA_DURN_001_DECLARATION_UNIQUE_REFERENCE_NUMBER_IS_BLANK =
        "Column 'LA Declaration Unique Reference Number' is a required field. Please confirm the unique reference number, please refer to the Great British Insulation Scheme and ECO4 Flex Declaration Notification Data Dictionary for the correct format.";

    public const string LA_DURN_004_SINGLE_DECLARATION_UNIQUE_REFERENCE_NUMBER =
        "Please note the file cannot be uploaded because there are more than 1 submitting Local Authorities within it. Please note bulk upload is allowed for 1 Local Authority at a time.";



    public const int SOI_PAGE_SIZE = 50;
    public const string SOI_MINIMUM_DATE = "2022-04-01";

    public const string ThreePartDate_Hint = "For example, 25 2 2024";

    public const string SOI_EDIT_MANDATORY_FIELDS_SIGN_OFF =
        "Mandatory fields on sign-off checklist are incomplete. Please review sign-off checklist and retry.";

    public const string VERSION_NUMBER_IS_INVALID_MESSAGE =
        "Please review mandatory field 'Statement of Intent version number' and retry. Only alphanumeric entries and the special character '.' and '-' and '_' are permitted.";

    public const string VERSION_NUMBER_IS_MANDATORY =
        "Enter a version number.";

    public const string NAME_EXISTS = "Please enter a Local Authority name that does not already exist within the portal.";

    public const string PublishedDate_IS_MANDATORY =
        "Select an option to indicate if the publish date is correct.";

    public const string Proxy5PartOfRoute2_IS_MANDATORY =
        "Select an option to indicate if the scheme is part of Route 2.";
}