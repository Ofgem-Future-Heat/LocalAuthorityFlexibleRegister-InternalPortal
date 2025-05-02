using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Shared
{
    [ExcludeFromCodeCoverage]
    public class _SoiV2DesignatedLasModel : PageModel
    {
        public List<Ofgem.LAF.SharedLibrary.Models.Announcement> Announcements { get; set; } = new();

        public void OnGet()
        {
            // Method intentionally left empty.
        }
    }
}
