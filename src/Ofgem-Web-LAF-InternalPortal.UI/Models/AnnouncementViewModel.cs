using Microsoft.AspNetCore.Mvc;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class AnnouncementViewModel
    {
        [BindProperty]
        public Guid AnnouncementId { get; set; }

        [BindProperty]
        public string? Headline { get; set; }

        [BindProperty]
        public string? Text { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool PublishNow { get; set; }

        [BindProperty]
        public DateTime? PublishedDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool DoNotUnPublish { get; set; }

        [BindProperty]
        public DateTime? UnPublishedDate { get; set; }

        [BindProperty]
        public DateTime? ActionDate { get; set; }

        public bool PublishedDateError { get; set; }
        public bool UnPublishedDateError { get; set; }
        public bool HeadlineError { get; set; }
        public bool BodyError { get; set; }


        public const string BodyInvalidCharacterError = "Invalid characters in the Text.";

        public const string BodyEmptyError = "You must have text in the body";

        public const string HeadingInvalidCharacterError = "Invalid characters in the Headline.";

        public const string HeadingEmptyError = "You must have a headline";

        public const string Date_Invalid_Combination = "Publish date must be before Unpublish date";

        public string HeadlineErrorId = "headline";

        public string BodyErrorId = "text";

        public string PublishNowErrorId = "publishedNow";

        public string SchedulePublishErrorId = "schedulePublishedTime";

        public string UnPublishErrorId = "doNotUnpublished";

        public string ScheduleUnpublishErrorId = "scheduleUnpublished";


        public AnnouncementViewModel()
        {
            PublishNow = true;
            DoNotUnPublish = true;
        }
    }
}
