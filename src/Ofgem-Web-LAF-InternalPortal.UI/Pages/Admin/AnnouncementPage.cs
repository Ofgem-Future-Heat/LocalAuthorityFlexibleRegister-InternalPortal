using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;
using System.Globalization;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Admin
{
    public class AnnouncementPage : PageModel
    {
        internal ILogger<AnnouncementPage> Logger;
        internal IHttpClientFactory HttpClientFactory;
        internal IHttpContextAccessor HttpContextAccessor;

        [BindProperty(SupportsGet = true)] public Models.AnnouncementViewModel AnnouncementViewModel { get; set; }

        [BindProperty] public Models.ThreePartDate? SchedulePublishedControl { get; set; }

        [BindProperty] public Models.ThreePartDate2? ScheduledUnPublishedControl { get; set; }

        [BindProperty] public Models.ThreePartDateNullable? SchedulePublishedNullableControl { get; set; }

        [BindProperty] public Models.ThreePartDateNullable2? ScheduledUnPublishedNullableControl { get; set; }

        public string DateFromMin { get; set; } = Constants.SOI_MINIMUM_DATE;

        public string DateFromMax { get; set; } = DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATETIME_END_OF_DAY);

        public List<PageErrorsModel> DisplayPageErrors { get; set; } = [];

        [BindProperty] public bool HasMessage => DisplayPageErrors.Any(x => x.DisplayMessage?.Length > 0);

        [BindProperty] public string? GetHeadlineErrorMessage => 
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == AnnouncementViewModel.HeadlineErrorId)?.DisplayMessage;

        [BindProperty] public string? GetBodyErrorMessage => 
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == AnnouncementViewModel.BodyErrorId)?.DisplayMessage;


        protected AnnouncementPage(ILogger<AnnouncementPage> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            Logger = logger;
            HttpClientFactory = httpClientFactory;
            HttpContextAccessor = httpContextAccessor;

            AnnouncementViewModel = new Models.AnnouncementViewModel();
        }

        internal void SetPublishedNow()
        {
            AnnouncementViewModel.PublishNow = true;
            AnnouncementViewModel.PublishedDate = DateTime.Now;
        }

        internal void SetCanScheduledToPublish()
        {
            AnnouncementViewModel.PublishNow = false;
        }

        internal void SetDoNotUnPublish()
        {
            AnnouncementViewModel.DoNotUnPublish = true;
            AnnouncementViewModel.UnPublishedDate = null;
        }

        internal void SetCanScheduledToUnPublish()
        {
            AnnouncementViewModel.DoNotUnPublish = false;
        }

        public bool ValidateHeadline()
        {
            if (AnnouncementViewModel.Headline == null)
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.HeadingEmptyError, AnnouncementViewModel.HeadlineErrorId);
                AnnouncementViewModel.HeadlineError = true;
                return false;
            }
            if (string.IsNullOrEmpty(AnnouncementViewModel.Headline))
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.HeadingEmptyError, AnnouncementViewModel.HeadlineErrorId);
                AnnouncementViewModel.HeadlineError = true;
                return false;
            }
            if (!TextValidation.HasNoIllegalCharacters(AnnouncementViewModel.Headline))
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.HeadingInvalidCharacterError, AnnouncementViewModel.HeadlineErrorId);
                AnnouncementViewModel.HeadlineError = true;
                return false;
            }
            return true;
        }

        public bool ValidateBody()
        {
            if (AnnouncementViewModel.Text == null)
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.BodyEmptyError, AnnouncementViewModel.BodyErrorId);
                AnnouncementViewModel.BodyError = true;
                return false;
            }

            if (string.IsNullOrEmpty(AnnouncementViewModel.Text))
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.BodyEmptyError, AnnouncementViewModel.BodyErrorId);
                AnnouncementViewModel.BodyError = true;
                return false;
            }

            if (!TextValidation.HasNoIllegalCharacters(AnnouncementViewModel.Text))
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.BodyInvalidCharacterError, AnnouncementViewModel.BodyErrorId);
                AnnouncementViewModel.BodyError = true;
                return false;
            }

            return true;
        }

        public bool ValidatePublishDate()
        {
            if (!AnnouncementViewModel.PublishNow && !ValidateSchedulePublishedControl()) return false;

            if (AnnouncementViewModel.PublishNow || !AnnouncementViewModel.PublishedDate.HasValue) return true;

            return true;
        }

        public bool ValidateUnPublishDate()
        {
            if (!AnnouncementViewModel.DoNotUnPublish && !ValidateScheduledUnPublishedControl()) return false;

            if (AnnouncementViewModel.DoNotUnPublish || !AnnouncementViewModel.UnPublishedDate.HasValue) return true;

            return true;
        }

        internal bool ValidateSchedulePublishedControl()
        {
            if (SchedulePublishedControl != null && SchedulePublishedControl.HasErrors())
            {
                AnnouncementPageErrors(SchedulePublishedControl.ErrorMessage, AnnouncementViewModel.SchedulePublishErrorId);
                return false;
            }

            AnnouncementViewModel.PublishedDate = new DateTime(SchedulePublishedControl!.Year, SchedulePublishedControl.Month, SchedulePublishedControl.Day);

            if (AnnouncementViewModel.PublishedDate < Convert.ToDateTime(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture))
            {
                var message = $"Date published field cannot be before {Convert.ToDateTime(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture):dd MM yyyy}";

                AnnouncementPageErrors(message, AnnouncementViewModel.SchedulePublishErrorId);

                SchedulePublishedControl.HasError = true;
                SchedulePublishedControl.HasDayError = true;
                SchedulePublishedControl.HasMonthError = true;
                SchedulePublishedControl.HasYearError = true;
                SchedulePublishedControl.ErrorMessage = message;
                return false;
            }

            return true;
        }

        internal bool ValidateScheduledUnPublishedControl()
        {
            if (ScheduledUnPublishedControl != null && ScheduledUnPublishedControl.HasErrors())
            {
                AnnouncementPageErrors(ScheduledUnPublishedControl.ErrorMessage2, AnnouncementViewModel.ScheduleUnpublishErrorId);
                return false;
            }

            AnnouncementViewModel.UnPublishedDate = new DateTime(ScheduledUnPublishedControl!.Year2, ScheduledUnPublishedControl.Month2, ScheduledUnPublishedControl.Day2);

            if (AnnouncementViewModel.UnPublishedDate < Convert.ToDateTime(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture))
            {
                var message = "You cannot schedule an announcement in the past";
                AnnouncementPageErrors(message, AnnouncementViewModel.ScheduleUnpublishErrorId);

                ScheduledUnPublishedControl.HasError2 = true;
                ScheduledUnPublishedControl.HasDay2Error = true;
                ScheduledUnPublishedControl.HasMonth2Error = true;
                ScheduledUnPublishedControl.HasYear2Error = true;
                ScheduledUnPublishedControl.ErrorMessage2 = message;
                return false;
            }

            if (SchedulePublishedControl != null && AnnouncementViewModel.PublishedDate > AnnouncementViewModel.UnPublishedDate)
            {
                AnnouncementPageErrors(Models.AnnouncementViewModel.Date_Invalid_Combination, AnnouncementViewModel.SchedulePublishErrorId);
                AnnouncementViewModel.PublishedDateError = true;
                SchedulePublishedControl.HasError = true;
                SchedulePublishedControl.ErrorMessage = AnnouncementViewModel.Date_Invalid_Combination;
                return false;
            }


            return true;
        }

        public void AnnouncementPageErrors(string? displayMessage, string? errorId)
        {
            var error = new PageErrorsModel
            {
                DisplayMessage = displayMessage,
                ErrorId = errorId
            };

            DisplayPageErrors.Add(error);
        }
    }
}
