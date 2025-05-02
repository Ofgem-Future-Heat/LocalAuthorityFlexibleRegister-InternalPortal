using Ofgem.LAF.SharedLibrary.Models;
using System.Text;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Services
{
    public interface IBasicValidationService
    {
        Tuple<bool, List<string>, string> Validate(byte[] fileBytes);
    }

    public class BasicValidationService(
        ILogger<BasicValidationService> logger,
        IHttpClientFactory httpClientFactory)
        : IBasicValidationService
    {
        public Tuple<bool, List<string>, string> Validate(byte[] fileBytes)
        {
            try
            {
                logger.LogLafInformation(LogEvents.ValidateFile);

                var rawDeclarations = Ofgem.LAF.SharedLibrary.Extensions.Declaration.ConvertToObject(fileBytes);

                // 1. blank or N/A urn - reject file LA_DURN_001_Declaration_Unique_Reference_Number_Has_A_Value
                if (BlankUrn_Rule(rawDeclarations).Any() || NotApplicableUrn_Rule(rawDeclarations).Any())
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            Constants.LA_DURN_001_DECLARATION_UNIQUE_REFERENCE_NUMBER_IS_BLANK
                        ],
                        string.Empty
                    );
                }

                // 2. Single Submitting Local Authorities Rule - reject file
                if (!Single_Submitting_Local_Authorities_Rule(rawDeclarations))
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            Constants.LA_DURN_004_SINGLE_DECLARATION_UNIQUE_REFERENCE_NUMBER
                        ],
                        string.Empty
                    );
                }

                // 3. duplicate URN - reject file DU_DECL_001_Urn_Is_Unique
                var duplicateUrn = DuplicateUrn_Rule(rawDeclarations).ToList();

                if (duplicateUrn.Count > 0)
                {
                    return new Tuple<bool, List<string>, string>(false, duplicateUrn!, string.Empty);
                }

                // 4. invalid format urn - reject file LA_DURN_002_Urn_Is_Formatted_correctly
                if (InvalidUrnFormat_Rule(rawDeclarations) > 0)
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            "Incorrect Unique Reference Number format in column LA_Declaration_Unique_Reference_Number, please use the accepted format ANNNNNNNN-NNNNN"
                        ],
                        string.Empty
                    );
                }

                // 5. validate the values are either YES or NO RM_OUT_001_Referral_Made_Outside_Of_LAs_Remit
                var success = Referral_Made_Outside_Of_Remit_Rule(rawDeclarations);

                if (!success)
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            Constants.RM_OUT_001_Referral_Made_Outside_Of_LAs_Remit_Is_InValid
                        ],
                        string.Empty
                    );
                }

                // 6. ONSCode is known within the DB LA_ACODE_003_LA_Area_Code_Must_Exist
                var onsCodeExists = OnsCode_Part_Is_In_The_DB_Rule(rawDeclarations[0].LA_Declaration_Unique_Reference_Number);

                if (!onsCodeExists.Result)
                {
                    return new Tuple<bool, List<string>, string>(
                        false,
                        [
                            "Please note this file cannot be uploaded because the submitted Local Authority for this declaration does not exist in the Profile database. Please add this Local Authority in the Profile section and re-upload the declaration."
                        ],
                        string.Empty
                    );
                }

                // 7. run LA_ACODE_001_LA_Area_Code_Has_A_Value
                if (HasMissingLaAreaCode_Rule(rawDeclarations))
                {
                    return new Tuple<bool, List<string>, string>(false, [Constants.LA_ACODE_001_LA_Area_Code_Has_A_Value], string.Empty);
                }

                // 8. run LA_ACODE_002_LA_Area_Code_Has_Correct_Format
                if (HasBadlyFormattedLaAreaCode_Rule(rawDeclarations))
                {
                    return new Tuple<bool, List<string>, string>(false, [Constants.LA_ACODE_002_LA_Area_Code_Has_Correct_Format], string.Empty);
                }

                // 9. run LA_ACODE_003_LA_Area_Code_Must_Exist
                var laCodesExist = LocalAuthorityMustExist_Rule(rawDeclarations);

                if (!laCodesExist.Result)
                {
                    return new Tuple<bool, List<string>, string>(false, [Constants.LA_ACODE_003_LA_Area_Code_Must_Exist_Error], string.Empty);
                }

                // 10. run LA_ACODE_004_Area_Code_Entry_Is_Valid
                var laCodeCombination = LocalAuthorityCodeCombination_Rule(rawDeclarations);

                if (!laCodeCombination.Result)
                {
                    return new Tuple<bool, List<string>, string>(false, [Constants.LA_ACODE_004_Area_Code_Entry_Is_Valid_Error], string.Empty);
                }

                // 11. run LA_ACODE_005_Referral_Made_Outside_LA_Remit
                var referralOutsideLaRemitValid = ReferralMadeOutsideOfLARemitValid(rawDeclarations);

                if (!referralOutsideLaRemitValid)
                {
                    return new Tuple<bool, List<string>, string>(false, [Constants.LA_ACODE_005_Referral_Made_Outside_LA_Remit_Error], string.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(ex, LogEvents.ValidateFile);
                return new Tuple<bool, List<string>, string>(false, [], ex.Message);
            }

            return new Tuple<bool, List<string>, string>(true, [], string.Empty);
        }

        /// <summary>
        /// Valid format is A12345678
        /// Alpha 8 digits
        /// </summary>
        /// <param name="onsCode"></param>
        /// <returns>true or false</returns>
        public static bool OnsCodeIsValid(string? onsCode)
        {
            if (string.IsNullOrEmpty(onsCode)) return false;

            if (onsCode.Length != 9) return false;

            // verify that the first character is an alphabetic character
            var firstPart = onsCode.ToCharArray(0, 9);

            if (!char.IsLetter(firstPart[0])) return false;

            // verify that the last 8 characters of the first part are numeric
            for (var i = 1; i < firstPart.Length; i++)
            {
                if (!char.IsNumber(firstPart[i])) return false;
            }

            return true;
        }


        /// <summary>
        /// Valid format is A12345678-12345
        /// Alpha 8 digits hyphen 5 digits
        /// </summary>
        /// <param name="urn"></param>
        /// <returns>true or false</returns>
        private static bool ValidateUrnFormat(string? urn)
        {
            if (string.IsNullOrEmpty(urn)) return false;

            var splitUrn = urn.Split('-');
            if (splitUrn.Length != 2) return false;

            if (splitUrn[0].Length != 9) return false;

            // verify that the first character of the first part is an alphabetic character
            var firstPart = splitUrn[0].ToCharArray(0, 9);

            if (!char.IsLetter(firstPart[0])) return false;

            // verify that the last 8 characters of the first part are numeric
            for (var i = 1; i < firstPart.Length; i++)
            {
                if (!char.IsNumber(firstPart[i])) return false;
            }

            // verify second part has a length of 5
            if (splitUrn[1].Length != 5) return false;

            // numeric
            var secondPart = splitUrn[1].ToCharArray();

            for (var i = 0; i < secondPart.Length; i++)
            {
                if (!char.IsNumber(secondPart[i])) return false;
            }

            return true;
        }


        private async Task<bool> OnsCode_Part_Is_In_The_DB_Rule(string? source)
        {
            if (string.IsNullOrEmpty(source)) return false;

            var onsCode = source[..9];

            try
            {
                var httpClient = httpClientFactory.CreateClient(LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(LocalAuthorityApi.RouteOnsExists + onsCode);

                return httpResponseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogLafError(ex, LogEvents.ValidateFile, "OnsCode_Part_Is_In_The_DB_Rule");
                return false;
            }
        }
        

        private static bool Referral_Made_Outside_Of_Remit_Rule(List<RawDeclaration> rawDeclarations)
        {
            for (var i = 0; i < rawDeclarations.Count; i++)
            {
                if (string.IsNullOrEmpty(rawDeclarations[i].Referral_Made_Outside_Of_LAs_Remit)) return false;

                if (rawDeclarations[i].Referral_Made_Outside_Of_LAs_Remit.ToUpper() != "YES" &&
                    rawDeclarations[i].Referral_Made_Outside_Of_LAs_Remit.ToUpper() != "NO") return false;
            }

            return true;
        }
        private static int InvalidUrnFormat_Rule(List<RawDeclaration> rawDeclarations)
        {
            var counter = 0;

            for (var i = 0; i < rawDeclarations.Count; i++)
            {
                if (!ValidateUrnFormat(rawDeclarations[i].LA_Declaration_Unique_Reference_Number)) counter++;
            }

            return counter;
        }

        private static bool Single_Submitting_Local_Authorities_Rule(List<RawDeclaration> rawDeclarations)
        {
            var multipleUrns = rawDeclarations
                .Select(x => x.LA_Declaration_Unique_Reference_Number![..10].ToUpper())
                .Distinct().ToList();

            return multipleUrns.Count == 1;
        }

        private static List<string?> DuplicateUrn_Rule(List<RawDeclaration> rawDeclarations)
        {
            var errors = new List<string?>();
            var genericMessageOverride = false;

            var duplicateUrns = rawDeclarations.GroupBy(x => x.LA_Declaration_Unique_Reference_Number)
                .Where(group => group.Count() > 1)
                .Select(group => new
                {
                    rows = group.ToList()
                });

            var duplicateDetail = new StringBuilder();

            foreach (var group in duplicateUrns)
            {
                switch (group.rows.Count)
                {
                    case 2:
                        duplicateDetail.AppendLine($"Duplicate URN found in rows {group.rows[0].RowNumber + 1} and {group.rows[1].RowNumber + 1} of the CSV file, file is invalid");
                        break;
                    case 3:
                        duplicateDetail.AppendLine($"Duplicate URN found in rows {group.rows[0].RowNumber + 1}, {group.rows[1].RowNumber + 1} and {group.rows[2].RowNumber + 1} of the CSV file, file is invalid");
                        break;
                    case 4:
                        duplicateDetail.AppendLine($"Duplicate URN found in rows {group.rows[0].RowNumber + 1}, {group.rows[1].RowNumber + 1}, {group.rows[2].RowNumber + 1} and {group.rows[3].RowNumber + 1} of the CSV file, file is invalid");
                        break;
                    default:
                        duplicateDetail.AppendLine($"Duplicate URN found in the CSV file, file is invalid.");
                        genericMessageOverride = true;
                        break;
                }
            }

            if (genericMessageOverride)
            {
                errors = ["Duplicate URN identified in the CSV file, file is invalid."];
            }
            else
            {
                if (!string.IsNullOrEmpty(duplicateDetail.ToString()))
                {
                    errors = [.. duplicateDetail.ToString().Split(["\r\n", "\r", "\n"], StringSplitOptions.None)];

                    if (errors.Count > 5)
                    {
                        errors = ["Duplicate URN identified in the CSV file, file is invalid."];
                    }
                }
            }

            return errors;

        }


        private static bool HasBadlyFormattedLaAreaCode_Rule(List<RawDeclaration> rawDeclarations)
        {
            foreach (var rawDeclaration in rawDeclarations)
            {
                // verify LA_Area_Code has 9 characters
                if (rawDeclaration.LA_Area_Code!.Length != 9)
                {
                    return true;
                }

                // verify LA_Area_Code first character is an alphabetic character
                char[] firstcharacter = rawDeclaration.LA_Area_Code.ToCharArray(0, 8);
                if (!char.IsLetter(firstcharacter[0]))
                {
                    return true;
                }

                // verify LA_Area_Code first character last 7 characters are numeric
                foreach (char numericPart in firstcharacter[1..])
                {
                    if (!char.IsNumber(numericPart))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasMissingLaAreaCode_Rule(List<RawDeclaration> rawDeclarations)
        {
            foreach (var rawDeclaration in rawDeclarations)
            {
                if (string.IsNullOrWhiteSpace(rawDeclaration.LA_Area_Code)
                    || (rawDeclaration.LA_Area_Code!.ToUpper() == Constants.NotApplicable))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string?> BlankUrn_Rule(List<RawDeclaration> rawDeclarations)
        {
            var blankUrns = rawDeclarations.GroupBy(x => x.LA_Declaration_Unique_Reference_Number)
                .Where(group => string.IsNullOrWhiteSpace(group.Key))
                .Select(group => group.Key);

            return blankUrns;
        }

        private static IEnumerable<string?> NotApplicableUrn_Rule(List<RawDeclaration> rawDeclarations)
        {
            var notApplicableUrns = rawDeclarations.GroupBy(x => x.LA_Declaration_Unique_Reference_Number)
                .Where(group => group.Key == "N/A")
                .Select(group => group.Key);

            return notApplicableUrns;
        }

        private async Task<bool> LocalAuthorityMustExist_Rule(List<RawDeclaration> rawDeclarations)
        {
            var cache = new List<string>();

            foreach (var rawDeclaration in rawDeclarations)
            {
                if (cache.Contains(rawDeclaration.LA_Area_Code!)) continue;

                cache.Add(rawDeclaration.LA_Area_Code!);

                var result = await OnsCode_Part_Is_In_The_DB_Rule(rawDeclaration.LA_Area_Code);

                if (result == false) return false;
            }

            return true;
        }

        private async Task<bool> LocalAuthorityCodeCombination_Rule(List<RawDeclaration> rawDeclarations)
        {
            foreach (var rawDeclaration in rawDeclarations)
            {
                if (rawDeclaration.LA_Declaration_Unique_Reference_Number is null) return false;
                if (rawDeclaration.LA_Area_Code is null) return false;
                if (rawDeclaration.Referral_Made_Outside_Of_LAs_Remit is null) return false;

                var choice = rawDeclaration.Referral_Made_Outside_Of_LAs_Remit.ToUpper();
                var onsCode = rawDeclaration.LA_Declaration_Unique_Reference_Number[..9];

                if (choice == "YES") continue;

                if (rawDeclaration.LA_Area_Code == onsCode) continue;

                return false;
            }

            return true;
        }

        private bool ReferralMadeOutsideOfLARemitValid(List<RawDeclaration> rawDeclarations)
        {
            foreach (var rawDeclaration in rawDeclarations)
            {
                if (string.IsNullOrWhiteSpace(rawDeclaration.LA_Area_Code)) return false;
                if (string.IsNullOrWhiteSpace(rawDeclaration.LA_Declaration_Unique_Reference_Number)) return false;
                if (string.IsNullOrWhiteSpace(rawDeclaration.Referral_Made_Outside_Of_LAs_Remit)) return false;
                if (string.Equals(rawDeclaration.Referral_Made_Outside_Of_LAs_Remit, "no", StringComparison.OrdinalIgnoreCase)) continue;
                if (rawDeclaration.LA_Area_Code == rawDeclaration.LA_Declaration_Unique_Reference_Number[..9]) return false;
            }

            return true;
        }
    }
}
