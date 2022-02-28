using System;
using System.Linq;
using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class AllocateResidentToTheTeamRequest
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        private AllocateResidentToTheTeamRequest GetValidRequest(){
            return new AllocateResidentToTheTeamRequest(){
               AllocatedTeamId = 2,


            };


        }
        [Test]        

    }
}