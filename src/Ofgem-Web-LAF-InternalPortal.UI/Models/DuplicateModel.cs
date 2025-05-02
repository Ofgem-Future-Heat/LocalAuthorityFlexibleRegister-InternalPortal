namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class DuplicateModel
    {
        public string DocumentContainer { get; set; }
        public string DocumentId { get; set; }
        public int UploadId { get; set; }

        public List<string?> UrnList { get; set; }

        public DuplicateModel()
        {
            DocumentContainer = string.Empty;
            DocumentId = string.Empty;
            UrnList = new List<string?>();
        }

        public DuplicateModel(string container, string documentId, int uploadId)
        {
            DocumentContainer = container;
            DocumentId = documentId;
            UploadId = uploadId;
            UrnList = new List<string?>();
        }
    }
}
