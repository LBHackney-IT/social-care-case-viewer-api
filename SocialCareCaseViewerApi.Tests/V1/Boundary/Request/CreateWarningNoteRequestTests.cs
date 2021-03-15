// using System;
// using AutoFixture;
// using FluentAssertions;
// using NUnit.Framework;
// using SocialCareCaseViewerApi.Tests.V1.Helpers;
// using SocialCareCaseViewerApi.V1.Boundary.Requests;

// namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
// {
//     [TestFixture]
//     public class CreateWarningNoteRequestTests
//     {
//         private CreateWarningNoteRequest _classUnderTest;
//         // private CreateWarningNoteRequest _request;
//         private Fixture _fixture;


//         [SetUp]
//         public void SetUp()
//         {
//             _classUnderTest = new CreateWarningNoteRequest();
//             // _request = GetValidCreateWarningNoteRequest();
//             _fixture = new Fixture();
//         }

//         [Test]
//         public void RequestHasPersonId()
//         {
//             _classUnderTest.PersonId.Should().Be(0);
//         }

//         [Test]
//         public void RequestHasStartDate()
//         {
//             _classUnderTest.StartDate.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasEndDate()
//         {
//             _classUnderTest.EndDate.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasIndividualNotified()
//         {
//             _classUnderTest.IndividualNotified.Should().Be(false);
//         }

//         [Test]
//         public void RequestHasNotificationDetails()
//         {
//             _classUnderTest.NotificationDetails.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasReviewDetails()
//         {
//             _classUnderTest.ReviewDetails.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasNoteType()
//         {
//             _classUnderTest.NoteType.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasStatus()
//         {
//             _classUnderTest.Status.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasDateInformed()
//         {
//             _classUnderTest.DateInformed.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasHowInformed()
//         {
//             _classUnderTest.HowInformed.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasWarningNarrative()
//         {
//             _classUnderTest.WarningNarrative.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasManagersName()
//         {
//             _classUnderTest.ManagersName.Should().Be(null);
//         }

//         [Test]
//         public void RequestHasDateManagerInformed()
//         {
//             _classUnderTest.DateManagerInformed.Should().Be(null);
//         }

//         #region Model validation
//         [Test]
//         public void ValidationFailsIfPersonIdIsNotBiggerThan0()
//         {
//             // _request.PersonId = 0;

//             var error = ValidationHelper.ValidateModel(_classUnderTest);

//             error.Count.Should().Be(1);
//             error.Should().Contain(x => x.ErrorMessage.Contains("Please enter a value bigger than 0"));
//         }

//         // [Test]
//         // public void ValidationFailsIfStartDateIsNotProvided()
//         // {
//         //     _request.StartDate = null;

//         //     var error = ValidationHelper.ValidateModel(_classUnderTest);

//         //     error.Count.Should().Be(1);
//         //     error.Should().Contain(x => x.ErrorMessage.Contains("Please enter a value bigger than 0"));
//         // }

//         #endregion

//         private CreateWarningNoteRequest GetValidCreateWarningNoteRequest()
//         {
//             return new CreateWarningNoteRequest()
//             {
//                 PersonId = _fixture.Create<long>(),
//                 StartDate = DateTime.Now,
//                 EndDate = DateTime.Now,
//                 IndividualNotified = false,
//                 NotificationDetails = _fixture.Create<string>(),
//                 ReviewDetails = _fixture.Create<string>(),
//                 NoteType = _fixture.Create<string>(),
//                 Status = _fixture.Create<string>(),
//                 DateInformed = DateTime.Now,
//                 HowInformed = _fixture.Create<string>(),
//                 WarningNarrative = _fixture.Create<string>(),
//                 ManagersName = _fixture.Create<string>(),
//                 DateManagerInformed = DateTime.Now
//             };
//         }
//     }
// }
