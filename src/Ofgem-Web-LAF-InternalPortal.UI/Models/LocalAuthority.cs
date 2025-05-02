using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Models;

public class LocalAuthority
{
    public Guid LocalAuthorityId { get; set; }

    public string? OnsCode { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public SoiStatusV2 Status { get; set; }
}