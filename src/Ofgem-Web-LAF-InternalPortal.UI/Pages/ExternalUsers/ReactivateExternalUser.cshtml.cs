using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_InternalPortal.Pages.ExternalUsers
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Admin,
        UserRoles.Expert,
        UserRoles.Standard,
        UserRoles.Basic
        )]
    public class ReactivateExternalUserModel(
        ILogger<ReactivateExternalUserModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        Services.IUserService userService)
        : PageModel
    {
        [BindProperty] public string UserId { get; set; } = string.Empty;

        [BindProperty] public string FirstName { get; set; } = string.Empty;

        [BindProperty] public string LastName { get; set; } = string.Empty;

        [BindProperty] public string EmailAddress { get; set; } = string.Empty;

        [BindProperty] public string UserType { get; set; } = string.Empty;

        [BindProperty] public string HomeBaseLocalAuthority { get; set; } = string.Empty;


        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public Extensions.Permissions.ExternalUser Permissions { get; set; } = new();


        public void OnGetById(string userId, string firstName, string lastName, string emailAddress, string userType, string homeBaseLocalAuthority)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            UserType = userType;
            HomeBaseLocalAuthority = homeBaseLocalAuthority;

            logger.LogLafInformation(LogEvents.ExternalUsers, $"ReactivateExternalUserModel - OnGet - userId: {UserId}, firstName: {FirstName}, lastName:{LastName}");

            Permissions = new Extensions.Permissions.ExternalUser(httpContextAccessor, httpClientFactory, TempData);
        }

        public async Task<IActionResult> OnPostReactivate(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, $"ReactivateExternalUserModel - OnPostReactivate - userId: {userId}, firstName: {FirstName}, lastName:{LastName}");

            Permissions = new Extensions.Permissions.ExternalUser(httpContextAccessor, httpClientFactory, TempData);

            var httpClient = httpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var user = await userService.GetExternalUserAsync(userId);

                if (user is null)
                {
                    DisplayMessage = "An issue occurred accessing the user data.";
                    return Page();
                }

                if (user.ExternalUserLocalAuthorities is null)
                {
                    DisplayMessage = "An issue occurred accessing the user's local authority data.";
                    return Page();
                }

                var homeBaseLocalAuthority = user.ExternalUserLocalAuthorities.FirstOrDefault(f => f.IsBaseLocalAuthority != null && (bool)f.IsBaseLocalAuthority);

                if (homeBaseLocalAuthority is null)
                {
                    DisplayMessage = "An issue occurred accessing the user's local authority home base data.";
                    return Page();
                }

                if (homeBaseLocalAuthority.OnsCode is null)
                {
                    DisplayMessage = "An issue occurred accessing the user's local authority home base data.";
                    return Page();
                }

                var result = await httpClient.PutAsync($"{Services.UserApi.RouteReactivateExternalUser}/{userId}", null);

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        LafPages.ExternalUsers.ExternalUserCompleted.ROUTE,
                        LafPages.ExternalUsers.ExternalUserCompleted.METHOD_GET_SUCCESS_REACTIVATE);
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, "ReactivateExternalUserModel - OnPostReactivate - {Message}", ex.Message);
                DisplayMessage = "An issue occurred deleting an upload.";
            }

            return RedirectToPage(LafPages.ExternalUsers.ROUTE);
        }
    }
}