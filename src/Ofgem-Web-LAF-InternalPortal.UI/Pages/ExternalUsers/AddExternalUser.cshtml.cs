using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Services;

namespace Ofgem_Web_LAF_InternalPortal.Pages.ExternalUsers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert
    )]
    public class AddExternalUserModel(
        ILogger<ExternalUserPage> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IUserService userService)
        : ExternalUserPage(logger, httpClientFactory, httpContextAccessor, userService)
    {

        public async Task OnGet()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("AddExternalUser - OnGet");

                PageDataInitialise();
            });
        }

        public async Task<IActionResult> OnPostSave()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "AddExternalUser - OnPostSave - ");

            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            PageDataInitialise();

            ValidFirstname();
            ValidLastname();
            ValidEmail();
            ValidLocalAuthority(out var baseLocalAuthority);

            await ValidUserTypeAsync();

            if (baseLocalAuthority is null) return Page();

            if (DisplayPageErrors.Count > 0) return Page();




            var resultOfCreation = await userService.CreateExternalUserAsync(ExternalUser, baseLocalAuthority);

            if (resultOfCreation.Success)
            {
                return RedirectToPage(
                    LafPages.ExternalUsers.ExternalUserCompleted.ROUTE,
                    LafPages.ExternalUsers.ExternalUserCompleted.METHOD_GET_SUCCESS_ADD);
            }

            DisplayErrors(resultOfCreation.ErrorMessage, string.Empty);

            return Page();
        }
    }
}
