using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Services;
using System.Text;
using System.Text.Json;

namespace Ofgem_Web_LAF_InternalPortal.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;


        [BindProperty]
        public string? Message { get; set; }
        [BindProperty] public List<Models.AnnouncementView> PublishedAnnouncements { get; set; } = [];

        private const string ExcludedAnnouncementIdsKey = "ExcludedAnnouncementIds";

        public IndexModel(
            IUserService userService,
            ILogger<IndexModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            
            Message = string.Empty;
        }

        public async Task OnGet()
        {
            await GetData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // post the sign in for the user
            try
            {
                var httpClientUser = _httpClientFactory.CreateClient(Services. UserApi.ApiName);

                var requestUser = new Models.User(_httpContextAccessor.HttpContext?.User!, _httpClientFactory).Data;

                var signInResponse = await httpClientUser.PutAsJsonAsync(Services.UserApi.RoutePut, requestUser);

                if (signInResponse.IsSuccessStatusCode)
                {
                    return RedirectToPage(LafPages.Dashboard.ROUTE);
                }

                Message = $"SignIn - failed for user: {signInResponse.IsSuccessStatusCode}, {signInResponse.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                Message = "SignIn - failed for user:" + _httpContextAccessor.HttpContext?.User.Identity!.Name;

                _logger.LogInformation(ex, "SignIn - failed for user: {Name}", _httpContextAccessor.HttpContext?.User.Identity!.Name);
            }

            return Page();
        }

        public async Task OnGetHideAnnouncement(Guid id)
        {
            var excludedAnnouncementIdsBytes = HttpContext.Session.Get(ExcludedAnnouncementIdsKey);
            var excludesAnnouncementIdsJson = excludedAnnouncementIdsBytes == null || excludedAnnouncementIdsBytes.Length == 0 ? null : Encoding.ASCII.GetString(excludedAnnouncementIdsBytes);
            var excludedAnnouncementIds = (excludesAnnouncementIdsJson == null ? [] : JsonSerializer.Deserialize<HashSet<Guid>>(excludesAnnouncementIdsJson)) ?? [];

            excludedAnnouncementIds.Add(id);

            HttpContext.Session.Set(ExcludedAnnouncementIdsKey, Encoding.ASCII.GetBytes(JsonSerializer.Serialize(excludedAnnouncementIds)));

            await OnGet();
        }
        
        private async Task GetData()
        {
            try
            {
                if (!HttpContext.Session.TryGetValue(ExcludedAnnouncementIdsKey, out var excludedAnnouncementIdsBytes))
                {
                    excludedAnnouncementIdsBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new HashSet<Guid>()));
                    HttpContext.Session.Set(ExcludedAnnouncementIdsKey, excludedAnnouncementIdsBytes);
                }

                var excludedAnnouncementIds = JsonSerializer.Deserialize<HashSet<Guid>>(Encoding.ASCII.GetString(excludedAnnouncementIdsBytes))!;

                PublishedAnnouncements = (await _userService.GetPublishedAnnouncementsAsync())
                    .Where(x => !excludedAnnouncementIds.Contains(x.AnnouncementId))
                    .ToList() ?? throw new InvalidOperationException("Unable to load the announcements data");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError(ex, "IndexModel - GetData");
            }
        }
    }
}