namespace SocialCareCaseViewerApi.V1.Domain
{
    //TODO: this name could be better
    public class OtherName
    {
        private string _firstName;

        private string _lastName;

        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }
    }
}
