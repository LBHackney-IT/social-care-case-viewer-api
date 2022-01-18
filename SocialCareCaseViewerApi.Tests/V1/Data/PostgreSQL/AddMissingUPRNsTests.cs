using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Data.PostgreSQL
{
    [TestFixture]
    public class AddMissingUPRNsTests : DatabaseTests
    {
        private static readonly FormattableString _queryUnderTest = $@"
                UPDATE dbo.dm_addresses a
                SET
                    unique_id = u.uprn,
                    sccv_last_modified_by = 'uprn_update_script',
                    sccv_last_modified_at = NOW()
                FROM dbo.sccv_uprn_update u
                WHERE u.address_id = a.ref_addresses_people_id AND a.unique_id IS NULL;
            ";

        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void DoesNotUpdateUPRNorAuditColumnValuesIfAddressAlreadyHasUPRNValue()
        {
            var address = TestHelpers.CreateAddress(uprn: 123);
            var uprn = 456;

            DatabaseContext.Addresses.Add(address);

            DatabaseContext.UPRNupdates.Add(new UPRNupdate()
            {
                AddressId = address.PersonAddressId,
                UPRN = uprn
            });

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedAddress = DatabaseContext.Addresses.FirstOrDefault();

            updatedAddress.Uprn.Should().Be(address.Uprn);
            updatedAddress.LastModifiedBy.Should().Be(address.LastModifiedBy);
            updatedAddress.LastModifiedAt.Should().Be(address.LastModifiedAt);
        }

        [Test]
        public void UpdatesUPRNValueForMatchingRecord()
        {
            var address = TestHelpers.CreateAddress();
            address.Uprn = null;

            var uprn = 456;

            DatabaseContext.Addresses.Add(address);

            DatabaseContext.UPRNupdates.Add(new UPRNupdate()
            {
                AddressId = address.PersonAddressId,
                UPRN = uprn
            });

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedAddress = DatabaseContext.Addresses.FirstOrDefault();

            updatedAddress.Uprn.Should().Be(uprn);
        }

        [Test]
        public void UpdatesModifiedAtAndModifiedByAuditColumnValuesWhenUpdatingUPRNvalue()
        {
            var address = TestHelpers.CreateAddress();
            address.Uprn = null;
            var uprn = 456;

            DatabaseContext.Addresses.Add(address);

            DatabaseContext.UPRNupdates.Add(new UPRNupdate()
            {
                AddressId = address.PersonAddressId,
                UPRN = uprn
            });

            DatabaseContext.SaveChanges();

            var lastModifiedBy = "uprn_update_script";

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedAddress = DatabaseContext.Addresses.FirstOrDefault();

            updatedAddress.LastModifiedAt.Should().BeCloseTo(DateTime.Now, precision: 5000);
            updatedAddress.LastModifiedBy.Should().Be(lastModifiedBy);
        }

        [Test]
        public void UpdatesMultipleMatchingRecordsThatDoNotHaveUPRNvalue()
        {
            var address1WithoutUPRN = TestHelpers.CreateAddress();
            address1WithoutUPRN.Uprn = null;
            var address2WithoutUPRN = TestHelpers.CreateAddress();
            address2WithoutUPRN.Uprn = null;

            var address1WithUPRN = TestHelpers.CreateAddress(uprn: 123);
            var address2WithUPRN = TestHelpers.CreateAddress(uprn: 345);

            var addressesToUpdate = new List<Address>() { address1WithoutUPRN, address2WithoutUPRN, address1WithUPRN, address2WithUPRN };

            DatabaseContext.Addresses.AddRange(addressesToUpdate);

            var uprnUpdate1 = new UPRNupdate() { AddressId = address1WithoutUPRN.PersonAddressId, UPRN = 555 };
            var uprnUpdate2 = new UPRNupdate() { AddressId = address2WithoutUPRN.PersonAddressId, UPRN = 666 };
            var uprnUpdate3 = new UPRNupdate() { AddressId = address1WithUPRN.PersonAddressId, UPRN = 777 };
            var uprnUpdate4 = new UPRNupdate() { AddressId = address2WithUPRN.PersonAddressId, UPRN = 888 };

            var uprnUpdates = new List<UPRNupdate>() { uprnUpdate1, uprnUpdate2, uprnUpdate3, uprnUpdate4 };

            DatabaseContext.UPRNupdates.AddRange(uprnUpdates);
            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedAddressWithoutUPRN1 = DatabaseContext.Addresses.FirstOrDefault(x => x.PersonAddressId == address1WithoutUPRN.PersonAddressId);
            var updatedAddressWithoutUPRN2 = DatabaseContext.Addresses.FirstOrDefault(x => x.PersonAddressId == address2WithoutUPRN.PersonAddressId);
            var updatedAddressWithUPRN1 = DatabaseContext.Addresses.FirstOrDefault(x => x.PersonAddressId == address1WithUPRN.PersonAddressId);
            var updatedAddressWithUPRN2 = DatabaseContext.Addresses.FirstOrDefault(x => x.PersonAddressId == address2WithUPRN.PersonAddressId);

            updatedAddressWithoutUPRN1.Uprn.Should().Be(uprnUpdate1.UPRN);
            updatedAddressWithoutUPRN2.Uprn.Should().Be(uprnUpdate2.UPRN);
            updatedAddressWithUPRN1.Uprn.Should().Be(address1WithUPRN.Uprn);
            updatedAddressWithUPRN2.Uprn.Should().Be(address2WithUPRN.Uprn);
        }
    }
}
