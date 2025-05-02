using Microsoft.AspNetCore.Mvc;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    [BindProperties]
    public class FilterBase
    {
        public string RecordsPerPage { get; set; } = "25";

        public int PageIndex { get; set; }

        public string? Filter { get; set; }
    }
}
