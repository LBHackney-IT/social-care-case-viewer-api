using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    [TestFixture]
    public class QueryOffsetTest
    {
        [Test]
        public void WhenThereCurrentOffsetIs0AndTotalRecordsIs30AndLimitIs20ShouldBe20()
        {
            var nextOffset =
                QueryOffset
                    .GetNextOffset(currentOffset: 0,
                    totalRecords: 30,
                    limit: 20);
            nextOffset.Should().Be(20);
        }

        [Test]
        public void WhenThereCurrentOffsetIs20AndTotalRecordsIs30AndLimitIs20ShouldBeNull()
        {
            var nextOffset =
                QueryOffset
                    .GetNextOffset(currentOffset: 20,
                    totalRecords: 30,
                    limit: 20);
            nextOffset.Should().Be(null);
        }

        [Test]
        public void WhenThereCurrentOffsetIs10AndTotalRecordsIs30AndLimitIs20ShouldBeNull()
        {
            var nextOffset =
                QueryOffset
                    .GetNextOffset(currentOffset: 10,
                    totalRecords: 30,
                    limit: 20);
            nextOffset.Should().Be(null);
        }

        [Test]
        public void WhenThereCurrentOffsetIs50AndTotalRecordsIs30AndLimitIs20ShouldBeNull()
        {
            var nextOffset =
                QueryOffset
                    .GetNextOffset(currentOffset: 50,
                    totalRecords: 30,
                    limit: 20);
            nextOffset.Should().Be(null);
        }
    }
}
