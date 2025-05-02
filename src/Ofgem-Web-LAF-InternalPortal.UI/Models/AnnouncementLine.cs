using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class AnnouncementLine
    {
        public required Ofgem.LAF.SharedLibrary.Models.Announcement Announcement { get; init; }

        public bool Published => Announcement.PublishedDate <= DateTime.Now &&
                                 (Announcement.DoNotUnPublish ||
                                  (!Announcement.DoNotUnPublish && Announcement.UnPublishedDate >= DateTime.Now));

        public bool Scheduled => Announcement.PublishedDate > DateTime.Now &&
                                (Announcement.UnPublishedDate >= DateTime.Now ||
                                 Announcement.DoNotUnPublish);

        public bool UnPublished => Announcement.UnPublishedDate <= DateTime.Now;
    }
}
