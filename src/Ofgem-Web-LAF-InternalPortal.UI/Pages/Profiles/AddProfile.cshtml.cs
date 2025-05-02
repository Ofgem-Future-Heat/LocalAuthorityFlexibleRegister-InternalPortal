using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem_Web_LAF_InternalPortal.Services;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert
    )]
    public class AddProfileModel(
        IProfileService profileService,
        ILogger<AddProfileModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty(SupportsGet = true)] public string Name { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string OnsCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string Email { get; set; } = string.Empty;

        [BindProperty] public Extensions.Permissions.AddProfile Permissions { get; set; } = new();

        public List<PageErrorsModel> DisplayPageErrors { get; set; } = [];

        [BindProperty] public bool HasMessage => DisplayPageErrors.Any(x => x.DisplayMessage?.Length > 0);

        [BindProperty]
        public string? GetNameErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == NameErrorId)?.DisplayMessage;

        [BindProperty]
        public string? GetOnsCodeErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == OnsCodeErrorId)?.DisplayMessage;

        [BindProperty]
        public string? GetEmailErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == EmailErrorId)?.DisplayMessage;

        private const string INVALID_ONS_CODE = "Invalid ONS. Please ensure the ONS is correct in the format ANNNNNNNN";

        private const string VALID_DOMAIN = "gov.uk";

        private const string INVALID_EMAIL =
            "Invalid email. Please ensure the email address is correct. E.g. localauthorityexample@gov.uk.";

        private const string ONS_EXISTS = "Please enter an ONS that does not already exist within the portal.";

        private const string NAME_IS_MANDATORY = "Invalid name. Please enter the name of the local authority.";

        private const string NAME_IS_INVALID = "Invalid characters in the name of the local authority.";

        [BindProperty] public bool ErrorName { get; set; }
        [BindProperty] public bool ErrorOnsCode{ get; set; }
        [BindProperty] public bool ErrorEmail { get; set; }

        public string NameErrorId = "Name";

        public string OnsCodeErrorId = "OnsCode";

        public string EmailErrorId = "Email";


        public async Task OnGet()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("AddProfile - OnGet");

                Permissions = new Extensions.Permissions.AddProfile(httpContextAccessor, httpClientFactory, TempData);
            });
        }

        public async Task<IActionResult> OnPostAdd()
        {
            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.AddProfile(httpContextAccessor, httpClientFactory, TempData);

            // validate the ons code and the email

            await ValidateProfileName();
            await ValidateOnsCode();
            ValidateEmail();

            if (DisplayPageErrors.Count > 0) return Page();


            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            try
            {
                var localAuthority = new Ofgem.LAF.SharedLibrary.Models.LocalAuthority()
                {
                    Name = Name,
                    Email = Email,
                    OnsCode = OnsCode
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteAdd, localAuthority);

                if (!httpResponseMessage.IsSuccessStatusCode) return RedirectToPage(LafPages.Profiles.ROUTE);

                var result = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                if (result != null)
                {
                    return RedirectToPage(LafPages.Profiles.ROUTE, LafPages.Profiles.METHOD_SUCCESSFUL_CREATE,
                        new
                        {
                            name = localAuthority.Name
                        });
                }

                return RedirectToPage(LafPages.Profiles.ROUTE);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogLafError(LogEvents.Profiles, $"OnPostAdd, Error {ex.Message}");
                var message = "An issue occurred retrieving data";

                DisplayErrors(message, string.Empty);
            }

            return Page();
        }

        private async Task<bool> ValidateProfileName()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                DisplayErrors(NAME_IS_MANDATORY, NameErrorId);
                ErrorName = true;
                return false;
            }

            if (TextValidation.IsValidProfileName(Name))
            {
                DisplayErrors(NAME_IS_INVALID, NameErrorId);
                ErrorName = true;
                return false;
            }

            var profileAlreadyExits = await profileService.ProfileExistsAsync(Name);
            if (profileAlreadyExits.Value)
            {
                DisplayErrors(Constants.NAME_EXISTS, NameErrorId);
                ErrorName = true;
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateOnsCode()
        {
            if (!BasicValidationService.OnsCodeIsValid(OnsCode))
            {
                DisplayErrors(INVALID_ONS_CODE, OnsCodeErrorId);
                ErrorOnsCode = true;
                return false;
            }

            if (await OnsExistsAsync(OnsCode))
            {
                DisplayErrors(ONS_EXISTS, OnsCodeErrorId);
                ErrorOnsCode = true;
                return false;
            }

            return true;
        }

        private bool ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                DisplayErrors(INVALID_EMAIL, EmailErrorId);
                ErrorEmail = true;
                return false;
            }

            if (Email.Contains(' '))
            {
                DisplayErrors(INVALID_EMAIL, EmailErrorId);
                ErrorEmail = true;
                return false;
            }

            if (!Email.Contains('@'))
            {
                DisplayErrors(INVALID_EMAIL, EmailErrorId);
                ErrorEmail = true;
                return false;
            }

            if (!Email.EndsWith(VALID_DOMAIN))
            {
                DisplayErrors(INVALID_EMAIL, EmailErrorId);
                ErrorEmail = true;
                return false;
            }

            return true;
        }

        private async Task<bool> OnsExistsAsync(string onsCode)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteOnsExists + $"{onsCode}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                    return result != null;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.Profiles, $"OnsExistsAsync, Error {ex.Message}");
                var message = "An issue occurred retrieving the data.";

                DisplayErrors(message, string.Empty);
                throw;
            }
        }

        public void DisplayErrors(string? displayMessage, string? errorId)
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
