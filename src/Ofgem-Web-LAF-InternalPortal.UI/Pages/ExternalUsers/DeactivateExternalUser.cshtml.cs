using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;

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
    public class DeactivateExternalUserModel(
        ILogger<DeactivateExternalUserModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty] public string UserId { get; set; } = string.Empty;

        [BindProperty] public string FirstName { get; set; } = string.Empty;

        [BindProperty] public string LastName { get; set; } = string.Empty;

        [BindProperty] public string EmailAddress { get; set; } = string.Empty;

        [BindProperty] public ExternalUserType UserType { get; set; }

        [BindProperty] public string HomeBaseLocalAuthority { get; set; } = string.Empty;

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public Extensions.Permissions.ExternalUser Permissions { get; set; } = new();


        public void OnGetById(string userId, string firstName, string lastName, string emailAddress, ExternalUserType userType, string homeBaseLocalAuthority)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            UserType = userType;
            HomeBaseLocalAuthority = homeBaseLocalAuthority;

            logger.LogLafInformation(LogEvents.ExternalUsers, $"DeactivateExternalUserModel - OnGet - userId: {UserId}, firstName: {FirstName}, lastName:{LastName}");

            Permissions = new Extensions.Permissions.ExternalUser(httpContextAccessor, httpClientFactory, TempData);
        }

        public async Task<IActionResult> OnPostDeactivate(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, $"DeactivateExternalUserModel - OnPostDeactivate - userId: {userId}, firstName: {FirstName}, lastName:{LastName}");

            Permissions = new Extensions.Permissions.ExternalUser(httpContextAccessor, httpClientFactory, TempData);

            var httpClient = httpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var result = await httpClient.PutAsync($"{Services.UserApi.RouteDeactivateExternalUser}/{userId}", null);

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToPage(
                        LafPages.ExternalUsers.ExternalUserCompleted.ROUTE,
                        LafPages.ExternalUsers.ExternalUserCompleted.METHOD_GET_SUCCESS_DEACTIVATION);
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, "DeactivateExternalUserModel - OnPostDeactivate - {Message}", ex.Message);
                DisplayMessage = "An issue occurred deleting an upload.";
            }

            return RedirectToPage(LafPages.ExternalUsers.ROUTE);
        }

    }
}