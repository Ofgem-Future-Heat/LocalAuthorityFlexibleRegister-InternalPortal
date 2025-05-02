using Microsoft.AspNetCore.Mvc;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    [BindProperties]
    public class ListUploadsFilter
    {
        public string RecordsPerPageSelected { get; set; } = "25";

        public int PageIndex { get; set; }
    }
}
