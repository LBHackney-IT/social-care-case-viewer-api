using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    public class UpdateMashReferralTests
    {
        private UpdateMashReferralValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateMashReferralValidator();
        }

        [Test]
        public void WhenUpdateTypeIsInitialDecisionRequiresUrgentContactMustBeProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.RequiresUrgentContact = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide if urgent contact is required");
        }

        [Test]
        public void WhenUpdateTypeIsInitialDecisionReferralCategoryMustNotBeNull()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.ReferralCategory = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a referral category");
        }

        [Test]
        public void WhenUpdateTypeIsInitialDecisionReferralCategoryMustHaveALength()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.ReferralCategory = "";

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a referral category");
        }

        [Test]
        public void WhenUpdateTypeIsInitialDecisionADecisionMustNotBeNull()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.Decision = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a decision");
        }

        [Test]
        public void WhenUpdateTypeIsInitialDecisionDecisionMustHaveALength()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            request.Decision = "";

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a decision");
        }

        [Test]
        public void WhenUpdateTypeIsScreeningDecisionADecisionMustNotBeNull()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            request.Decision = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a decision");
        }

        [Test]
        public void WhenUpdateTypeIsScreeningDecisionDecisionMustHaveALength()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            request.Decision = "";

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a decision");
        }

        [Test]
        public void WhenUpdateTypeIsScreeningDecisionRequiresUrgentContactMustBeProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            request.RequiresUrgentContact = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide if urgent contact is required");
        }

        [Test]
        public void WhenUpdateTypeIsFinalDecisionADecisionMustNotBeNull()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
            request.Decision = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a decision");
        }

        [Test]
        public void WhenUpdateTypeIsFinalDecisionDecisionMustHaveALength()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
            request.Decision = "";

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a decision");
        }

        [Test]
        public void WhenUpdateTypeIsFinalDecisionRequiresUrgentContactMustBeProvided()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
            request.RequiresUrgentContact = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide if urgent contact is required");
        }

        [Test]
        public void WhenUpdateTypeIsFinalDecisionReferralCategoryMustNotBeNull()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
            request.ReferralCategory = null;

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a referral category");
        }

        [Test]
        public void WhenUpdateTypeIsFinalDecisionReferralCategoryMustHaveALength()
        {
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
            request.ReferralCategory = "";

            var validationResult = _validator.Validate(request);

            validationResult.ToString().Should().Be("Must provide a referral category");
        }
    }
}
