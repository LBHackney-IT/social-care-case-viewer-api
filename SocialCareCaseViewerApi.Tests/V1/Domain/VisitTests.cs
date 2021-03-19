using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    public class VisitTests
    {

        private Visit _visits;

        [SetUp]
        public void SetUp()
        {
            _visits = new Visit();
        }

        [Test]
        public void VisitHasMosaicId()
        {
            Assert.IsNull(_visits.MosaicId);
        }

        [Test]
        public void VisitHasId()
        {
            Assert.IsNull(_visits.Id);
        }

        [Test]
        public void VisitHasCreatedBy()
        {
            Assert.IsNull(_visits.CreatedByEmail);
        }

        [Test]
        public void VisitHasCreatedOn()
        {
            Assert.IsNull(_visits.CreatedOn);
        }

        [Test]
        public void VisitHasTitle()
        {
            Assert.IsNull(_visits.Title);
        }

        [Test]
        public void VisitHasContent()
        {
            Assert.IsNull(_visits.Content);
        }
    }
}
