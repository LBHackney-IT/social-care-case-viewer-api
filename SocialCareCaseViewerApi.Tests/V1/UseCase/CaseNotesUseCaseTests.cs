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
        private Mock<IHistoricalDataGateway> _historicalDataGateway = null!;
        private CaseNotesUseCase _caseNotesUseCase = null!;

        [SetUp]
        public void SetUp()
        {
            _historicalDataGateway = new Mock<IHistoricalDataGateway>();
            _caseNotesUseCase = new CaseNotesUseCase(_historicalDataGateway.Object);
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1L };

            _historicalDataGateway.Setup(x => x.GetCaseNotesByPersonId(It.IsAny<long>())).Returns(new List<CaseNote>());

            _caseNotesUseCase.ExecuteGetByPersonId(request.Id);

            _historicalDataGateway.Verify(x => x.GetCaseNotesByPersonId(It.IsAny<long>()));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWithParameterWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1L };

            var personId = Convert.ToInt64(request.Id);

            _historicalDataGateway.Setup(x => x.GetCaseNotesByPersonId(personId)).Returns(new List<CaseNote>());

            _caseNotesUseCase.ExecuteGetByPersonId(request.Id);

            _historicalDataGateway.Verify(x => x.GetCaseNotesByPersonId(personId));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWhenNoteIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1 };

            _historicalDataGateway.Setup(x => x.GetCaseNoteById(It.IsAny<long>())).Returns(new CaseNote());

            var response = _caseNotesUseCase.ExecuteGetById(request.Id.ToString());

            Assert.IsInstanceOf<CaseNoteResponse>(response);

            _historicalDataGateway.Verify(x => x.GetCaseNoteById(It.IsAny<long>()));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWithParameterWhenNoteIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = 1 };

            var caseNoteId = Convert.ToInt64(request.Id);

            _historicalDataGateway.Setup(x => x.GetCaseNoteById(caseNoteId)).Returns(new CaseNote());

            var response = _caseNotesUseCase.ExecuteGetById(request.Id.ToString());

            Assert.IsInstanceOf<CaseNoteResponse>(response);

            _historicalDataGateway.Verify(x => x.GetCaseNoteById(caseNoteId));
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
            _historicalDataGateway.Setup(x => x.GetCaseNoteById(It.IsAny<long>())).Returns(default(CaseNote?));

            Action act = () => _caseNotesUseCase.ExecuteGetById("1");

            act.Should().Throw<CaseNoteNotFoundException>();
        }
    }
}
