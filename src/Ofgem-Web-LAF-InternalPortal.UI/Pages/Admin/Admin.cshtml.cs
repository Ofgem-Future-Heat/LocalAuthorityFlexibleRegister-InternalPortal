using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Admin
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert
    )]
    public class AdminModel : PageModel
    {
        private readonly ILogger<AdminModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty]
        public List<AnnouncementLine> PublishedAnnouncements { get; set; }
        [BindProperty]
        public List<AnnouncementLine> ScheduledToPublishAnnouncements { get; set; }
        [BindProperty]
        public List<AnnouncementLine> UnPublishedAnnouncements { get; set; }

        [BindProperty] public Extensions.Permissions.Admin Permissions { get; set; }

        [BindProperty] public string? DisplayMessage { get; set; }

        public AdminModel(
            ILogger<AdminModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            Permissions = new();

            PublishedAnnouncements = new List<AnnouncementLine>();
            ScheduledToPublishAnnouncements = new List<AnnouncementLine>();
            UnPublishedAnnouncements = new List<AnnouncementLine>();

        }

        public async Task OnGet()
        {
            _logger.LogLafInformation(LogEvents.Admin);

            Permissions = new Extensions.Permissions.Admin(_httpContextAccessor, _httpClientFactory, TempData);

            await GetData();
        }

        public IActionResult OnGetAddAnnouncement()
        {
            _logger.LogLafInformation(LogEvents.Admin);

            return RedirectToPage(LafPages.AnnouncementAdd.ROUTE, LafPages.Admin.METHOD_ADD_ANNOUNCEMENT);
        }

        public async Task<IActionResult> OnPostAnnouncementUnPublish(Guid id)
        {
            _logger.LogLafInformation(LogEvents.Admin, id);

            var httpClient = _httpClientFactory.CreateClient(Services.UserApi.ApiName);

            var httpResponseMessage =
                await httpClient.PostAsJsonAsync($"{Services.UserApi.RouteUnPublishAnnouncement}/{id}",
                    new { announcementId = id });

            if (httpResponseMessage.IsSuccessStatusCode) return RedirectToPage(LafPages.Admin.ROUTE);

            DisplayMessage = "Unpublish : Failed";

            Permissions = new Extensions.Permissions.Admin(_httpContextAccessor, _httpClientFactory, TempData);

            await GetData();

            return Page();
        }

        public async Task<IActionResult> OnPostAnnouncementDelete(Guid id)
        {
            _logger.LogLafInformation(LogEvents.Admin, id);

            var httpClient = _httpClientFactory.CreateClient(Services.UserApi.ApiName);

            var httpResponseMessage =
                await httpClient.DeleteAsync($"{Services.UserApi.RouteDeleteAnnouncement}/{id}");

            if (httpResponseMessage.IsSuccessStatusCode) return RedirectToPage(LafPages.Admin.ROUTE);

            DisplayMessage = "Delete : Failed";

            Permissions = new Extensions.Permissions.Admin(_httpContextAccessor, _httpClientFactory, TempData);

            await GetData();

            return Page();
        }

        private async Task GetData()
        {
            _logger.LogLafInformation(LogEvents.Admin);

            var httpClient = _httpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync(Services.UserApi.RouteGetAllAnnouncements);
                
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var announcements = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.Announcement>>();

                    if (announcements != null)
                    {
                        var source = announcements.Select(announcement => new AnnouncementLine() { Announcement = announcement }).ToList();

                        PublishedAnnouncements = source.FindAll(f => f.Published).OrderByDescending(o => o.Announcement.PublishedDate).ToList();

                        ScheduledToPublishAnnouncements = source.FindAll(f => f.Scheduled).OrderBy(o => o.Announcement.PublishedDate).ToList();

                        UnPublishedAnnouncements = source.FindAll(f => f.UnPublished).OrderByDescending(o => o.Announcement.PublishedDate).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogLafError(ex, LogEvents.Admin);
                DisplayMessage = "An issue occurred retrieving data";
            }
        }
    }
}
