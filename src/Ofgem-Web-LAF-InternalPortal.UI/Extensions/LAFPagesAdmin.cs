#pragma warning disable CA1707
namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {
        public static class Admin
        {
            public const string ROUTE = "/Admin/Admin";
            public const string METHOD_ADD_ANNOUNCEMENT = "AddAnnouncement";
            public const string METHOD_ANNOUNCEMENT_UNPUBLISH = "AnnouncementUnPublish";
            public const string METHOD_ANNOUNCEMENT_DELETE = "AnnouncementDelete";
        }

        public static class AnnouncementAdd
        {
            public const string ROUTE = "/Admin/AddAnnouncement";

            public const string METHOD_ON_GET = "Start";
            public const string METHOD_PUBLISH_AND_CONTINUE = "PublishAndContinue";

            public const string METHOD_PUBLISHED_NOW = "PublishedNow";
            public const string METHOD_SCHEDULED_TO_PUBLISH = "ScheduledToPublish";

            public const string METHOD_DONOT_UNPUBLISH = "DoNotUnPublish";
            public const string METHOD_SCHEDULED_TO_UNPUBLISH = "ScheduledToUnPublish";
        }

        public static class AnnouncementEdit
        {
            public const string ROUTE = "/Admin/EditAnnouncement";
            public const string METHOD_GET_WITH_ID = "WithId";

            public const string METHOD_PUBLISH_AND_CONTINUE = "PublishAndContinue";

            public const string METHOD_PUBLISHED_NOW = "PublishedNow";
            public const string METHOD_SCHEDULED_TO_PUBLISH = "ScheduledToPublish";

            public const string METHOD_DONOT_UNPUBLISH = "DoNotUnPublish";
            public const string METHOD_SCHEDULED_TO_UNPUBLISH = "ScheduledToUnPublish";
        }


    }
}
