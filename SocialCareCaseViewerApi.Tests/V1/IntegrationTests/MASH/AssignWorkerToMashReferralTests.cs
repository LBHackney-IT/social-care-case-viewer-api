using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.MASH
{
    [TestFixture]
    public class CreateNewMashReferralTests : IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task SuccessfulPatchAssignsWorkerToReferral()
        {

        }
    }
}
