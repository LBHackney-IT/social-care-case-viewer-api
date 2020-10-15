using System.Text.RegularExpressions;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ValidatePostcode : IValidatePostcode
    {
        public bool Execute(string postcode)
        {
            const string postcodeRegex =
                "^((([A-PR-UWYZ][A-HK-Y]?[0-9][0-9]?)|(([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRV-Y]))) {0,}[0-9][ABD-HJLNP-UW-Z]{2})$";
            return postcode == null || Regex.IsMatch(postcode.ToUpper(), postcodeRegex);
        }
    }
}
