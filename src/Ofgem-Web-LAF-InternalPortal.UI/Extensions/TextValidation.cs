namespace Ofgem_Web_LAF_InternalPortal.Extensions
{
    public static class TextValidation
    {
        private const string IllegalCharacters = "\"<>%";

        public static bool HasNoIllegalCharacters(string? value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return !value.Intersect(IllegalCharacters).Any();
        }

        private static readonly System.Text.RegularExpressions.Regex AcceptableValidator = new(
            @"[^a-zA-Z1234567890,_'-]+",
            System.Text.RegularExpressions.RegexOptions.Compiled |
            System.Text.RegularExpressions.RegexOptions.CultureInvariant |
            System.Text.RegularExpressions.RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(500));

        private static readonly System.Text.RegularExpressions.Regex AcceptableProfileValidator = new(
            @"[^a-zA-Z1234567890, _'-]+",
            System.Text.RegularExpressions.RegexOptions.Compiled |
            System.Text.RegularExpressions.RegexOptions.CultureInvariant |
            System.Text.RegularExpressions.RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(500));

        public static bool IsValidName(string name)
        {
            return AcceptableValidator.IsMatch(name);
        }

        public static bool IsValidProfileName(string name)
        {
            return AcceptableProfileValidator.IsMatch(name);
        }

        public static bool IsValidCharacterCount(string? value)
        {
            if(string.IsNullOrEmpty(value)) { return true; }
            if(value.Length>200)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}
