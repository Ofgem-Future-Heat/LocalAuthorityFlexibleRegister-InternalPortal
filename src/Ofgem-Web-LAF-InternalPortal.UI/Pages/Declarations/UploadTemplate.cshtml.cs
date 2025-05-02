using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Expert
        )]
    public class UploadTemplateModel(
        Services.IBasicValidationService basicValidationService,
        Services.IRedactionService redactionService,
        ILogger<UploadTemplateModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : PageModel
    {
        [BindProperty]
        public IFormFile? SelectedUploadFile { get; set; }

        [BindProperty]
        public string? SubmissionNotes { get; set; }

        [BindProperty]
        public List<string> UploadErrors { get; set; } = [];

        [BindProperty]
        public string? SubmissionNotesError { get; set; } = string.Empty;

        [BindProperty]
        public string Message { get; set; } = string.Empty;


        [BindProperty] public bool HasMessage => Message.Length > 0;
        [BindProperty] public bool HasUploadError => UploadErrors.Count > 0;
        [BindProperty] public bool HasSubmissionNoteError => SubmissionNotesError?.Length > 0;

        [BindProperty] public Extensions.Permissions.UploadTemplate Permissions { get; set; } = new();

        public void OnGet()
        {
            logger.LogInformation("UploadTemplate - OnGet");

            Permissions = new Extensions.Permissions.UploadTemplate(httpContextAccessor, httpClientFactory, TempData);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            logger.LogInformation("UploadTemplate - OnPostAsync");

            Permissions = new Extensions.Permissions.UploadTemplate(httpContextAccessor, httpClientFactory, TempData);

            Message = string.Empty;
            UploadErrors = [];


            (var successfullyUploaded, var documentLocationDescriptor) = await UploadDocumentToDocumentStore();

            if (!successfullyUploaded) return Page();

            var document = documentLocationDescriptor;
            var items = document.Split("/");
            var fileContainer = items[0];
            var fileName = items[1];

            var redirectionActionResult = await Validate(fileContainer, fileName);

            return redirectionActionResult;
        }

        public async Task<IActionResult> OnGetValidate(string fileContainer, string fileName)
        {
            var redirectionActionResult = await Validate(fileContainer, fileName);
            return redirectionActionResult;
        }

        private async Task<IActionResult> Validate(string fileContainer, string fileName)
        {
            try
            {
                var validateBodyRequest = new ValidationRequest
                {
                    DocumentContainer = fileContainer,
                    DocumentId = fileName,
                    SubmissionNotes = SubmissionNotes,
                    Overwrite = false
                };

                var httpClientDeclaration = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

                var declarationResponse =
                    await httpClientDeclaration.PostAsJsonAsync(Services.DeclarationApi.RouteValidation, validateBodyRequest);

                if (!declarationResponse.IsSuccessStatusCode)
                {
                    logger.LogError(
                        "File upload page - failed to validate File: {FileName}, supplied by: {SupplierName}",
                        fileName, Permissions.Name);
                    Message =
                        $"File upload page - failed to validate File: {fileName}, supplied by: {Permissions.Name}";
                    return Page();
                }

                var declarationResult = await declarationResponse.Content.ReadFromJsonAsync<ValidationResponse>();

                if (declarationResult == null)
                {
                    logger.LogError(
                        "File upload page - failed to retrieve validation result for File: {FileName}, supplied by: {SupplierName}",
                        fileName, Permissions.Name);
                    Message =
                        $"File upload page - failed to retrieve validation result for File: {fileName}, supplied by: {Permissions.Name}";
                    return Page();
                }

                // Duplicates to be confirmed
                if (declarationResult.HasDuplicates)
                {
                    TempData.Put(TempDataKeys.DuplicateData, declarationResult);
                    TempData.Put(TempDataKeys.SubmissionNote, SubmissionNotes);

                    return RedirectToPage(LafPages.DuplicateEntries.ROUTE,
                        LafPages.DuplicateEntries.METHOD_DUPLICATE_DETECTED_ACTION);
                }

                var errorCount = declarationResult.Responses?.Sum(response => response.Result?.Count);

                // Upload completed successfully 
                return RedirectToPage(
                    LafPages.Dashboard.ROUTE,
                    LafPages.Dashboard.METHOD_SUCCESSFUL_UPLOAD,
                    new
                    {
                        declarationCount = declarationResult.Responses?.Count ?? 0,
                        declarationErrorCount = errorCount,
                        detailedUploadMessage = DetailedMessage(declarationResult)
                    });

            }

            catch (TaskCanceledException tce)
            {
                logger.LogError(tce,
                    "The processing of the upload file will continue in background, Message: {Message}", tce.Message);

                return RedirectToPage(
                    LafPages.Dashboard.ROUTE,
                    LafPages.Dashboard.METHOD_SUCCESSFUL_UPLOAD,
                    new
                    {
                        declarationCount = 0,
                        declarationErrorCount = 0,
                        displayMessage =
                            "The upload has been successful, due to the size of the file the processing will continue in background"
                    });

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UploadTemplate - OnPostAsync - Upload declarations to documents service error");
                Message = "The selected file could not be uploaded – try again";
            }

            return Page();

        }

        private async Task<(bool, string)> UploadDocumentToDocumentStore()
        {
            logger.LogInformation("UploadTemplate - OnPostAsync");

            Permissions = new Extensions.Permissions.UploadTemplate(httpContextAccessor, httpClientFactory, TempData);

            Message = string.Empty;
            UploadErrors = [];

            if (SelectedUploadFile == null)
            {
                UploadErrors.Add("Select an LA Flex file");
                return (false, "");
            }

            if (!SelectedUploadFile.FileName.EndsWith(".csv"))
            {
                UploadErrors.Add("The selected file must be a CSV.");
                return (false, "");
            }

            if (SelectedUploadFile.Length > 2048000)
            {
                UploadErrors.Add("The CSV must be smaller than 2MB");
                return (false, "");
            }

            if (!TextValidation.HasNoIllegalCharacters(SubmissionNotes))
            {
                SubmissionNotesError = "Invalid characters in the submission note.";

                return (false, "");
            }

            if (!TextValidation.IsValidCharacterCount(SubmissionNotes))
            {
                SubmissionNotesError = "Submission note must be 200 characters or less";

                return (false, "");
            }


            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    await SelectedUploadFile.CopyToAsync(memoryStream);

                    var fileBytes = memoryStream.ToArray();

                    var (isValid, reason, errorMessage) = basicValidationService.Validate(fileBytes);

                    if (!isValid)
                    {
                        UploadErrors = reason;

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            logger.LogInformation("File upload: {FileName} failed with error: {Message}", SelectedUploadFile.FileName, errorMessage);

                            UploadErrors.Add("Your declaration notification is not using the correct template. Please download the latest version of the Great British Insulation Scheme and ECO4 Flex declaration notification template.");

                            UploadErrors.Add(errorMessage);
                        }

                        return (false, "");
                    }

                    var file = new Models.FileToUploadRequest
                    {
                        UserId = Permissions.UserId,
                        FileName = SelectedUploadFile.FileName,
                        ContentData = redactionService.RedactPersonalInformation(fileBytes),
                        SubmissionNotes = SubmissionNotes,
                        SupplierName = Permissions.Name
                    };

                    var httpClientDocument = httpClientFactory.CreateClient(Services.DocumentApi.ApiName);

                    var documentResponse = await httpClientDocument.PostAsJsonAsync(Services.DocumentApi.Route, file);

                    documentResponse.EnsureSuccessStatusCode();

                    if (!documentResponse.IsSuccessStatusCode)
                    {
                        logger.LogError("File upload page - failed, File: {FileName}, supplied by: {SupplierName}",
                            file.FileName, file.SupplierName);
                        return (false, "");
                    }

                    if (documentResponse.Headers.Location == null)
                    {
                        var text =
                            $"File upload page - failed, File: {file.FileName}, supplied by: {file.SupplierName}";
                        logger.LogError("File upload page - failed, File: {FileName}, supplied by: {SupplierName}",
                            file.FileName, file.SupplierName);
                        Message = text;
                        return (false, "");
                    }

                    var location = documentResponse.Headers.Location.ToString();

                    var documentRecordResponse = await httpClientDocument.GetAsync(location);

                    if (!documentRecordResponse.IsSuccessStatusCode)
                    {
                        logger.LogError(
                            "File upload page - failed to retrieve the document record associated with File: {FileName}, supplied by: {SupplierName}",
                            file.FileName, file.SupplierName);
                        Message =
                            $"File upload page - failed to retrieve the document record associated with File: {file.FileName}, supplied by: {file.SupplierName}";
                        return (false, "");
                    }

                    var documentResult = await documentRecordResponse.Content.ReadFromJsonAsync<Models.Document>();

                    if (documentResult == null || documentResult.Description == null)
                    {
                        logger.LogError(
                            "File upload page - failed to retrieve the document record with File: {FileName}, supplied by: {SupplierName}",
                            file.FileName, file.SupplierName);
                        Message =
                            $"File upload page - failed to retrieve the document record with File: {file.FileName}, supplied by: {file.SupplierName}";
                        return (false, "");
                    }

                    return (true, documentResult.Description);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "UploadTemplate - OnPostAsync - Upload declarations to documents service error");
                    Message = "The selected file could not be uploaded – try again";
                }
            }

            return (false, "");
        }

        private static string DetailedMessage(ValidationResponse declarationResult)
        {
            string detailedMessage;
            if (declarationResult.Upload is { FailedCoreRulesCount: 0, FailedSoiRulesCount: 0, PassedAllRulesCount: 0 })
            {
                detailedMessage = "";
            }
            else
            {
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                detailedMessage = declarationResult.Upload?.Comments ?? "";
            }

            return detailedMessage;
        }


        public async Task<IActionResult> OnGetDuplicateSaveAndContinue()
        {
            logger.LogInformation("UploadTemplate - OnGetDuplicateSaveAndContinue");

            Permissions = new Extensions.Permissions.UploadTemplate(httpContextAccessor, httpClientFactory, TempData);

            var duplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);
            var submissionNote = TempData.Get<string>(TempDataKeys.SubmissionNote);

            try
            {
                var httpClientDeclaration = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);


                // delete the original upload
                var deleteResponse = await httpClientDeclaration.DeleteAsync($"{Services.DeclarationApi.RouteUploadDelete}/{duplicateData.UploadId}");
                if (!deleteResponse.IsSuccessStatusCode)
                {
                    logger.LogError("File upload page - OnGetDuplicateSaveAndContinue - failed to delete upload");
                    Message = "File upload page - OnGetDuplicateSaveAndContinue - failed to delete upload";
                }


                var validateBodyRequest = new ValidationRequest
                {
                    DocumentContainer = duplicateData.DcoumentContainer,
                    DocumentId = duplicateData.DocumentId,
                    SubmissionNotes = submissionNote,
                    Overwrite = true
                };


                var declarationResponse =
                    await httpClientDeclaration.PostAsJsonAsync(Services.DeclarationApi.RouteValidationAcceptAll,
                        validateBodyRequest);

                if (declarationResponse.IsSuccessStatusCode)
                {
                    var declarationResult = await declarationResponse.Content
                        .ReadFromJsonAsync<ValidationResponse>();
                    // Upload completed successfully 
                    return RedirectToPage(
                        LafPages.Dashboard.ROUTE,
                        LafPages.Dashboard.METHOD_SUCCESSFUL_UPLOAD,
                        new
                        {
                            declarationCount = 0,
                            declarationErrorCount = 0,
                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                            detailedUploadMessage = declarationResult?.Upload?.Comments ?? "",
                            displayMessage = "Upload successful, duplicates have been updated"
                        });
                }
            }

            catch (TaskCanceledException tce)
            {
                logger.LogError(tce,
                    "The processing of the upload file will continue in background, Message: {Message}", tce.Message);

                return RedirectToPage(
                    LafPages.Dashboard.ROUTE,
                    LafPages.Dashboard.METHOD_SUCCESSFUL_UPLOAD,
                    new
                    {
                        declarationCount = 0,
                        declarationErrorCount = 0,
                        displayMessage =
                            "The upload has been successful, due to the size of the file the processing will continue in background"
                    });

            }

            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "File upload page - OnGetDuplicateSaveAndContinue - failed to validate Container: {DcoumentContainer}, File: {DocumentId}",
                    duplicateData.DcoumentContainer, duplicateData.DocumentId);

                Message =
                    $"File upload page - OnGetDuplicateSaveAndContinue - failed to validate Container: {duplicateData.DcoumentContainer}, File: {duplicateData.DocumentId}";
            }

            return Page();
        }

        public async Task<IActionResult> OnGetSaveAndContinueIgnoreDuplicates()
        {
            logger.LogInformation("UploadTemplate - OnGetSaveAndContinueIgnoreDuplicates");

            Permissions = new Extensions.Permissions.UploadTemplate(httpContextAccessor, httpClientFactory, TempData);

            var duplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);
            var submissionNote = TempData.Get<string>(TempDataKeys.SubmissionNote);

            try
            {
                var httpClientDeclaration = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

                // delete the original upload
                var deleteResponse = await httpClientDeclaration.DeleteAsync($"{Services.DeclarationApi.RouteUploadDelete}/{duplicateData.UploadId}");
                if (!deleteResponse.IsSuccessStatusCode)
                {
                    logger.LogError("File upload page - OnGetSaveAndContinueIgnoreDuplicates - failed to delete upload");
                    Message = "File upload page - OnGetSaveAndContinueIgnoreDuplicates - failed to delete upload";
                }

                var validateBodyRequest = new ValidationRequest
                {
                    DocumentContainer = duplicateData.DcoumentContainer,
                    DocumentId = duplicateData.DocumentId,
                    SubmissionNotes = submissionNote,
                    Overwrite = true
                };

                var declarationResponse = await httpClientDeclaration.PostAsJsonAsync(Services.DeclarationApi.RouteValidationIgnoreDuplicates, validateBodyRequest);

                if (declarationResponse.IsSuccessStatusCode)
                {
                    var declarationResult = await declarationResponse.Content.ReadFromJsonAsync<ValidationResponse>();

                    // Upload completed successfully 
                    return RedirectToPage(
                        LafPages.Dashboard.ROUTE,
                        LafPages.Dashboard.METHOD_SUCCESSFUL_UPLOAD,
                        new
                        {
                            declarationCount = 0,
                            declarationErrorCount = 0,
                            detailedUploadMessage = declarationResult?.Upload?.Comments ?? "",
                            displayMessage = $"Upload successful, {duplicateData.Duplicates.Count} duplicate(s) have been ignored"
                        });
                }
            }

            catch (TaskCanceledException tce)
            {
                logger.LogError(tce,
                    "The processing of the upload file will continue in background, Message: {Message}", tce.Message);

                return RedirectToPage(
                    LafPages.Dashboard.ROUTE,
                    LafPages.Dashboard.METHOD_SUCCESSFUL_UPLOAD,
                    new
                    {
                        declarationCount = 0,
                        declarationErrorCount = 0,
                        displayMessage =
                            "The upload has been successful, due to the size of the file the processing will continue in background"
                    });

            }

            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "File upload page - OnGetSaveAndContinueIgnoreDuplicates - failed to validate Container: {DcoumentContainer}, File: {DocumentId}",
                    duplicateData.DcoumentContainer, duplicateData.DocumentId);

                Message =
                    $"File upload page - OnGetSaveAndContinueIgnoreDuplicates - failed to validate Container: {duplicateData.DcoumentContainer}, File: {duplicateData.DocumentId}";
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDuplicateCancel()
        {
            logger.LogInformation("UploadTemplate - OnGetDuplicateCancel");

            Permissions = new Extensions.Permissions.UploadTemplate(httpContextAccessor, httpClientFactory, TempData);

            var duplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);

            var httpClientDeclaration = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);
            var declarationResponse = await httpClientDeclaration.DeleteAsync($"{Services.DeclarationApi.RouteUploadDelete}/{duplicateData.UploadId}");

            if (declarationResponse.IsSuccessStatusCode)
            {
                Message = "Upload successfully cancelled";
            }
            else
            {
                logger.LogError("File upload page - OnGetDuplicateSaveAndContinue - failed to validate Container: {DcoumentContainer}, File: {DocumentId}", duplicateData.DcoumentContainer, duplicateData.DocumentId);
                Message = $"File upload page - OnGetDuplicateCancel - failed to cancel upload for Container: {duplicateData.DcoumentContainer}, File: {duplicateData.DocumentId}";
            }

            return Page();
        }
    }
}
