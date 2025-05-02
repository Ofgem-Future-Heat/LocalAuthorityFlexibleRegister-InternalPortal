using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Models;

public class Upload
{
    public int UploadId { get; set; }

    public DateTime? When { get; set; }

    public string? Who { get; set; }

    public UploadStatus State { get; set; }

    public string? LocalAuthority { get; set; }

    public string? Comments { get; set; }

    public bool HasErrors { get; set; }
}