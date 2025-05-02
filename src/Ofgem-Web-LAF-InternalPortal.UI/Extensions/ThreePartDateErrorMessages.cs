namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    public static class ThreePartDateErrorMessages
    {
        public static string ErrorInvalidDay => "A valid date must have a correct input for Day.";
        public static string ErrorInvalidMonth => "A valid date must have a correct input for Month.";
        public static string ErrorInvalidYear => "A valid date must have a correct input for Year.";
        public static string ErrorInvalidLeapYear => "It is not a leap year. February only has 28 days.";
        public static string ErrorInvalidDate => "Please provide a valid date.";

        public static string ErrorEmptyDay(string title) => $"The date for '{title}' must include the Day.";
        public static string ErrorEmptyMonth(string title) => $"The date for '{title}' must include the Month.";
        public static string ErrorEmptyYear(string title) => $"The date for '{title}' must include the Year.";
        public static string ErrorTwoEmptyBox(string title) => $"The date for '{title}' cannot be left blank.";
    }
}