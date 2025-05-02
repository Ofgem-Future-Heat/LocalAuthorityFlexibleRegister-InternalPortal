using System.Globalization;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class ThreePartDateNullable
    {
        public ThreePartDateNullable(DateTime? source, string title, string firstHint, string secondHint, string titleToBeUsedInErrorMessage, bool showTheHighlightBar = true)
        {
            Day = source?.Day;
            Month = source?.Month;
            Year = source?.Year;

            Title = title;
            TitleToBeUsedInErrorMessage = string.IsNullOrEmpty(titleToBeUsedInErrorMessage) ? Title : titleToBeUsedInErrorMessage;
            FirstHint = firstHint;
            SecondHint = secondHint;
            ErrorMessage = string.Empty;
            ShowTheHighlightBar = showTheHighlightBar;
        }

        public ThreePartDateNullable()
        {
            Title = string.Empty;
            TitleToBeUsedInErrorMessage = string.Empty;
            FirstHint = string.Empty;
            SecondHint = string.Empty;
            ErrorMessage = string.Empty;
        }

        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

        public bool HasError { get; set; }
        public bool HasDayError { get; set; }
        public bool HasMonthError { get; set; }
        public bool HasYearError { get; set; }
        public string ErrorMessage { get; set; }

        public bool ShowTheHighlightBar { get; init; }

        public string Title { get; init; }
        public string TitleToBeUsedInErrorMessage { get; init; }
        public string FirstHint { get; init; }
        public string SecondHint { get; init; }

        public bool HasErrors()
        {
            if (IsDateEmpty())
            {
                SetErrorState(true, true, true, true, ThreePartDateErrorMessages.ErrorTwoEmptyBox(TitleToBeUsedInErrorMessage));
                return true;
            }

            if (IsPartialDateEmpty())
            {
                SetErrorState(true, true, true, true, ThreePartDateErrorMessages.ErrorTwoEmptyBox(TitleToBeUsedInErrorMessage));
                return true;
            }

            if (IsDayInvalid())
            {
                SetErrorState(true, true, false, false, ThreePartDateErrorMessages.ErrorEmptyDay(TitleToBeUsedInErrorMessage));
                return true;
            }

            if (IsMonthInvalid())
            {
                SetErrorState(true, false, true, false, ThreePartDateErrorMessages.ErrorEmptyMonth(TitleToBeUsedInErrorMessage));
                return true;
            }

            if (IsYearInvalid())
            {
                SetErrorState(true, false, false, true, ThreePartDateErrorMessages.ErrorEmptyYear(TitleToBeUsedInErrorMessage));
                return true;
            }

            if (IsDayOutOfRange())
            {
                SetErrorState(true, true, false, false, ThreePartDateErrorMessages.ErrorInvalidDay);
                return true;
            }

            if (IsMonthOutOfRange())
            {
                SetErrorState(true, false, true, false, ThreePartDateErrorMessages.ErrorInvalidMonth);
                return true;
            }

            if (IsYearOutOfRange())
            {
                SetErrorState(true, false, false, true, ThreePartDateErrorMessages.ErrorInvalidYear);
                return true;
            }

            if (IsInvalidDate())
            {
                SetErrorState(true, true, true, true, "The date contains a month that cannot have 31 days.");
                return true;
            }

            if (IsLeapYearInvalid())
            {
                SetErrorState(true, true, true, true, ThreePartDateErrorMessages.ErrorInvalidLeapYear);
                return true;
            }

            return !IsValidDate();
        }

        private bool IsDateEmpty() => (Day == 0 && Month == 0 && Year == 0) || (Day == null && Month == null && Year == null);

        private bool IsPartialDateEmpty() => (Day == 0 && Month == 0) || (Day == 0 && Year == 0) || (Month == 0 && Year == 0) ||
                                             (Day == null && Month == null) || (Day == null && Year == null) || (Month == null && Year == null);

        private bool IsDayInvalid() => Day == null || Day == 0;

        private bool IsMonthInvalid() => Month == null || Month == 0;

        private bool IsYearInvalid() => Year == null || Year == 0;

        private bool IsDayOutOfRange() => Day is < 1 or > 31;

        private bool IsMonthOutOfRange() => Month is < 1 or > 12;

        private bool IsYearOutOfRange() => Year < 1000;

        private bool IsInvalidDate() => Month is 2 or 4 or 6 or 9 or 11 && Day > 30;

        private bool IsLeapYearInvalid() => Month == 2 && Day > 28 && Year != null && !DateTime.IsLeapYear((int)Year);

        private bool IsValidDate()
        {
            var provider = CultureInfo.InvariantCulture;
            var format = "yyyy-MM-dd";
            var dateString = $"{Year}-{Month:D2}-{Day:D2}";

            try
            {
                _ = DateTime.ParseExact(dateString, format, provider);
                return true;
            }
            catch (FormatException)
            {
                SetErrorState(true, true, true, true, ThreePartDateErrorMessages.ErrorInvalidDate);
                return false;
            }
        }

        private void SetErrorState(bool hasError, bool hasDayError, bool hasMonthError, bool hasYearError, string errorMessage)
        {
            HasError = hasError;
            HasDayError = hasDayError;
            HasMonthError = hasMonthError;
            HasYearError = hasYearError;
            ErrorMessage = errorMessage;
        }
    }
}
