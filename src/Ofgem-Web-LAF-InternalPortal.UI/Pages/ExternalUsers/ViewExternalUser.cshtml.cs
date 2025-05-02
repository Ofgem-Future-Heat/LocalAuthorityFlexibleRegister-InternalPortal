using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Pages.ExternalUsers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert
    )]
    public class ViewExternalUserModel(
        ILogger<ViewExternalUserModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Ofgem.LAF.SharedLibrary.Models.User ExternalUser { get; set; } = new()
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            EmailAddress = string.Empty,
            UserType = ExternalUserType.LocalAuthorityOfficer
        };

        [BindProperty] public Extensions.Permissions.ExternalUser Permissions { get; set; } = new();

        [BindProperty] public required string HomeBaseLocalAuthority { get; set; } = string.Empty;


        [BindProperty] public string Message { get; set; } = string.Empty;
        [BindProperty] public bool HasMessage => Message.Length > 0;


        public async Task OnGet(string userId)
        {
            await Task.Run(() =>
            {
                logger.LogLafInformation(LogEvents.ExternalUsers, "ViewExternalUser - OnGet");

                PageDataInitialise(userId);
            });
        }


        private void PageDataInitialise(string userId)
        {
            Permissions = new Extensions.Permissions.ExternalUser(httpContextAccessor, httpClientFactory, TempData);

            GetExternalUser(userId);
        }

        private void GetExternalUser(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "ViewExternalUser - GetExternalUser");

            var httpClient = httpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var httpResponseMessage = httpClient.GetAsync($"{Services.UserApi.Route}/{userId}");

                if (!httpResponseMessage.Result.IsSuccessStatusCode)
                {
                    Message = "An issue occurred retrieving the external user data";
                    return;
                }

                var result = httpResponseMessage.Result.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.User?>();

                if (result.Result == null)
                {
                    Message = "An issue occurred retrieving the external user data";
                    return;
                }

                ExternalUser = result.Result;

                if (ExternalUser.ExternalUserLocalAuthorities is not { Count: > 0 }) return;

                var source =
                    ExternalUser.ExternalUserLocalAuthorities.First(f => f.IsBaseLocalAuthority != null && (bool)f.IsBaseLocalAuthority);

                HomeBaseLocalAuthority = $"{source.OnsCode} {source.Name}";

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, "ExternalUsers - GetExternalUser, userId {userId}, {Message}", userId, ex.Message);
                Message = "An issue occurred retrieving data";
            }
        }

    }
}
