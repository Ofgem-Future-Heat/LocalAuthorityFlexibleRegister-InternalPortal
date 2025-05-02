using Microsoft.AspNetCore.Mvc;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    [BindProperties]
    public class FilterExternalUsers : FilterBase
    {
        public override string ToString()
        {
            return $"PageIndex: {PageIndex}, RecordsPerPage: {RecordsPerPage}, Filter: {Filter}";
        }
    }
}
