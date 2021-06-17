# Transform historic personal relationships

## The problem we're trying to solve

The historic personal relationships aren't in the structure we want to use and
don't use the same relationship types.

## Justification for doing a manual update

This is the simplest and quickest way to transform the data, using DMS for this
is overkill.

## The plan

1. Create a lookup table to map historic personal relationship types to our new relationships types
2. Run SQL statement to insert into `dbo.sccv_personal_relationship` for historic personal relationships

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-410

## SQL statement(s)

```sql
-- create lookup table
CREATE TABLE IF NOT EXISTS dbo.dm_relationship_historic_types_lookup
(
    historic_type_id int,
    type_id int
);

-- add mappings
insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'greatGrandchild')
from dbo.dm_personal_rel_types
where description similar to 'Great Granddaughter%|Great Grandson%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'greatGrandparent')
from dbo.dm_personal_rel_types
where description similar to 'Great Grandmother%|Great Grandfather%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'grandchild')
from dbo.dm_personal_rel_types
where description similar to 'Granddaughter%|Grandson%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'grandparent')
from dbo.dm_personal_rel_types
where description similar to 'Grandmother%|Grandfather%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'parent')
from dbo.dm_personal_rel_types
where description similar to 'Parent : Child%|Parent : Daughter%|Parent : Son%|Mother : Child%|Mother : Daughter%|Mother : Son%|Father : Child%|Father : Daughter%|Father : Son%|';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'stepParent')
from dbo.dm_personal_rel_types
where description similar to 'Step Father%|Step-parent%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'auntUncle')
from dbo.dm_personal_rel_types
where description similar to 'Aunt%|Uncle%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'child')
from dbo.dm_personal_rel_types
where description similar to 'Son%|Daughter%|Child%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'stepChild')
from dbo.dm_personal_rel_types
where description similar to 'Step-child%|Step Son%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'unbornChild')
from dbo.dm_personal_rel_types
where description similar to 'Unborn Child%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'partner')
from dbo.dm_personal_rel_types
where description similar to 'Partner%|Female Partner%|Civil Partner%|Male Partner%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'exPartner')
from dbo.dm_personal_rel_types
where description similar to 'Ex Partner%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'sibling')
from dbo.dm_personal_rel_types
where description similar to 'Sibling : Brother%|Sibling : Sibling%|Sibling : Sister%|Brother : Brother%|Brother : Sibling%|Brother : Sister%|Sister : Brother%|Sister : Sibling%|Sister : Sister%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'halfSibling')
from dbo.dm_personal_rel_types
where description similar to 'Half-sibling%|Half-sister%|Half-brother%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'stepSibling')
from dbo.dm_personal_rel_types
where description similar to 'Step-sibling%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'unbornSibling')
from dbo.dm_personal_rel_types
where description similar to 'Unborn Sibling%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'spouse')
from dbo.dm_personal_rel_types
where description similar to 'Husband%|Wife%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'cousin')
from dbo.dm_personal_rel_types
where description similar to 'Cousin%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'nieceNephew')
from dbo.dm_personal_rel_types
where description similar to 'Niece%|Nephew%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'fosterCarer')
from dbo.dm_personal_rel_types
where description similar to 'Foster Carer : Foster Child%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'friend')
from dbo.dm_personal_rel_types
where description similar to 'Friend%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'other')
from dbo.dm_personal_rel_types
where description similar to 'Other%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'parentOfUnbornChild')
from dbo.dm_personal_rel_types
where description similar to 'Father : Unborn Child%|Mother : Unborn Child%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'siblingOfUnbornChild')
from dbo.dm_personal_rel_types
where description similar to 'Brother : Unborn Sibling%|Sister : Unborn Sibling%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'fosterCarerSupportCarer')
from dbo.dm_personal_rel_types
where description similar to 'Foster Carer : Support Carer%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'privateFosterCarer')
from dbo.dm_personal_rel_types
where description similar to 'Private Foster Carer%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'privateFosterChild')
from dbo.dm_personal_rel_types
where description similar to 'Private Foster Child%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'fosterChild')
from dbo.dm_personal_rel_types
where description similar to 'Foster Child%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'supportCarerFosterCarer')
from dbo.dm_personal_rel_types
where description similar to 'Support Carer : Foster Carer%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'neighbour')
from dbo.dm_personal_rel_types
where description similar to 'Neighbour%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'inContactWith')
from dbo.dm_personal_rel_types
where description similar to 'In contact with%';

insert into dbo.dm_relationship_historic_types_lookup(historic_type_id, type_id)
select personal_rel_type_id, (select id from dbo.sccv_personal_relationship_type where description = 'acquaintance')
from dbo.dm_personal_rel_types
where description similar to 'Acquaintance%';

-- insert into new personal relationship table
INSERT INTO dbo.sccv_personal_relationship (fk_person_id, fk_other_person_id, fk_personal_relationship_type_id)
SELECT hpr.person_id, hpr.other_person_id, rtl.type_id
FROM dbo.dm_personal_relationships hpr
LEFT JOIN dbo.dm_relationship_historic_types_lookup rtl ON hpr.personal_rel_type_id = rtl.historic_type_id;
```

## Useful resources

N/A
