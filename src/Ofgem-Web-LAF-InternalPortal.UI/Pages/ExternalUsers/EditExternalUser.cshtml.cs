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
    public class EditExternalUserModel(
        ILogger<ExternalUserPage> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IUserService userService)
        : ExternalUserPage(logger, httpClientFactory, httpContextAccessor, userService)
    {

        public async Task OnGetById(string userId)
        {
            await Task.Run(() =>
            {
                logger.LogLafInformation(LogEvents.ExternalUsers, $"EditExternalUser - OnGetById - userId:{userId}");

                GetExternalUser(userId);

                PageDataInitialise();
            });
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "EditExternalUser - OnPostUpdate - ");

            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            PageDataInitialise();

            ValidFirstname();
            ValidLastname();
            ValidEmail();
            ValidLocalAuthority(out var baseLocalAuthority);

            await ValidUserTypeAsync();


            if (DisplayPageErrors.Count > 0) return Page();

            if (baseLocalAuthority is null) return Page();


            var resultOfUpdate = await userService.UpdateExternalUserAsync(ExternalUser, baseLocalAuthority);

            if (resultOfUpdate.Success)
            {
                return RedirectToPage(
                    LafPages.ExternalUsers.ExternalUserCompleted.ROUTE,
                    LafPages.ExternalUsers.ExternalUserCompleted.METHOD_GET_SUCCESS_EDIT, 
                    new
                    {
                        emailAddress = ExternalUser.EmailAddress
                    });
            }

            DisplayErrors(resultOfUpdate.ErrorMessage, string.Empty);

            return Page();
        }

        private void GetExternalUser(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "EditExternalUser - GetExternalUser");

            var abc = userService.GetExternalUserAsync(userId);

            if (abc.Result is null)
            {
                var message = "An issue occurred retrieving the external user data";
                DisplayErrors(message, string.Empty);
                return;
            }

            ExternalUser = abc.Result;

            if (ExternalUser.ExternalUserLocalAuthorities is not { Count: > 0 }) return;

            var source =
                ExternalUser.ExternalUserLocalAuthorities.First(f => f.IsBaseLocalAuthority != null && (bool)f.IsBaseLocalAuthority);

            HomeBaseLocalAuthority = $"{source.OnsCode} {source.Name}";

            if (source.OnsCode != null) SelectedLa = source.OnsCode;

        }
    }
}
