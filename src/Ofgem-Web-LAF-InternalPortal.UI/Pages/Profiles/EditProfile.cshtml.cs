using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public class EditProfileModel : PageModel
    {
        private readonly ILogger<EditProfileModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty(SupportsGet = true)] public string? Name { get; set; }

        [BindProperty] public string? OnsCode { get; set; }

        [BindProperty(SupportsGet = true)] public string? Email { get; set; }

        [BindProperty] public Extensions.Permissions.EditProfile Permissions { get; set; }

        public List<PageErrorsModel> DisplayPageErrors { get; set; } = [];

        [BindProperty] public bool HasMessage => DisplayPageErrors.Any(x => x.DisplayMessage?.Length > 0);

        [BindProperty]
        public string? GetNameErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == NameErrorId)?.DisplayMessage;

        [BindProperty]
        public string? GetEmailErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == EmailErrorId)?.DisplayMessage;

      
        private const string INVALID_ONS_CODE = "Invalid ONS. Please ensure the ONS is correct in the format ANNNNNNNN";

        private const string VALID_DOMAIN = "gov.uk";

        private const string INVALID_EMAIL =
            "Invalid email. Please ensure the email address is correct. E.g. localauthorityexample@gov.uk.";

        private const string NAME_IS_MANDATORY = "Invalid name. Please enter the name of the local authority.";

        private const string NAME_IS_INVALID = "Invalid characters in the name of the local authority.";

        [BindProperty] public bool ErrorName { get; set; }
        [BindProperty] public bool ErrorOnsCode { get; set; }
        [BindProperty] public bool ErrorEmail { get; set; }

        public string NameErrorId { get; set; } = "Name";

        public string OnsCodeErrorId { get; set; } = "OnsCode";

        public string EmailErrorId { get; set; } = "Email";

        public EditProfileModel(
                ILogger<EditProfileModel> logger,
                IHttpClientFactory httpClientFactory,
                IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.EditProfile();
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task OnGetByOns(string onsCode)
        {
            _logger.LogInformation("EditProfile - OnGetByOnsCode");

            Permissions = new Extensions.Permissions.EditProfile(_httpContextAccessor, _httpClientFactory, TempData);

            await RefreshData(onsCode);
        }

        private async Task RefreshData(string onsCode)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteOnsExists + $"{onsCode}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                    if (result != null)
                    {
                        OnsCode = result.OnsCode;
                        Name = result.Name;
                        Email = result.Email;
                    }
                }

                TempData.Keep();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditProfile - RefreshData, Error");
                var message = "An issue occurred retrieving the data.";
                DisplayErrors(message, string.Empty);
                throw;
            }
        }

        public async Task<IActionResult> OnPostSave()
        {
            _logger.LogInformation("EditProfile - OnPostSave");

            Permissions = new Extensions.Permissions.EditProfile(_httpContextAccessor, _httpClientFactory, TempData);

            // validate page details
            ValidateProfileName();
            ValidateOnsCode();
            ValidateEmail();

            if (DisplayPageErrors.Count > 0) return Page();

            var httpClient = _httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            var localAuthority = new Ofgem.LAF.SharedLibrary.Models.LocalAuthority()
            {
                Name = Name,
                Email = Email,
                OnsCode = OnsCode!
            };

            var httpResponseMessage =
                await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RoutePutByOnsCode, localAuthority);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return RedirectToPage(LafPages.ProfileV2.ROUTE,
                    LafPages.ProfileV2.METHOD_SUCCESSFUL_EDIT,
                    new
                    {
                        name = localAuthority.Name,
                        onsCode = localAuthority.OnsCode
                    });
            }

            throw new BadHttpRequestException("EditProfile - OnPostSave : Failed");
        }

        private bool ValidateOnsCode()
        {
            if (!BasicValidationService.OnsCodeIsValid(OnsCode))
            {
                DisplayErrors(INVALID_ONS_CODE, OnsCodeErrorId);
                ErrorOnsCode = true;
                return false;
            }

            return true;
        }

        private bool ValidateProfileName()
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

            if (Email!.Contains(' '))
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
