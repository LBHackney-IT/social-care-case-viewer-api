using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class FormDataTests
    {
        [Test]
        public void FormDataIncludesRequiredProperties()
        {
            var formData = new FormData
            {
                RecordId = "Record",
                FormName = "Form Name",
                PersonId = "Person",
                FirstName = "First",
                LastName = "Last",
                DateOfBirth = "1980-10-02",
                OfficerEmail = "Email",
                DateOfEvent = "2020-12-01",
                CaseFormTimestamp = "2018-11-03",
                CaseFormUrl = "Url"
            };

            formData.RecordId.Should().Be("Record");
            formData.FormName.Should().Be("Form Name");
            formData.PersonId.Should().Be("Person");
            formData.FirstName.Should().Be("First");
            formData.LastName.Should().Be("Last");
            formData.DateOfBirth.Should().Be("1980-10-02");
            formData.OfficerEmail.Should().Be("Email");
            formData.DateOfEvent.Should().Be("2020-12-01");
            formData.CaseFormTimestamp.Should().Be("2018-11-03");
            formData.CaseFormUrl.Should().Be("Url");
        }
    }
}
