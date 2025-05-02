using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    [BindProperties]
    public class DeclarationFilter
    {
        // [BindProperty]
        public string Urn { get; set; } = string.Empty;

        public int? UploadId { get; set; } = null;

        public const int DateDaysDifference = 365;

        [BindProperty] public int DateFromDay { get; set; }
        [BindProperty] public int DateFromMonth { get; set; }
        [BindProperty] public int DateFromYear { get; set; }

        [BindProperty] public int DateToDay { get; set; }
        [BindProperty] public int DateToMonth { get; set; }
        [BindProperty] public int DateToYear { get; set; }

        public string DateFromMin { get; set; } = "2023-01-01";
        public string DateFromMax { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        public bool DateHasError { get; set; }
        public bool ToDateHasError { get; set; }
        public bool FromDateHasError { get; set; }
        public string DateErrorText { get; set; } = string.Empty;

        // Routes
        public bool Route1 { get; set; }
        public bool Route2 { get; set; }
        public bool Route3 { get; set; }
        public bool Route4 { get; set; }

        // Status
        public bool Accepted { get; set; }
        public bool NotAccepted { get; set; }
        public bool AwaitingSignOff { get; set; }
        public bool FailedCoreChecks { get; set; }
        public bool FailedSoiChecks { get; set; }
        public bool OnHold { get; set; }
        public bool Withdrawn { get; set; }

        // [BindProperty]
        public List<SelectListItem>? LAs { get; set; }


        // [BindProperty]
        public List<SelectListItem> RecordsPerPage =>
        [
            new() { Text = "25", Value = "25" },
            new() { Text = "50", Value = "50" },
            new() { Text = "100", Value = "100" }
        ];

        // [BindProperty]
        public string? RecordsPerPageSelected { get; set; }

        // [BindProperty]
        public List<SelectListItem> SortOrder =>
        [
            new()
            {
                Text = "Local Authority (A-Z)",
                Value = ((int)Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.LocalAuthorityA2Z).ToString()
            },
            new()
            {
                Text = "Local Authority (Z-A)",
                Value = ((int)Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.LocalAuthorityZ2A).ToString()
            },
            new()
            {
                Text = "Date uploaded (newest first)",
                Value = ((int)Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.DateUploadedZ2A).ToString()
            },
            new()
            {
                Text = "Date uploaded (oldest first)",
                Value = ((int)Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.DateUploadedA2Z).ToString()
            }
        ];

        // [BindProperty]
        public string? SortOrderId { get; set; }

        // [BindProperty]
        public int PageIndex { get; set; }
    }
}
