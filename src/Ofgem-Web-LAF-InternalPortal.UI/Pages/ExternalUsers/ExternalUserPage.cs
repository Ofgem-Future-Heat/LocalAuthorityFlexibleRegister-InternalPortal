using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem_Web_LAF_InternalPortal.Services;

namespace Ofgem_Web_LAF_InternalPortal.Pages.ExternalUsers
{
    public class ExternalUserPage(
        ILogger<ExternalUserPage> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IUserService userService) : PageModel
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


        [BindProperty]
        public required List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>? LocalAuthorities { get; set; } = [];

        [BindProperty] public required List<SelectListItem> LaSelectList { get; set; } = [];
        [BindProperty] public required string SelectedLa { get; set; } = string.Empty;


        [BindProperty]
        public required List<SelectListItem> UserTypes { get; set; } = [];
        [BindProperty] public required string SelectedUserType { get; set; } = string.Empty;

        public List<PageErrorsModel> DisplayPageErrors { get; set; } = [];

        [BindProperty] public bool HasMessage => DisplayPageErrors.Any(x => x.DisplayMessage?.Length > 0);

        [BindProperty] public bool ErrorFirstname { get; set; }
        [BindProperty] public bool ErrorLastname { get; set; }
        [BindProperty] public bool ErrorEmail { get; set; }
        [BindProperty] public bool ErrorExternalUserType { get; set; }
        [BindProperty] public bool ErrorLocalAuthority { get; set; }

