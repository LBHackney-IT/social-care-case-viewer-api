using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class AuditTests
    {
        private Audit _audit;

        [SetUp]
        public void SetUp()
        {
            _audit = new Audit();
        }

        [Test]
        public void AuditHasId()
        {
            Assert.AreEqual(0, _audit.Id);
        }

        [Test]
        public void AuditHasTableName()
        {
            Assert.IsNull(_audit.TableName);
        }


        [Test]
        public void AuditHasEntityState()
        {
            Assert.IsNull(_audit.EntityState);
        }

        [Test]
        public void AuditHasDateTime()
        {
            Assert.AreEqual(DateTime.MinValue, _audit.DateTime);
        }

        [Test]
        public void AuditHasKeyValues()
        {
            Assert.IsNull(_audit.KeyValues);
        }

        [Test]
        public void AuditHasOldValues()
        {
            Assert.IsNull(_audit.OldValues);
        }

        [Test]
        public void AuditHasNewValues()
        {
            Assert.IsNull(_audit.NewValues);
        }
    }
}
