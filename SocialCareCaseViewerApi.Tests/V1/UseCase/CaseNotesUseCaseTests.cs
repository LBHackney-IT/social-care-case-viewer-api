using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CaseNotesUseCaseTests
    {
        private Mock<IHistoricalSocialCareGateway> _historicalSocialCareGateway = null!;
        private CaseNotesUseCase _caseNotesUseCase = null!;

        [SetUp]
        public void SetUp()
        {
            _historicalSocialCareGateway = new Mock<IHistoricalSocialCareGateway>();
            _caseNotesUseCase = new CaseNotesUseCase(_historicalSocialCareGateway.Object);
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1L };

            _historicalSocialCareGateway.Setup(x => x.GetAllCaseNotes(It.IsAny<long>())).Returns(new List<CaseNote>());

            _caseNotesUseCase.ExecuteGetByPersonId(request.Id);

            _historicalSocialCareGateway.Verify(x => x.GetAllCaseNotes(It.IsAny<long>()));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWithParameterWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1L };

            var personId = Convert.ToInt64(request.Id);

            _historicalSocialCareGateway.Setup(x => x.GetAllCaseNotes(personId)).Returns(new List<CaseNote>());

            _caseNotesUseCase.ExecuteGetByPersonId(request.Id);

            _historicalSocialCareGateway.Verify(x => x.GetAllCaseNotes(personId));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWhenNoteIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1 };

            _historicalSocialCareGateway.Setup(x => x.GetCaseNoteInformationById(It.IsAny<long>())).Returns(new CaseNote());

            var response = _caseNotesUseCase.ExecuteGetById(request.Id.ToString());

            Assert.IsInstanceOf<CaseNoteResponse>(response);

            _historicalSocialCareGateway.Verify(x => x.GetCaseNoteInformationById(It.IsAny<long>()));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWithParameterWhenNoteIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1 };

            var caseNoteId = Convert.ToInt64(request.Id);

            _historicalSocialCareGateway.Setup(x => x.GetCaseNoteInformationById(caseNoteId)).Returns(new CaseNote());

            var response = _caseNotesUseCase.ExecuteGetById(request.Id.ToString());

            Assert.IsInstanceOf<CaseNoteResponse>(response);

            _historicalSocialCareGateway.Verify(x => x.GetCaseNoteInformationById(caseNoteId));
        }

        [Test]
        public void ExecuteGetByIdThrowsFormatExceptionWhenNoteIdConversionFails()
        {
            var noteId = "not a number";

            Action act = () => _caseNotesUseCase.ExecuteGetById(noteId);

            act.Should().Throw<CaseNoteIdConversionException>().WithMessage($"Note id conversion failed for {noteId}");
        }

        [Test]
        public void ExcecureGetByIdThrowsCaseNoteNotFoundExceptionWhenNoteNotFound()
        {
            _historicalSocialCareGateway.Setup(x => x.GetCaseNoteInformationById(It.IsAny<long>())).Returns(default(CaseNote?));

            Action act = () => _caseNotesUseCase.ExecuteGetById("1");

            act.Should().Throw<CaseNoteNotFoundException>();
        }
    }
}