        [BindProperty]
        public string? GetFirstNameErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == ExternalUserFirstNameId)?.DisplayMessage;

        [BindProperty]
        public string? GetLastNameErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == ExternalUserLastNameId)?.DisplayMessage;

        [BindProperty]
        public string? GetEmailErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == ExternalUserEmailId)?.DisplayMessage;

        [BindProperty]
        public string? GetLocalAuthorityErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == ExternalUserLocalAuthorityId)?.DisplayMessage;

        [BindProperty]
        public string? GetUserTypeErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == ExternalUserTypeId)?.DisplayMessage;

        public string? ErrorId { get; set; }


        public const string LocalAuthoritySelectInstruction = "Select a local authority";

        public const string ValidDomain = "gov.uk";

        public const string FirstnameIsMandatory = "Enter a first name";

        public const string FirstnameIsInvalid =
            "Enter a first name containing letters, numbers or the special characters of comma, apostrophe, dash and underscore";

        public const string LastnameIsMandatory = "Enter a last name";

        public const string LastnameIsInvalid =
            "Enter a last name containing letters, numbers or the special characters of comma, apostrophe, dash and underscore";

        public const string EmailIsMandatory = "Enter an email address";

        public const string InvalidEmail =
            "Enter an email address in the correct format, like name@example.com";

        public const string InvalidDomain =
            "Enter a government email address, for example name@example.gov.uk";

        public const string ExternalUserTypeIsMandatory =
            "Please select a user type.";

        public const string ExternalUserTypeIsInvalid =
            "Invalid characters in the external user type of the external user.";

        public const string LocalAuthoritySelectionMissing = "Please select a local authority";

        public const string LocalAuthoritySelectionMatchNotFound =
            "Local authority. Unable to match the selection to a local authority..";

        public string ExternalUserFirstNameId = "ExternalUser_FirstName";

        public string ExternalUserLastNameId = "ExternalUser_LastName";

        public string ExternalUserEmailId = "ExternalUser_EmailAddress";

        public string ExternalUserLocalAuthorityId = "local-authority";

        public string ExternalUserTypeId = "user-types";


        public bool ValidFirstname()
        {
            // firstname

            if (string.IsNullOrEmpty(ExternalUser.FirstName))
            {
                DisplayErrors(FirstnameIsMandatory, ExternalUserFirstNameId);

                ErrorFirstname = true;
                return false;
            }

            if (TextValidation.IsValidName(ExternalUser.FirstName))
            {
                DisplayErrors(FirstnameIsInvalid, ExternalUserFirstNameId);

                ErrorFirstname = true;
                return false;
            }

            return true;
        }

        public bool ValidLastname()
        {
            // lastname

            if (string.IsNullOrEmpty(ExternalUser.LastName))
            {
                DisplayErrors(LastnameIsMandatory, ExternalUserLastNameId);

                ErrorLastname = true;
                return false;
            }

            if (TextValidation.IsValidName(ExternalUser.LastName))
            {
                DisplayErrors(LastnameIsInvalid, ExternalUserLastNameId);

                ErrorLastname = true;
                return false;
            }

            return true;
        }

        public bool ValidEmail()
        {
            // email
            if (string.IsNullOrEmpty(ExternalUser.EmailAddress))
            {
                DisplayErrors(EmailIsMandatory, ExternalUserEmailId);

                ErrorEmail = true;
                return false;
            }

            if (ExternalUser.EmailAddress.Contains(' '))
            {
                DisplayErrors(InvalidEmail, ExternalUserEmailId);

                ErrorEmail = true;
                return false;
            }

            if (!ExternalUser.EmailAddress.Contains('@'))
            {
                DisplayErrors(InvalidEmail, ExternalUserEmailId);

                ErrorEmail = true;
                return false;
            }

            if (!ExternalUser.EmailAddress.EndsWith(".gov.uk"))
            {
                DisplayErrors(InvalidDomain, ExternalUserEmailId);

                ErrorEmail = true;
                return false;
            }

            return true;
        }

        public bool ValidLocalAuthority(out Ofgem.LAF.SharedLibrary.Models.LocalAuthority? baseLocalAuthority)
        {
            if (string.IsNullOrEmpty(SelectedLa))
            {
                DisplayErrors(LocalAuthoritySelectionMissing, ExternalUserLocalAuthorityId);

                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            if (SelectedLa == LocalAuthoritySelectInstruction)
            {
                DisplayErrors(LocalAuthoritySelectionMissing, ExternalUserLocalAuthorityId);

                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            if (LocalAuthorities == null)
            {
                var message = "An issue occurred retrieving the local authority data";

                DisplayErrors(message, ExternalUserLocalAuthorityId);

                logger.LogLafError(LogEvents.ExternalUsers, $"GetLocalAuthoritiesAsync, Error {message}");
                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            var source = LocalAuthorities.Find(f => f.OnsCode == SelectedLa);

            if (source == null)
            {
                DisplayErrors(LocalAuthoritySelectionMatchNotFound, ExternalUserLocalAuthorityId);

                logger.LogLafError(LogEvents.ExternalUsers, $"GetLocalAuthoritiesAsync, Error {LocalAuthoritySelectionMatchNotFound}");
                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            baseLocalAuthority = source;
            return true;
        }

        public async Task<bool> ValidUserTypeAsync()
        {
            if (string.IsNullOrEmpty(SelectedUserType))
            {
                DisplayErrors(ExternalUserTypeIsMandatory, ExternalUserTypeId);

                ErrorExternalUserType = true;
                return false;
            }

            if (SelectedUserType == "0")
            {
                DisplayErrors(ExternalUserTypeIsMandatory, ExternalUserTypeId);

                ErrorExternalUserType = true;
                return false;
            }

            if (TextValidation.IsValidName(SelectedUserType))
            {
                DisplayErrors(ExternalUserTypeIsInvalid, ExternalUserTypeId);

                ErrorExternalUserType = true;
                return false;
            }

            ExternalUserType selectedType;
            if (Enum.TryParse(SelectedUserType, out selectedType))
            {
                ExternalUser.UserType = selectedType;
            }
            else
            {
                DisplayErrors(ExternalUserTypeIsInvalid, ExternalUserTypeId);

                ErrorExternalUserType = true;
                return false;
            }

            if (selectedType != ExternalUserType.DesignatedAuthorisedSignatory) return true;

            var (dasUser, found, errorMessage) = await userService.AuthorisedSignatoryExistsAsync(HomeBaseLocalAuthority, SelectedLa);

            if (!found) return true;

            if (dasUser is not null)
            {
                if (dasUser.UserId == ExternalUser.UserId)
                {
                    return true;
                }
                else
                {
                    errorMessage =
                        "Select a different user type as you cannot have 2 Dedicated Authorised Signatory users for one local authority";

                    DisplayErrors(errorMessage, ExternalUserTypeId);

                    ErrorExternalUserType = true;
                    return false;
                }
            }

            DisplayErrors(errorMessage, ExternalUserTypeId);

            ErrorExternalUserType = true;
            return false;
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

        public void PageDataInitialise()
        {
            Permissions = new Extensions.Permissions.ExternalUser(httpContextAccessor, httpClientFactory, TempData);

            LocalAuthorities = GetLocalAuthoritiesAsync().Result;

            CreateLocalAuthoritiesSelectList();

            CreateUserTypesList();

            TempData.Put(TempDataKeys.LocalAuthorities, LocalAuthorities);
        }

        private async Task<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>> GetLocalAuthoritiesAsync()
        {
            try
            {
                List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>? result = [];

                HttpClient httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(Services.LocalAuthorityApi.Route);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>();
                }

                return result ?? [];
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, $"GetLocalAuthoritiesAsync, Error {ex.Message}");

                var message = "An issue occurred retrieving the data.";

                DisplayErrors(message, string.Empty);

                throw;
            }
        }

        private void CreateLocalAuthoritiesSelectList()
        {
            if (LocalAuthorities is null) return;

            List<SelectListItem> list =
            [
                .. LocalAuthorities.Select(a => new SelectListItem
                {
                    Value = a.OnsCode,
                    Text = a.Name,
                    Selected = a.OnsCode == SelectedLa
                }).OrderBy(x => x.Text),
            ];

            list.Insert(0, new SelectListItem(LocalAuthoritySelectInstruction, LocalAuthoritySelectInstruction, SelectedUserType == LocalAuthoritySelectInstruction));

            LaSelectList = list;
        }

        private void CreateUserTypesList()
        {


            List<SelectListItem> list = new List<SelectListItem>();

            foreach (ExternalUserType type in Enum.GetValues(typeof(ExternalUserType)))
            {
                list.Add(new SelectListItem(type.GetDescription(), ((int)type).ToString()));
            }
            if (((int)ExternalUser.UserType).ToString() != "0")
            {
                SelectedUserType = ((int)ExternalUser.UserType).ToString();
                list.Insert(0, new SelectListItem(ExternalUserTypeIsMandatory, "0", false));
                var item = list.Single(x => x.Value == ((int)ExternalUser.UserType).ToString());
                item.Selected = true;
            }

            UserTypes = list;
        }
    }
}
