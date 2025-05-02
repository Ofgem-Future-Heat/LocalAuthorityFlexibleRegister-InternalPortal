using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using System.Globalization;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Admin
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
    )]
    public class AddAnnouncementModel : AnnouncementPage
    {
        [BindProperty] public Extensions.Permissions.AddAnnouncement Permissions { get; set; }

        [BindProperty] public bool PageLevelError { get; set; }

        public AddAnnouncementModel(ILogger<AddAnnouncementModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpClientFactory, httpContextAccessor)
        {
            Logger = logger;
            HttpClientFactory = httpClientFactory;
            HttpContextAccessor = httpContextAccessor;

            Permissions = new();
        }

        public async Task OnGet()
        {
            await Task.Run(() =>
            {
                Logger.LogLafInformation(LogEvents.Announcements);

                Permissions =
                    new Extensions.Permissions.AddAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);
            });
        }

        public async Task<IActionResult> OnPostPublishAndContinue()
        {
            Logger.LogLafInformation(LogEvents.Announcements);

            if (HttpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.AddAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            if (AnnouncementViewModel is null)
            {
                Logger.LogLafError(LogEvents.Announcements, "AnnouncementViewModel is null");
                var message = "An issue occurred Adding data";
                AnnouncementPageErrors(message, string.Empty);
                return Page();
            }

            ValidateHeadline();
            ValidateBody();
            ValidatePublishNullableDate();
            ValidateUnPublishNullableDate();

            if (DisplayPageErrors.Count > 0) return Page();

            var httpClient = HttpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var announcement = new Ofgem.LAF.SharedLibrary.Models.Announcement()
                {
                    Headline = AnnouncementViewModel.Headline,
                    Text = AnnouncementViewModel.Text,
                    PublishNow = AnnouncementViewModel.PublishNow,
                    PublishedDate = AnnouncementViewModel.PublishNow
                        ? DateTime.Now
                        : (DateTime)AnnouncementViewModel.PublishedDate!,
                    DoNotUnPublish = AnnouncementViewModel.DoNotUnPublish,
                    UnPublishedDate = AnnouncementViewModel.UnPublishedDate,
                    ActionDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    CreatedByName = Permissions.Name
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.UserApi.RouteAddAnnouncement, announcement);

                if (!httpResponseMessage.IsSuccessStatusCode) return RedirectToPage(LafPages.AnnouncementAdd.ROUTE);

                var result = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.Announcement>();

                if (result != null)
                {
                    return RedirectToPage(LafPages.Admin.ROUTE,
                        new
                        {
                            announcementId = result.AnnouncementId
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Logger.LogLafError(ex, LogEvents.Announcements);
                var message = "An issue occurred Adding data";
                AnnouncementPageErrors(message, string.Empty);
            }

            return Page();
        }

        // UI Radio button interactions
        public async Task OnPostPublishedNow()
        {
            Logger.LogLafInformation(LogEvents.Announcements);

            Permissions = new Extensions.Permissions.AddAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetPublishedNow);
        }

        public async Task OnPostScheduledToPublish()
        {
            Logger.LogLafInformation(LogEvents.Announcements);

            Permissions = new Extensions.Permissions.AddAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            SchedulePublishedNullableControl = new Models.ThreePartDateNullable(
                source: null,
                title: "When will the announcement be published?",
                titleToBeUsedInErrorMessage: "When will the announcement be published",
                firstHint: "",
                secondHint: "For example, 25 2 2024",
                showTheHighlightBar: true
                );

            if (SchedulePublishedNullableControl.Day.HasValue && SchedulePublishedNullableControl.Month.HasValue && SchedulePublishedNullableControl.Year.HasValue)
            {
                AnnouncementViewModel.PublishedDate = new DateTime(SchedulePublishedNullableControl.Year.Value,
                                                                    SchedulePublishedNullableControl.Month.Value,
                                                                     SchedulePublishedNullableControl.Day.Value, 0, 0, 0, DateTimeKind.Utc);
            }

            await Task.Run(SetCanScheduledToPublish);
        }

        public async Task OnPostDoNotUnPublish()
        {
            Logger.LogLafInformation(LogEvents.Announcements);

            Permissions = new Extensions.Permissions.AddAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetDoNotUnPublish);
        }

        public async Task OnPostScheduledToUnPublish()
        {
            Logger.LogLafInformation(LogEvents.Announcements);

            Permissions = new Extensions.Permissions.AddAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            ScheduledUnPublishedNullableControl = new Models.ThreePartDateNullable2(
                source: null,
                title: "When will the announcement be unpublished?",
                titleToBeUsedInErrorMessage: "When will the announcement be unpublished",
                firstHint: "",
                secondHint: "For example, 25 2 2024",
                showTheHighlightBar: true
                );

            await Task.Run(SetCanScheduledToUnPublish);
        }

        public bool ValidatePublishNullableDate()
        {
            if (!AnnouncementViewModel.PublishNow && !ValidateSchedulePublishedNullableControl()) return false;

            if (AnnouncementViewModel.PublishNow || !AnnouncementViewModel.PublishedDate.HasValue) return true;

            return true;
        }

        public bool ValidateUnPublishNullableDate()
        {
            if (!AnnouncementViewModel.DoNotUnPublish && !ValidateScheduledUnPublishedNullableControl()) return false;

            if (AnnouncementViewModel.DoNotUnPublish || !AnnouncementViewModel.UnPublishedDate.HasValue) return true;

            return true;
        }

        internal bool ValidateSchedulePublishedNullableControl()
        {
            if (SchedulePublishedNullableControl != null && SchedulePublishedNullableControl.HasErrors())
            {
                AnnouncementPageErrors(SchedulePublishedNullableControl.ErrorMessage, AnnouncementViewModel.SchedulePublishErrorId);
                return false;
            }

            if (SchedulePublishedNullableControl != null)
            {
                if (SchedulePublishedNullableControl!.Year.HasValue && SchedulePublishedNullableControl.Month.HasValue && SchedulePublishedNullableControl.Day.HasValue)
                {
                    AnnouncementViewModel.PublishedDate = new DateTime(SchedulePublishedNullableControl.Year.Value, SchedulePublishedNullableControl.Month.Value, SchedulePublishedNullableControl.Day.Value);

                    if (AnnouncementViewModel.PublishedDate < Convert.ToDateTime(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture))
                    {
                        var message = $"Date published field cannot be before {Convert.ToDateTime(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture):dd MM yyyy}";

                        AnnouncementPageErrors(message, AnnouncementViewModel.SchedulePublishErrorId);

                        SchedulePublishedNullableControl.HasError = true;
                        SchedulePublishedNullableControl.HasDayError = true;
                        SchedulePublishedNullableControl.HasMonthError = true;
                        SchedulePublishedNullableControl.HasYearError = true;
                        SchedulePublishedNullableControl.ErrorMessage = message;
                        return false;
                    }
                }
                else
                {
                    var message = "Incomplete date. Please provide day, month, and year.";
                    AnnouncementPageErrors(message, AnnouncementViewModel.SchedulePublishErrorId);

                    SchedulePublishedNullableControl.HasError = true;
                    SchedulePublishedNullableControl.HasDayError = true;
                    SchedulePublishedNullableControl.HasMonthError = true;
                    SchedulePublishedNullableControl.HasYearError = true;
                    SchedulePublishedNullableControl.ErrorMessage = message;
                    return false;
                }
            }

            return true;
        }

        internal bool ValidateScheduledUnPublishedNullableControl()
        {
            if (ScheduledUnPublishedNullableControl != null && ScheduledUnPublishedNullableControl.HasErrors())
            {
                AnnouncementPageErrors(ScheduledUnPublishedNullableControl.ErrorMessage2, AnnouncementViewModel.ScheduleUnpublishErrorId);
                return false;
            }

            if (ScheduledUnPublishedNullableControl != null)
            {
                if (ScheduledUnPublishedNullableControl!.Year2.HasValue && ScheduledUnPublishedNullableControl.Month2.HasValue && ScheduledUnPublishedNullableControl.Day2.HasValue)
                {

                    AnnouncementViewModel.UnPublishedDate = new DateTime(ScheduledUnPublishedNullableControl!.Year2.Value, ScheduledUnPublishedNullableControl.Month2.Value, ScheduledUnPublishedNullableControl.Day2.Value);

                    if (AnnouncementViewModel.UnPublishedDate < Convert.ToDateTime(DateTime.Today.AddDays(1), CultureInfo.InvariantCulture))
                    {
                        var message = "You cannot schedule an announcement in the past";
                        AnnouncementPageErrors(message, AnnouncementViewModel.ScheduleUnpublishErrorId);

                        ScheduledUnPublishedNullableControl.HasError2 = true;
                        ScheduledUnPublishedNullableControl.HasDay2Error = true;
                        ScheduledUnPublishedNullableControl.HasMonth2Error = true;
                        ScheduledUnPublishedNullableControl.HasYear2Error = true;
                        ScheduledUnPublishedNullableControl.ErrorMessage2 = message;
                        return false;
                    }

                    if (SchedulePublishedNullableControl != null && AnnouncementViewModel.PublishedDate > AnnouncementViewModel.UnPublishedDate)
                    {
                        AnnouncementPageErrors(Models.AnnouncementViewModel.Date_Invalid_Combination, AnnouncementViewModel.SchedulePublishErrorId);
                        AnnouncementViewModel.PublishedDateError = true;
                        SchedulePublishedNullableControl.HasError = true;
                        SchedulePublishedNullableControl.ErrorMessage = Models.AnnouncementViewModel.Date_Invalid_Combination;
                        return false;
                    }
                }
                else
                {
                    var message = "Incomplete date. Please provide day, month, and year.";
                    AnnouncementPageErrors(message, AnnouncementViewModel.ScheduleUnpublishErrorId);

                    ScheduledUnPublishedNullableControl.HasError2 = true;
                    ScheduledUnPublishedNullableControl.HasDay2Error = true;
                    ScheduledUnPublishedNullableControl.HasMonth2Error = true;
                    ScheduledUnPublishedNullableControl.HasYear2Error = true;
                    ScheduledUnPublishedNullableControl.ErrorMessage2 = message;
                    return false;
                }

            }

            return true;
        }

    }
}
