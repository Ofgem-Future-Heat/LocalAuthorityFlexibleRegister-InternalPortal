using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using System.Globalization;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
    )]
    public class DeclarationDetailEditModel(
        ILogger<DeclarationDetailEditModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        private const string DataError = "An issue occurred retrieving data";

        [BindProperty] public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public Guid DeclarationNoteId { get; set; } = Guid.Empty;

        [BindProperty(SupportsGet = true)]
        public Models.Declaration? Declaration { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.Declaration? DeclarationOriginal { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.DeclarationErrorFields DeclarationErrorFields { get; set; } = new();

        [BindProperty]
        public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public Extensions.Permissions.DeclarationDetailEdit Permissions { get; set; } = new();

        [BindProperty] public DateTime? DateOfHouseholderEligibility { get; set; }

        [BindProperty] public DateTime? DateOfStatementOfIntentPublication { get; set; }

        [BindProperty] public Models.ThreePartDate2? DateOfHouseholderEligibilityControl { get; set; }

        [BindProperty] public Models.ThreePartDate? DateOfStatementOfIntentPublicationControl { get; set; }


        public string DateFromMin { get; set; } = Constants.SOI_MINIMUM_DATE;

        public string DateFromMax { get; set; } = DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATETIME_END_OF_DAY);

        [BindProperty] public Choices DropdownChoices { get; set; } = new();

        public string[] Answers = new[] { "Yes", "No" };

        public bool FirstTimeNoShowError { get; set; }

        public string DateHint { get; set; } = Constants.ThreePartDate_Hint;


        private static DateTime ParseMeOrMinDate(string dateTimeIn)
        {
            if (DateTime.TryParse(dateTimeIn, new CultureInfo("en-GB"), out DateTime dateOhe))
            {
                return dateOhe;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("DeclarationDetailEditModel - OnGetWithId");

            Permissions = new Extensions.Permissions.DeclarationDetailEdit(httpContextAccessor, httpClientFactory, TempData);

            DeclarationId = declarationId;

            await RefreshData(declarationId);

            if (Declaration is null)
            {
                logger.LogError(null, "DeclarationDetailEditModel - OnGetWithId - declaration is null id: {declarationId}", declarationId);
                DisplayMessage = "An issue occurred retrieving data";
            }
            else
            {
                DeclarationOriginal = Declaration;

                DropdownChoices = new Choices();

                if (Declaration != null)
                {
                    if (Declaration.DateOfHouseholderEligibility != null)
                    {
                        DateOfHouseholderEligibility = ParseMeOrMinDate(Declaration.DateOfHouseholderEligibility);

                        DateOfHouseholderEligibilityControl = new Models.ThreePartDate2(
                            source: (DateTime)DateOfHouseholderEligibility,
                            title: "",
                            titleToBeUsedInErrorMessage: "Date of householder eligibility",
                            firstHint: DateHint,
                            secondHint: "",
                            showTheHighlightBar: false);
                    }

                    if (Declaration.DateOfStatementOfIntentPublication != null)
                    {
                        DateOfStatementOfIntentPublication = ParseMeOrMinDate(Declaration.DateOfStatementOfIntentPublication);

                        DateOfStatementOfIntentPublicationControl = new Models.ThreePartDate(
                            source: (DateTime)DateOfStatementOfIntentPublication,
                            title: "",
                            titleToBeUsedInErrorMessage: "SoI publication date",
                            firstHint: DateHint,
                            secondHint: "",
                            showTheHighlightBar: false);
                    }
                }

                FirstTimeNoShowError = true;
            }
        }


        public Task OnGetForChange(Guid declarationId)
        {
            try
            {
                logger.LogInformation("DeclarationDetailEditModel - OnGetForChange");

                DropdownChoices = new Choices();

                Permissions = new Extensions.Permissions.DeclarationDetailEdit(httpContextAccessor, httpClientFactory, TempData);

                Declaration = TempData.Get<Models.Declaration>(TempDataKeys.DeclarationEdit);
                DeclarationOriginal = TempData.Get<Models.Declaration>(TempDataKeys.DeclarationEditOriginal);

                DeclarationId = Declaration.DeclarationId;

                if (Declaration.DateOfHouseholderEligibility != null)
                {
                    DateOfHouseholderEligibility = ParseMeOrMinDate(Declaration.DateOfHouseholderEligibility);

                    DateOfHouseholderEligibilityControl = new Models.ThreePartDate2(
                        source: (DateTime)DateOfHouseholderEligibility,
                        title: "",
                        titleToBeUsedInErrorMessage: "Date of householder eligibility",
                        firstHint: "",
                        secondHint: "",
                        showTheHighlightBar: false);
                }

                if (Declaration.DateOfStatementOfIntentPublication != null)
                {
                    DateOfStatementOfIntentPublication = ParseMeOrMinDate(Declaration.DateOfStatementOfIntentPublication);

                    DateOfStatementOfIntentPublicationControl = new Models.ThreePartDate(
                        source: (DateTime)DateOfStatementOfIntentPublication,
                        title: "",
                        titleToBeUsedInErrorMessage: "SoI publication date",
                        firstHint: "",
                        secondHint: "",
                        showTheHighlightBar: false);
                }

                FirstTimeNoShowError = true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "DeclarationDetailEditModel - OnGetForChange - declaration id: {DeclarationId}", declarationId);
                DisplayMessage = DataError;
            }

            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostValidate(Guid declarationId)
        {
            logger.LogInformation("DeclarationDetailEditModel - OnPostValidate");

            Permissions = new Extensions.Permissions.DeclarationDetailEdit(httpContextAccessor, httpClientFactory, TempData);

            if (Declaration is null)
            {
                Console.WriteLine($@"RouteRunCoreRules - OnPostValidate - declaration is null id: {declarationId}", declarationId);
                logger.LogError(null, "RouteRunCoreRules - OnPostValidate - declaration is null id: {declarationId}", declarationId);
                DisplayMessage = DataError;
                return Page();
            }

            try
            {
                // page level rules
                PageLevelValidateErrors(
                    out var invalidDateOfHouseholderEligibility,
                    out var invalidDateOfStatementOfIntentPublication,
                    out var invalidSoiLinkMandatory,
                    out var invalidSoiLinkNotHttps);

                if (invalidSoiLinkNotHttps ||
                    invalidSoiLinkMandatory ||
                    invalidDateOfHouseholderEligibility ||
                    invalidDateOfStatementOfIntentPublication)
                {
                    PageLevelApplyErrors(
                        invalidDateOfHouseholderEligibility,
                        invalidDateOfStatementOfIntentPublication,
                        invalidSoiLinkMandatory,
                        invalidSoiLinkNotHttps);

                    return Page();
                }


                // api level rules
                var rawDeclaration = Models.Declaration.MapToRawDeclaration(Declaration);

                var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

                var httpResponseMessage = await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteRunCoreRules, rawDeclaration);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.RunRulesResponse>();

                    if (result != null)
                    {
                        Declaration.DeclarationErrors = Models.Declaration.MapToDeclarationError(result);

                        DeclarationErrorFields = new Models.DeclarationErrorFields(Declaration.DeclarationErrors);


                        if (DeclarationErrorFields.HasNoErrors)
                        {
                            TempData.Put(TempDataKeys.DeclarationEdit, Declaration);
                            TempData.Put(TempDataKeys.DeclarationEditOriginal, DeclarationOriginal);

                            return RedirectToPage(
                                LafPages.DeclarationDetailEditSummary.ROUTE,
                                LafPages.DeclarationDetailEditSummary.METHOD_GET_WITH_ID_ACTION);
                        }
                    }
                }
                else
                {
                    throw new BadHttpRequestException("RouteRunCoreRules - OnPostValidate : Failed");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "DeclarationDetailEditModel - OnPostValidate");
                DisplayMessage = DataError;
            }

            return Page();
        }

        private void PageLevelValidateErrors(
            out bool invalidDateOfHouseholderEligibility,
            out bool invalidDateOfStatementOfIntentPublication,
            out bool invalidSoiLinkMandatory,
            out bool invalidSoiLinkNotHttps)
        {
            invalidDateOfHouseholderEligibility = !ValidateDateOfHouseholderEligibilityControl();

            invalidDateOfStatementOfIntentPublication = !ValidateDateOfStatementOfIntentPublicationControl();

            invalidSoiLinkMandatory = UrlValidation.IsLinkEmpty(Declaration!.StatementOfIntentLink!);

            invalidSoiLinkNotHttps = !UrlValidation.IsLinkValid(Declaration!.StatementOfIntentLink!);
        }

        /// <summary>
        /// Add the relevant error message for the fields IF it does not already exist
        /// </summary>
        /// <param name="invalidDateOfHouseholderEligibility"></param>
        /// <param name="invalidDateOfStatementOfIntentPublication"></param>
        /// <param name="invalidSoiLinkMandatory"></param>
        /// <param name="invalidSoiLinkNotHttps"></param>
        private void PageLevelApplyErrors(
            bool invalidDateOfHouseholderEligibility,
            bool invalidDateOfStatementOfIntentPublication,
            bool invalidSoiLinkMandatory,
            bool invalidSoiLinkNotHttps)
        {
            if (Declaration is null) return;
            if (Declaration.DeclarationErrors is null)
            {
                DeclarationErrorFields = new Models.DeclarationErrorFields();
                Declaration.DeclarationErrors = new List<Ofgem.LAF.SharedLibrary.Models.DeclarationError>();
            }

            // add in any page level rule issue
            if (invalidDateOfHouseholderEligibility)
            {
                DeclarationErrorFields.DateOfHouseholderEligibility = true;
                Declaration.DeclarationErrors.Add(new Ofgem.LAF.SharedLibrary.Models.DeclarationError() { Message = DateOfHouseholderEligibilityControl!.ErrorMessage2 });
                DeclarationErrorFields.HasNoErrors = false;
            }

            if (invalidDateOfStatementOfIntentPublication)
            {
                DeclarationErrorFields.DateOfStatementOfIntentPublication = true;
                Declaration.DeclarationErrors.Add(new Ofgem.LAF.SharedLibrary.Models.DeclarationError() { Message = DateOfStatementOfIntentPublicationControl!.ErrorMessage });
                DeclarationErrorFields.HasNoErrors = false;
            }

            var errorMessage = "Column 'Statement_Of_Intent_Link' is a required field. Please refer to the Great British Insulation Scheme and ECO4 Flex Data Dictionary.";

            if (invalidSoiLinkMandatory)
            {
                DeclarationErrorFields.StatementOfIntentLink = true;
                Declaration.DeclarationErrors.Add(new Ofgem.LAF.SharedLibrary.Models.DeclarationError() { Message = errorMessage });
                DeclarationErrorFields.StatementOfIntentLinkErrorMessage = errorMessage;
                DeclarationErrorFields.HasNoErrors = false;
            }
            else
            {
                errorMessage = "The statement of Intent link must begin with https://";

                if (invalidSoiLinkNotHttps)
                {
                    DeclarationErrorFields.StatementOfIntentLink = true;
                    Declaration.DeclarationErrors.Add(new Ofgem.LAF.SharedLibrary.Models.DeclarationError() { Message = errorMessage });
                    DeclarationErrorFields.StatementOfIntentLinkErrorMessage = errorMessage;
                    DeclarationErrorFields.HasNoErrors = false;
                }
            }
        }

        private async Task RefreshData(Guid declarationId)
        {
            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{Services.DeclarationApi.Route}/{declarationId}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Models.Declaration>();

                    if (result != null)
                    {
                        Declaration = result;

                        DeclarationErrorFields = new Models.DeclarationErrorFields(Declaration.DeclarationErrors);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "DeclarationDetailEditModel - RefreshData");
                DisplayMessage = DataError;
            }
        }

        /// <summary>
        /// Validates the date control and sets the underlying model fields to the appropriate values
        /// </summary>
        /// <returns>true or false</returns>
        internal bool ValidateDateOfHouseholderEligibilityControl()
        {
            if (DateOfHouseholderEligibilityControl != null && DateOfHouseholderEligibilityControl.HasErrors())
            {
                DisplayMessage = DateOfHouseholderEligibilityControl.ErrorMessage2;
                return false;
            }

            DateOfHouseholderEligibility = new DateTime(
                DateOfHouseholderEligibilityControl!.Year2,
                DateOfHouseholderEligibilityControl!.Month2,
                DateOfHouseholderEligibilityControl!.Day2);

            Declaration!.DateOfHouseholderEligibility = ((DateTime)DateOfHouseholderEligibility).ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT);

            if (DateOfHouseholderEligibility < Convert.ToDateTime(DateFromMin, CultureInfo.InvariantCulture))
            {
                DisplayMessage = "A valid date must have a correct input for year.";
                DateOfHouseholderEligibilityControl.HasError2 = true;
                DateOfHouseholderEligibilityControl.HasDay2Error = true;
                DateOfHouseholderEligibilityControl.HasMonth2Error = true;
                DateOfHouseholderEligibilityControl.HasYear2Error = true;
                DateOfHouseholderEligibilityControl.ErrorMessage2 = DisplayMessage;
                return false;
            }

            if (DateOfHouseholderEligibility > Convert.ToDateTime(DateFromMax, CultureInfo.InvariantCulture))
            {
                DisplayMessage = $"{DateOfHouseholderEligibilityControl.TitleToBeUsedInErrorMessage2} cannot be after {DateTime.Now:dd/MM/yyyy}";
                DateOfHouseholderEligibilityControl.HasError2 = true;
                DateOfHouseholderEligibilityControl.HasDay2Error = true;
                DateOfHouseholderEligibilityControl.HasMonth2Error = true;
                DateOfHouseholderEligibilityControl.HasYear2Error = true;
                DateOfHouseholderEligibilityControl.ErrorMessage2 = DisplayMessage;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the date control and sets the underlying model fields to the appropriate values
        /// </summary>
        /// <returns>true or false</returns>
        internal bool ValidateDateOfStatementOfIntentPublicationControl()
        {
            if (DateOfStatementOfIntentPublicationControl != null && DateOfStatementOfIntentPublicationControl.HasErrors())
            {
                DisplayMessage = DateOfStatementOfIntentPublicationControl.ErrorMessage;
                return false;
            }

            DateOfStatementOfIntentPublication = new DateTime(
                DateOfStatementOfIntentPublicationControl!.Year,
                DateOfStatementOfIntentPublicationControl!.Month,
                DateOfStatementOfIntentPublicationControl!.Day);

            Declaration!.DateOfStatementOfIntentPublication = ((DateTime)DateOfStatementOfIntentPublication).ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT);

            if (DateOfStatementOfIntentPublication < Convert.ToDateTime(DateFromMin, CultureInfo.InvariantCulture))
            {
                DisplayMessage = "A valid date must have a correct input for year.";
                DateOfStatementOfIntentPublicationControl.HasError = true;
                DateOfStatementOfIntentPublicationControl.HasDayError = true;
                DateOfStatementOfIntentPublicationControl.HasMonthError = true;
                DateOfStatementOfIntentPublicationControl.HasYearError = true;
                DateOfStatementOfIntentPublicationControl.ErrorMessage = DisplayMessage;
                return false;
            }

            if (DateOfStatementOfIntentPublication > Convert.ToDateTime(DateFromMax, CultureInfo.InvariantCulture))
            {
                DisplayMessage = $"{DateOfStatementOfIntentPublicationControl.TitleToBeUsedInErrorMessage} cannot be after {DateTime.Now:dd/MM/yyyy}";
                DateOfStatementOfIntentPublicationControl.HasError = true;
                DateOfStatementOfIntentPublicationControl.HasDayError = true;
                DateOfStatementOfIntentPublicationControl.HasMonthError = true;
                DateOfStatementOfIntentPublicationControl.HasYearError = true;
                DateOfStatementOfIntentPublicationControl.ErrorMessage = DisplayMessage;
                return false;
            }

            return true;
        }



    }


    public class Choices
    {
        public List<SelectListItem> ECO4orGreatBritishInsulationSchemeFlexReferralRoute_List = new()
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "Route 1", Value = "Route 1" },
            new SelectListItem { Text = "Route 2", Value = "Route 2" },
            new SelectListItem { Text = "Route 3", Value = "Route 3" },
            new SelectListItem { Text = "Route 4 (ECO4 Flex only)", Value = "Route 4 (ECO4 Flex only)" }
        };

        public List<SelectListItem> Route2ProxiesRoute3UmbrellaConditions_List = new()
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "N/A", Value = "N/A" },
            new SelectListItem { Text = "Route 2 Proxy 1", Value = "Route 2 Proxy 1" },
            new SelectListItem { Text = "Route 2 Proxy 2", Value = "Route 2 Proxy 2" },
            new SelectListItem { Text = "Route 2 Proxy 3", Value = "Route 2 Proxy 3" },
            new SelectListItem { Text = "Route 2 Proxy 4", Value = "Route 2 Proxy 4" },
            new SelectListItem { Text = "Route 2 Proxy 5", Value = "Route 2 Proxy 5" },
            new SelectListItem { Text = "Route 2 Proxy 6", Value = "Route 2 Proxy 6" },
            new SelectListItem { Text = "Route 2 Proxy 7 PPM", Value = "Route 2 Proxy 7 PPM" },
            new SelectListItem { Text = "Route 2 Proxy 7 Non - PPM", Value = "Route 2 Proxy 7 Non - PPM" }
        };

        public List<SelectListItem> AdditionalRoute2Proxies_List = new()
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "N/A", Value = "N/A" },
            new SelectListItem { Text = "Route 2 Proxy 1", Value = "Route 2 Proxy 1" },
            new SelectListItem { Text = "Route 2 Proxy 2", Value = "Route 2 Proxy 2" },
            new SelectListItem { Text = "Route 2 Proxy 3", Value = "Route 2 Proxy 3" },
            new SelectListItem { Text = "Route 2 Proxy 4", Value = "Route 2 Proxy 4" },
            new SelectListItem { Text = "Route 2 Proxy 5", Value = "Route 2 Proxy 5" },
            new SelectListItem { Text = "Route 2 Proxy 6", Value = "Route 2 Proxy 6" },
            new SelectListItem { Text = "Route 2 Proxy 7 PPM", Value = "Route 2 Proxy 7 PPM" },
            new SelectListItem { Text = "Route 2 Proxy 7 Non - PPM", Value = "Route 2 Proxy 7 Non - PPM" }
        };

        public List<SelectListItem> StatementOfIntentPublishedFor_List = new()
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "ECO4 Flex", Value = "ECO4 Flex" }
        };
    }
}
