using System.Globalization;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class ThreePartDate2
    {
        /// <summary>
        /// A second copy of the 3 part date control as all controls require a unique id to map the data correctly
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="title">Title to be shown at the top of the control</param>
        /// <param name="firstHint">First hint to shown below the title</param>
        /// <param name="secondHint">Second hint to be shown below the first hint</param>
        /// <param name="titleToBeUsedInErrorMessage">Alternative title to be used in any error messages</param>
        /// <param name="showTheHighlightBar">Is the red bar on the left hand side of the control required</param>
        public ThreePartDate2(DateTime source, string title, string firstHint, string secondHint, string titleToBeUsedInErrorMessage, bool showTheHighlightBar = true)
        {
            Day2 = source.Day;
            Month2 = source.Month;
            Year2 = source.Year;

            Title2 = title;

            TitleToBeUsedInErrorMessage2
                = string.IsNullOrEmpty(titleToBeUsedInErrorMessage)
                    ? Title2
                    : titleToBeUsedInErrorMessage;

            FirstHint2 = firstHint;
            SecondHint2 = secondHint;
            ErrorMessage2 = string.Empty;
            ShowTheHighlightBar2 = showTheHighlightBar;
        }

        public ThreePartDate2()
        {
            Title2 = string.Empty;
            TitleToBeUsedInErrorMessage2 = string.Empty;
            FirstHint2 = string.Empty;
            SecondHint2 = string.Empty;
            ErrorMessage2 = string.Empty;
        }

        public int Day2{ get; init; }
        public int Month2 { get; init; }
        public int Year2 { get; init; }

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
            if (AllFieldsEmpty()) return true;

            if (TwoOfThreeFieldsEmpty()) return true;

            if (MissingDay()) return true;

            if (DayMinAndMax()) return true;

            if (MissingMonth()) return true;

            if (MonthMinAndMax()) return true;

            if (MissingYear()) return true;

            if (YearMin()) return true;

            if (Month31DaysCheck()) return true;

            var provider = CultureInfo.InvariantCulture;
            const string format = "yyyy-MM-dd";
            var dateString = $"{Year2}-{Month2:D2}-{Day2:D2}";

            return LeapYearCheck(dateString, format, provider, out var hasErrors) 
                ? hasErrors 
                : InvalidDateCheck(dateString, format, provider);
        }

        private bool InvalidDateCheck(string dateString, string format, CultureInfo provider)
        {
            try
            {
                _ = DateTime.ParseExact(dateString, format, provider);
                return false;
            }
            catch (FormatException)
            {
                HasError2 = true;
                HasDay2Error = true;
                HasMonth2Error = true;
                HasYear2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorInvalidDate;
                return true;
            }
        }

        private bool LeapYearCheck(string dateString, string format, CultureInfo provider, out bool hasErrors)
        {
            // leap Year2 check
            if (Month2 == 2 && Day2 > 28)
            {
                if (!DateTime.IsLeapYear(Year2))
                {
                    HasError2 = true;
                    HasDay2Error = true;
                    HasMonth2Error = true;
                    HasYear2Error = true;
                    ErrorMessage2 = ThreePartDateErrorMessages.ErrorInvalidLeapYear;
                    {
                        hasErrors = true;
                        return true;
                    }
                }

                try
                {
                    _ = DateTime.ParseExact(dateString, format, provider);
                    {
                        hasErrors = false;
                        return true;
                    }
                }
                catch (FormatException)
                {
                    HasError2 = true;
                    HasDay2Error = true;
                    HasMonth2Error = true;
                    HasYear2Error = true;
                    ErrorMessage2 = ThreePartDateErrorMessages.ErrorInvalidLeapYear;
                    {
                        hasErrors = true;
                        return true;
                    }
                }
            }

            hasErrors = false;
            return false;
        }

        private bool Month31DaysCheck()
        {
            // check for February, April, June, September, November. 
            if (Month2 is 2 or 4 or 6 or 9 or 11 && Day2 > 30)
            {
                HasError2 = true;
                HasDay2Error = true;
                HasMonth2Error = true;
                HasYear2Error = true;
                ErrorMessage2 = "The date contains a Month that cannot have 31 Days.";
                return true;
            }

            return false;
        }

        private bool YearMin()
        {
            if (Year2 < 1000)  // less than 4 Digit Year2 is not allowed
            {
                HasError2 = true;
                HasYear2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorInvalidYear;
                return true;
            }

            return false;
        }

        private bool MissingYear()
        {
            if (Year2 == 0)
            {
                HasError2 = true;
                HasYear2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorEmptyYear(TitleToBeUsedInErrorMessage2);
                return true;
            }

            return false;
        }

        private bool MonthMinAndMax()
        {
            if (Month2 is < 1 or > 12)
            {
                HasError2 = true;
                HasMonth2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorInvalidMonth;
                return true;
            }

            return false;
        }

        private bool MissingMonth()
        {
            if (Month2 == 0)
            {
                HasError2 = true;
                HasMonth2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorEmptyMonth(TitleToBeUsedInErrorMessage2);
                return true;
            }

            return false;
        }

        private bool DayMinAndMax()
        {
            if (Day2 is < 1 or > 31)
            {
                HasError2 = true;
                HasDay2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorInvalidDay;
                return true;
            }

            return false;
        }

        private bool MissingDay()
        {
            if (Day2 == 0)
            {
                HasError2 = true;
                HasDay2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorEmptyDay(TitleToBeUsedInErrorMessage2);
                return true;
            }

            return false;
        }

        private bool TwoOfThreeFieldsEmpty()
        {
            if (Day2 == 0 && Month2 == 0 || Day2 == 0 && Year2 == 0 || Month2 == 0 && Year2 == 0)
            {
                HasError2 = true;
                HasDay2Error = true;
                HasMonth2Error = true;
                HasYear2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorTwoEmptyBox(TitleToBeUsedInErrorMessage2);
                return true;
            }

            return false;
        }

        private bool AllFieldsEmpty()
        {
            if (Day2 == 0 && Month2 == 0 && Year2 == 0)
            {
                HasError2 = true;
                HasDay2Error = true;
                HasMonth2Error = true;
                HasYear2Error = true;
                ErrorMessage2 = ThreePartDateErrorMessages.ErrorTwoEmptyBox(TitleToBeUsedInErrorMessage2);
                return true;
            }

            return false;
        }
    }
}
