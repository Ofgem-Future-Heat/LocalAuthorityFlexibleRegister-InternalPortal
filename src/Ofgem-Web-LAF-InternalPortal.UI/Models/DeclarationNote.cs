namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class DeclarationNote
    {
        public Guid DeclarationNoteId { get; set; }
        public Guid DeclarationId { get; set; }
        public string? Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}
