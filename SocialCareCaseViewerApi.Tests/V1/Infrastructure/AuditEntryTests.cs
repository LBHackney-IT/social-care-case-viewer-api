using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class AuditEntryTests
    {
        private AuditEntry _auditEntry;

        [SetUp]
        public void SetUp()
        {
            _auditEntry = new AuditEntry();
        }


        [Test]
        public void AuditEntryTableName()
        {
            Assert.IsNull(_auditEntry.TableName);
        }

        [Test]
        public void AuditEntryHasEntityState()
        {
            Assert.IsNull(_auditEntry.EntityState);
        }

        [Test]
        public void AuditEntryHasKeyValues()
        {
            Assert.IsNotNull(_auditEntry.KeyValues);
        }

        [Test]
        public void AuditEntryHasOldValues()
        {
            Assert.IsNotNull(_auditEntry.OldValues);
        }

        [Test]
        public void AuditEntryHasNewValues()
        {
            Assert.IsNotNull(_auditEntry.NewValues);
        }

        [Test]
        public void AuditEntryHasTemporaryProperties()
        {
            Assert.IsNotNull(_auditEntry.TemporaryProperties);
        }

        [Test]
        public void AuditEntryHasHasTemporaryProperties()
        {
            Assert.IsFalse(_auditEntry.HasTemporaryProperties);
        }

    }
}
