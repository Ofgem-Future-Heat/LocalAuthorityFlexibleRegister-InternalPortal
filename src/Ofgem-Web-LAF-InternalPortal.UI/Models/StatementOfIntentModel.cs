using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Models;

public class StatementOfIntentModel
{
    public bool IsSelectedTab { get; set; }

    public Guid StatementOfIntentId { get; set; }

    public Guid LocalAuthorityId { get; set; }

    public string? OnsCode { get; set; }

    public DateTime PublishedDate { get; set; }

    public SoiStatusV2 Status { get; set; }

    public SoiCategory Category { get; set; }

    public bool CanSubmit { get; set; }

    public string? VersionNumber { get; set; }

    public string? StatementOfIntentLink { get; set; }

    public bool? IsAppropriateTitle { get; set; }

    public bool? IsPublishDateCorrect { get; set; }

    public bool? IsUserCombinedTemplate { get; set; }

    public bool? IsOfgemLogoPresent { get; set; }

    public bool? IsVersionClear { get; set; }

    public bool? IsRoute1Accurate { get; set; }

    public bool? IsRoute1SapBandsCorrect { get; set; }

    public bool? IsRoute2Accurate { get; set; }

    public bool? IsProxy5excluded { get; set; }

    public bool? IsProxy5notexcluded { get; set; }

    public bool? IsProxy5Named { get; set; }

    public bool? IsProxy1nad3CannotUsedTogether { get; set; }

    public bool? IsProxy7CannotCombi5or6 { get; set; }

    public bool? IsRoute3Accurate { get; set; }

    public bool? IsRoute3SapBandsCorrect { get; set; }

    public bool? IsRoute4Accurate { get; set; }

    public bool? IsRoute4SapBandsCorrect { get; set; }

    public bool? IsRoute4JointSoIOnlyUseECO4 { get; set; }

    public bool? IsSignOffLaOfficerResponsibleStatement { get; set; }

    public bool? IsSignOffResponsiblePersonSigned { get; set; }

    public bool HasDeclarations { get; set; }

    public List<DesignatedLA>? DesignatedLas { get; set; }

    public List<AssessmentNote>? AssessmentNotes { get; set; }

    public string? AssessmentNoteText { get; set; }

    public string? InternalNoteText { get; set; }

    public bool IsRoute1Complete => (IsRoute1Accurate == true) && (IsRoute1SapBandsCorrect == true);

    public bool IsRoute2Complete => (IsRoute2Accurate == true) &&
                                    (IsProxy5excluded == true) &&
                                    (IsProxy5notexcluded == true) &&
                                    (IsProxy5excluded == true) &&
                                    (IsProxy1nad3CannotUsedTogether == true) &&
                                    (IsProxy7CannotCombi5or6 == true);

    public bool IsRoute3Complete => (IsRoute3Accurate == true) && 
                                    (IsRoute3SapBandsCorrect == true);

    public bool IsRoute4Complete => (IsRoute4Accurate == true) &&
                                    (IsRoute4SapBandsCorrect == true) &&
                                    (IsRoute4JointSoIOnlyUseECO4 == true);

    public string SOIUniqueId => VersionNumber?.Replace('.', '_').Replace('-', '_') + PublishedDate.ToString("yyyyMMdd");
}
