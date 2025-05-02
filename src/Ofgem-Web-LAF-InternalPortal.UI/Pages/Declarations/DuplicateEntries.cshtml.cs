using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Expert
        )]
    public class DuplicateEntriesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;


        [BindProperty(SupportsGet = true)]
        public ValidationResponse DuplicateData { get; set; }

        [BindProperty]
        public string Message { get; set; }

        [BindProperty] public Extensions.Permissions.DuplicateEntries Permissions { get; set; }

        public DuplicateEntriesModel(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            DuplicateData = new ValidationResponse();
            Message = string.Empty;
            Permissions = new Extensions.Permissions.DuplicateEntries();
        }

        public Task OnGet()
        {
            if (_httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.DuplicateEntries(_httpContextAccessor, _httpClientFactory, TempData);

            return Task.CompletedTask;
        }

        public Task OnGetDuplicateDetected()
        {
            if (_httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.DuplicateEntries(_httpContextAccessor, _httpClientFactory, TempData);

            DuplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);
            string submissionNote = TempData.Get<string>(TempDataKeys.SubmissionNote);

            var duplicateModel = CreateDuplicateModel();

            TempData.Put(TempDataKeys.DuplicateData, DuplicateData);
            TempData.Put(TempDataKeys.DuplicateDataToAction, duplicateModel);
            TempData.Put(TempDataKeys.SubmissionNote, submissionNote);

            return Task.CompletedTask;
        }

        public Task<IActionResult> OnPostDuplicateSaveAndContinue()
        {
            DuplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);
            string submissionNote = TempData.Get<string>(TempDataKeys.SubmissionNote);

            TempData.Put(TempDataKeys.DuplicateData, DuplicateData);
            TempData.Put(TempDataKeys.SubmissionNote, submissionNote);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.UploadTemplate.ROUTE, LafPages.UploadTemplate.METHOD_SAVE_AND_CONTINUE_WITH_DUPLICATES));
        }

        public Task<IActionResult> OnPostSaveAndContinueIgnoreDuplicates()
        {
            DuplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);
            string submissionNote = TempData.Get<string>(TempDataKeys.SubmissionNote);

            TempData.Put(TempDataKeys.DuplicateData, DuplicateData);
            TempData.Put(TempDataKeys.SubmissionNote, submissionNote);

            return Task.FromResult<IActionResult>(
                RedirectToPage(LafPages.UploadTemplate.ROUTE,
                    LafPages.UploadTemplate.METHOD_SAVE_AND_CONTINUE_IGNORE_DUPLICATES)
            );
        }

        public IActionResult OnPostDuplicateCancel()
        {
            DuplicateData = TempData.Get<ValidationResponse>(TempDataKeys.DuplicateData);
            string submissionNote = TempData.Get<string>(TempDataKeys.SubmissionNote);

            TempData.Put(TempDataKeys.DuplicateData, DuplicateData);
            TempData.Put(TempDataKeys.SubmissionNote, submissionNote);

            return RedirectToPage(LafPages.UploadTemplate.ROUTE, LafPages.UploadTemplate.METHOD_DUPLICATE_CANCEL);
        }

        private DuplicateModel CreateDuplicateModel()
        {
            var duplicateModel =
                new DuplicateModel(
                    DuplicateData.DcoumentContainer,
                    DuplicateData.DocumentId,
                    DuplicateData.UploadId);

            if (DuplicateData.Duplicates == null) return duplicateModel;

            foreach (var duplicate in DuplicateData.Duplicates)
            {
                duplicateModel.UrnList.Add(duplicate.Urn);
            }

            return duplicateModel;
        }

    }
}