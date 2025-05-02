using Ofgem_Web_LAF_InternalPortal.Extensions;
using System.Globalization;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class ThreePartDateNullable2
    {
        public ThreePartDateNullable2(DateTime? source, string title, string firstHint, string secondHint, string titleToBeUsedInErrorMessage, bool showTheHighlightBar = true)
        {
            Day2 = source?.Day;
            Month2 = source?.Month;
            Year2 = source?.Year;

            Title2 = title;
            TitleToBeUsedInErrorMessage2 = string.IsNullOrEmpty(titleToBeUsedInErrorMessage) ? Title2 : titleToBeUsedInErrorMessage;
            FirstHint2 = firstHint;
            SecondHint2 = secondHint;
            ErrorMessage2 = string.Empty;
            ShowTheHighlightBar2 = showTheHighlightBar;
        }

        public ThreePartDateNullable2()
        {
            Title2 = string.Empty;
            TitleToBeUsedInErrorMessage2 = string.Empty;
            FirstHint2 = string.Empty;
            SecondHint2 = string.Empty;
            ErrorMessage2 = string.Empty;
        }

        public int? Day2 { get; set; }
        public int? Month2 { get; set; }
        public int? Year2 { get; set; }

        public bool HasError2 { get; set; }
        public bool HasDay2Error { get; set; }
        public bool HasMonth2Error { get; set; }
        public bool HasYear2Error { get; set; }
        public string ErrorMessage2 { get; set; }

        public bool ShowTheHighlightBar2 { get; init; }

        public string Title2 { get; init; }
        public string TitleToBeUsedInErrorMessage2 { get; init; }
        public string FirstHint2 { get; init; }
        public string SecondHint2 { get; init; }

        public bool HasErrors()
        {
            if (IsDateEmpty() || IsTwoPartsEmpty())
            {
                SetErrorState(true, true, true, true, ThreePartDateErrorMessages.ErrorTwoEmptyBox(TitleToBeUsedInErrorMessage2));
                return true;
            }

            if (IsDayInvalid())
            {
                SetErrorState(true, true, false, false, ThreePartDateErrorMessages.ErrorEmptyDay(TitleToBeUsedInErrorMessage2));
                return true;
            }

            if (IsMonthInvalid())
            {
                SetErrorState(true, false, true, false, ThreePartDateErrorMessages.ErrorEmptyMonth(TitleToBeUsedInErrorMessage2));
                return true;
            }

            if (IsYearInvalid())
            {
                SetErrorState(true, false, false, true, ThreePartDateErrorMessages.ErrorEmptyYear(TitleToBeUsedInErrorMessage2));
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

            if (IsInvalidDayForMonth())
            {
                SetErrorState(true, true, true, true, "The date contains a month that cannot have 31 days.");
                return true;
            }

            if (IsInvalidLeapYear())
            {
                SetErrorState(true, true, true, true, ThreePartDateErrorMessages.ErrorInvalidLeapYear);
                return true;
            }

            return !IsValidDate();
        }

        private bool IsDateEmpty() => (Day2 == 0 && Month2 == 0 && Year2 == 0) || (Day2 == null && Month2 == null && Year2 == null);

        private bool IsTwoPartsEmpty() => (Day2 == 0 && Month2 == 0) || (Day2 == 0 && Year2 == 0) || (Month2 == 0 && Year2 == 0) ||
                                          (Day2 == null && Month2 == null) || (Day2 == null && Year2 == null) || (Month2 == null && Year2 == null);

        private bool IsDayInvalid() => Day2 == null || Day2 == 0;

        private bool IsMonthInvalid() => Month2 == null || Month2 == 0;

        private bool IsYearInvalid() => Year2 == null || Year2 == 0;

        private bool IsDayOutOfRange() => Day2 is < 1 or > 31;

        private bool IsMonthOutOfRange() => Month2 is < 1 or > 12;

        private bool IsYearOutOfRange() => Year2 < 1000;

        private bool IsInvalidDayForMonth() => Month2 is 2 or 4 or 6 or 9 or 11 && Day2 > 30;

        private bool IsInvalidLeapYear() => Month2 == 2 && Day2 > 28 && Year2 != null && !DateTime.IsLeapYear((int)Year2);

        private bool IsValidDate()
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var format = "yyyy-MM-dd";
            var dateString = $"{Year2}-{Month2:D2}-{Day2:D2}";

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
            HasError2 = hasError;
            HasDay2Error = hasDayError;
            HasMonth2Error = hasMonthError;
            HasYear2Error = hasYearError;
            ErrorMessage2 = errorMessage;
        }
    }
}
