using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class StatementOfIntentV2
    {
        public Guid StatementOfIntentId { get; set; }

        public string? LocalAuthority { get; set; }

        public string? BaseLaOnsCode { get; set; }

        public string? VersionNumber { get; set; }

        public string? SoiLink { get; set; }

        public DateTime? DateAdded { get; set; }

        public SoiStatusV2 SoiStatus { get; set; }




        public bool? IsLocalAuthorityNamed { get; set; }
        public bool? IsDelegatedAuthorityNamed { get; set; }

        public ForSchemeEnum ForScheme { get; set; }
        public bool? IsPublishedDateCorrect { get; set; }
        public bool? IsProxy5PartOfRoute2 { get; set; }



        public bool? IsRoute4SapBandsCorrect { get; set; }
        public bool? IsSoiSameAsWebsite { get; set; }
        public bool? HasMostRecentTemplate { get; set; }
        public bool? IsPreviousVersionStatusClear { get; set; }
        public bool? IsProxy5SchemePresent { get; set; }
        public bool? IsDescriptionNiceNg6Recommendation2 { get; set; }



        public bool IsInitialCheckListComplete =>
            (IsLocalAuthorityNamed != null) &&
            (IsDelegatedAuthorityNamed != null) &&
            (IsSoiSameAsWebsite != null) &&
            (HasMostRecentTemplate != null) &&
            (IsPreviousVersionStatusClear != null);

        public bool IsSchemeDetailComplete =>
            (IsPublishedDateCorrect != null) &&
            (IsProxy5PartOfRoute2 != null);

        public bool IsEligibilityForRoute2Proxy5Complete =>
            (IsProxy5SchemePresent != null);



        public bool CanSubmit { get; set; }

        public List<DesignatedLa>? DesignatedLAs { get; init; } = [];

        public class DesignatedLa
        {
            public Guid? LocalAuthorityId { get; init; }

            public string? LocalAuthorityName { get; init; }

            public string? OnsCode { get; init; }
        }

        public List<AssessmentNote> AssessmentNotes { get; init; } = [];

        public class AssessmentNote
        {
            public string Text { get; set; } = string.Empty;
            public string CreatedBy { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
        }

        /// <summary>
        /// Display ONLY version of the date
        /// </summary>
        public string DisplayCreatedDate => DateAdded.HasValue ? (LafLocalTimezone.ToLocalTime((DateTime)DateAdded).ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT)) : "";

        public string DelegatedLocalAuthorityList
        {
            get
            {
                return DesignatedLAs != null
                    ? string.Join(", ", DesignatedLAs.Select(p => p.LocalAuthorityName))
                    : string.Empty;
            }
        }


        public static StatementOfIntentV2 MapFromDtoStatementOfIntent(Ofgem.LAF.SharedLibrary.Models.StatementOfIntent statementOfIntent)
        {
            var rawStatementOfIntent = new StatementOfIntentV2
            {
                StatementOfIntentId = statementOfIntent.StatementOfIntentId,
                LocalAuthority = statementOfIntent.LocalAuthority?.Name,
                VersionNumber = statementOfIntent.VersionNumber,
                SoiLink = statementOfIntent.StatementOfIntentLink,
                DateAdded = statementOfIntent.PublishedDate,
                SoiStatus = statementOfIntent.Status,
                CanSubmit = statementOfIntent.CanSubmit,
                BaseLaOnsCode = statementOfIntent.OnsCode,
                DesignatedLAs = statementOfIntent.DesignatedLas?.Select(item => new DesignatedLa
                {
                    LocalAuthorityId = item?.LocalAuthority?.LocalAuthorityId,
                    LocalAuthorityName = item?.LocalAuthority?.Name,
                    OnsCode = item?.LocalAuthority?.OnsCode
                }).ToList() ?? [],

                AssessmentNotes = statementOfIntent.AssessmentNotes?.Select(item => new AssessmentNote()
                {
                    Text = item.Text,
                    CreatedBy = item.CreatedByName,
                    Date = ((DateTime)item.CreatedDate).ToString("dd MMM yyyy HH:mm")
                }).ToList() ?? [],

                IsRoute4SapBandsCorrect = statementOfIntent.IsRoute4SapBandsCorrect,
                IsSoiSameAsWebsite = statementOfIntent.IsSoiSameAsWebsite,
                HasMostRecentTemplate = statementOfIntent.HasMostRecentTemplate,
                IsPreviousVersionStatusClear = statementOfIntent.IsPreviousVersionStatusClear,
                ForScheme = statementOfIntent.ForScheme,
                IsPublishedDateCorrect = statementOfIntent.IsPublishedDateCorrect,
                IsProxy5PartOfRoute2 = statementOfIntent.IsProxy5PartOfRoute2,
                IsProxy5SchemePresent = statementOfIntent.IsProxy5SchemePresent,
                IsDescriptionNiceNg6Recommendation2 = statementOfIntent.IsDescriptionNiceNg6Recomendation2,
                IsLocalAuthorityNamed = statementOfIntent.IsLocalAuthorityNamed,
                IsDelegatedAuthorityNamed = statementOfIntent.IsDelegatedAuthorityNamed,
            };

            return rawStatementOfIntent;
        }
    }
}
