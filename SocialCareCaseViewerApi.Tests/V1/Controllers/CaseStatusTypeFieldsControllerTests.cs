using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.Tests.V1.Gateways;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseStatusTypeField = SocialCareCaseViewerApi.V1.Domain.CaseStatusTypeField;
using CaseStatusTypeFieldOption = SocialCareCaseViewerApi.V1.Domain.CaseStatusTypeFieldOption;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CaseStatusTypeFieldsControllerTest
    {
        private CaseStatusTypeFieldsController _caseStatusTypeFieldsController;
        private Mock<IGetCaseStatusFieldsUseCase> _mockCaseStatusesUseCase;
        // private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<IGetCaseStatusFieldsUseCase>();
            _caseStatusTypeFieldsController = new CaseStatusTypeFieldsController(_mockCaseStatusesUseCase.Object);
        }

        [Test]
        public void GetCaseStatusTypeFieldsByTypeReturns200WhenCaseStatusTypeIsFound()
        {
            var caseStatusType = new CaseStatusType() { Name = "Test", Description = "Test Type" };

            _mockCaseStatusesUseCase.Setup(x => x.Execute(It.IsAny<GetCaseStatusFieldsRequest>()))
                .Returns(new GetCaseStatusFieldsResponse()
                {
                    Fields = DatabaseGatewayTests.GetValidCaseStatusTypeFields(caseStatusType).ToResponse()
                });

            var response = _caseStatusTypeFieldsController.GetCaseStatusTypeFields("Test") as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(new GetCaseStatusFieldsResponse
            {
                Fields = new List<CaseStatusTypeField>
                {
                    new CaseStatusTypeField
                    {
                        Description = "Something",
                        Name = "someThing",
                        Options = new List<CaseStatusTypeFieldOption>
                        {
                            new CaseStatusTypeFieldOption
                            {
                                Description = "The first option", Name = "One"
                            },
                            new CaseStatusTypeFieldOption
                            {
                                Description = "The second option", Name = "Two"
                            }
                        }
                    }
                }
            });
        }

        [Test]
        public void GetCaseStatusTypeFieldsByTypeReturns404WhenCaseStatusTypeIsNotFound()
        {

            _mockCaseStatusesUseCase.Setup(x => x.Execute(It.IsAny<GetCaseStatusFieldsRequest>()))
                .Returns(new GetCaseStatusFieldsResponse()
                {
                    Fields = Enumerable.Empty<CaseStatusTypeField>()
                });

            var response = _caseStatusTypeFieldsController.GetCaseStatusTypeFields("Test") as ObjectResult;

            response?.StatusCode.Should().Be(404);
        }
    }
}
