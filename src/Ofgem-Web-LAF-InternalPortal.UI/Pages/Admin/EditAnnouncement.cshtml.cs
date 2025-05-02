using System.Net;
using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;

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
    public class EditAnnouncementModel : AnnouncementPage
    {      
        [BindProperty] public Extensions.Permissions.EditAnnouncement Permissions { get; set; }

        [BindProperty] public bool PageLevelError { get; set; }

        public string DateHint { get; set; } = Constants.ThreePartDate_Hint;

        public EditAnnouncementModel(ILogger<EditAnnouncementModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpClientFactory, httpContextAccessor)
        {
            Logger = logger;
            HttpClientFactory = httpClientFactory;
            HttpContextAccessor = httpContextAccessor;

            Permissions = new();
        }

        public async Task OnGetWithId(string id)
        {
            Logger.LogLafInformation(LogEvents.EditAnnouncement, id);

            Permissions = new Extensions.Permissions.EditAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            var httpClient = HttpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync(Services.UserApi.RouteGetAnnouncement + $"/{id}");

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.Announcement>();

                    if (result != null)
                    {
                        AnnouncementViewModel = new Models.AnnouncementViewModel
                        {
                            AnnouncementId = result.AnnouncementId,
                            Headline = result.Headline,
                            Text = result.Text,
                            PublishNow = result.PublishNow,
                            PublishedDate = result.PublishedDate,
                            DoNotUnPublish = result.DoNotUnPublish,
                            UnPublishedDate = result.UnPublishedDate
                        };

                        SchedulePublishedControl = new Models.ThreePartDate(
                            source: AnnouncementViewModel.PublishedDate ?? DateTime.Today,
                            title: "When will the announcement be published?",
                            titleToBeUsedInErrorMessage: "When will the announcement be published",
                            firstHint: "",
                            secondHint: DateHint,
                            showTheHighlightBar: true
                        );

                        ScheduledUnPublishedControl = new Models.ThreePartDate2(
                            source: AnnouncementViewModel.UnPublishedDate ?? DateTime.Today,
                            title: "When will the announcement be unpublished?",
                            titleToBeUsedInErrorMessage: "When will the announcement be unpublished",
                            firstHint: "",
                            secondHint: DateHint,
                            showTheHighlightBar: true
                        );
                    }
                }
                else
                {
                    var message = "Unable to retrieve the Announcement";
                    AnnouncementPageErrors(message, string.Empty);
                    PageLevelError = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Logger.LogLafError(ex, LogEvents.EditAnnouncement);
                var message = "An issue occurred Editing data";
                AnnouncementPageErrors(message, string.Empty);
                PageLevelError = true;
            }
        }

        public async Task<IActionResult> OnPostPublishAndContinue()
        {
            Logger.LogLafInformation(LogEvents.EditAnnouncement);

            if (HttpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.EditAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            if (AnnouncementViewModel is null)
            {
                Logger.LogLafError(LogEvents.EditAnnouncement, "Null announcement view model");
                var message = "An issue occurred Editing data";
                AnnouncementPageErrors(message, string.Empty);
                return Page();
            }

            ValidateHeadline();
            ValidateBody();
            ValidatePublishDate();
            ValidateUnPublishDate();

            if (DisplayPageErrors.Count > 0)
            {
                return Page();
            }

            var httpClient = HttpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var announcement = new Ofgem.LAF.SharedLibrary.Models.Announcement()
                {
                    AnnouncementId = AnnouncementViewModel.AnnouncementId,
                    Headline = AnnouncementViewModel.Headline,
                    Text = AnnouncementViewModel.Text,
                    PublishNow = AnnouncementViewModel.PublishNow,
                    PublishedDate = AnnouncementViewModel.PublishNow
                        ? DateTime.Now
                        : (DateTime)AnnouncementViewModel.PublishedDate!,
                    DoNotUnPublish = AnnouncementViewModel.DoNotUnPublish,
                    UnPublishedDate = AnnouncementViewModel.UnPublishedDate,
                    ActionDate = DateTime.Now
                };

                var httpResponseMessage =
                    await httpClient.PutAsJsonAsync(Services.UserApi.RouteEditAnnouncement, announcement);

                if (!httpResponseMessage.IsSuccessStatusCode) return RedirectToPage(LafPages.AnnouncementEdit.ROUTE);

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
                Logger.LogLafError(ex, LogEvents.EditAnnouncement);
                var message = "An issue occurred Editing data";
                AnnouncementPageErrors(message, string.Empty);
            }

            return Page();
        }

        // UI Radio button interactions
        public async Task OnPostPublishedNow()
        {
            Logger.LogLafInformation(LogEvents.EditAnnouncement);

            Permissions = new Extensions.Permissions.EditAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetPublishedNow);
        }

        public async Task OnPostScheduledToPublish()
        {
            Logger.LogLafInformation(LogEvents.EditAnnouncement);

            Permissions = new Extensions.Permissions.EditAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            SchedulePublishedControl = new Models.ThreePartDate(
                source: AnnouncementViewModel.PublishedDate ?? DateTime.Today,
                title: "When will the announcement be published?",
                titleToBeUsedInErrorMessage: "When will the announcement be published",
                firstHint: "",
                secondHint: DateHint,
                showTheHighlightBar: true
            );

            AnnouncementViewModel.PublishedDate = new DateTime(SchedulePublishedControl.Year, SchedulePublishedControl.Month, SchedulePublishedControl.Day);

            await Task.Run(SetCanScheduledToPublish);
        }

        public async Task OnPostDoNotUnPublish()
        {
            Logger.LogLafInformation(LogEvents.EditAnnouncement);

            Permissions = new Extensions.Permissions.EditAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetDoNotUnPublish);
        }

        public async Task OnPostScheduledToUnPublish()
        {
            Logger.LogLafInformation(LogEvents.EditAnnouncement);

            Permissions = new Extensions.Permissions.EditAnnouncement(HttpContextAccessor, HttpClientFactory, TempData);

            ScheduledUnPublishedControl = new Models.ThreePartDate2(
                source: AnnouncementViewModel.UnPublishedDate ?? DateTime.Today,
                title: "When will the announcement be unpublished?",
                titleToBeUsedInErrorMessage: "When will the announcement be unpublished",
                firstHint: "",
                secondHint: DateHint,
                showTheHighlightBar: true
            );

            await Task.Run(SetCanScheduledToUnPublish);
        }
    }
}
