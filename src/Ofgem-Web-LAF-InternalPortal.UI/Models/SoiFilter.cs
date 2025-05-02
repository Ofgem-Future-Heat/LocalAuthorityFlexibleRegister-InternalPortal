using Microsoft.AspNetCore.Mvc;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    [BindProperties]
    public class SoiFilter
    {
        public string LocalAuthority { get; set; } = string.Empty;

        public List<LocalAuthorityVm>? LAs { get; set; }

        // Status
        public bool IsPassedAssessment { get; set; }
        public bool IsToBeAssessed { get; set; }
        public bool IsFailedAssessment { get; set; }
        public bool IsAwaitingSignOff { get; set; }

        // [BindProperty]
        public string? SortOrderId { get; set; }

        // [BindProperty]
        public int PageIndex { get; set; }
    }
}
