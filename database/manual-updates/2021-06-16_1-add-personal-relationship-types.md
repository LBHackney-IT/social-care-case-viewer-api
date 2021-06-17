# Add personal relationship types

## The problem we're trying to solve

We need to store the personal relationships types we want to use.

## Justification for doing a manual update

We don't have a way to seed data at the moment.

## The plan

1. Run SQL statements to add personal relationship types in Staging

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-410

## SQL statement(s)

```sql
-- add personal relationship types
insert into dbo.sccv_personal_relationship_type (description) VALUES ('greatGrandchild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('greatGrandparent');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('grandchild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('grandparent');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('parent');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('stepParent');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('auntUncle');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('child');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('stepChild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('unbornChild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('partner');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('exPartner');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('sibling');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('halfSibling');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('stepSibling');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('unbornSibling');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('spouse');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('cousin');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('nieceNephew');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('fosterCarer');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('friend');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('other');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('exSpouse');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('parentOfUnbornChild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('siblingOfUnbornChild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('fosterCarerSupportCarer');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('privateFosterCarer');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('privateFosterChild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('fosterChild');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('supportCarerFosterCarer');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('neighbour');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('inContactWith');
insert into dbo.sccv_personal_relationship_type (description) VALUES ('acquaintance');

-- add inverse personal relationship types
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'greatGrandchild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'greatGrandparent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'greatGrandparent') where id = (select id from dbo.sccv_personal_relationship_type where description = 'greatGrandchild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'grandchild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'grandparent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'grandparent') where id = (select id from dbo.sccv_personal_relationship_type where description = 'grandchild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'parent') where id = (select id from dbo.sccv_personal_relationship_type where description = 'child');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'stepParent') where id = (select id from dbo.sccv_personal_relationship_type where description = 'stepChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'auntUncle') where id = (select id from dbo.sccv_personal_relationship_type where description = 'nieceNephew');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'child') where id = (select id from dbo.sccv_personal_relationship_type where description = 'parent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'stepChild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'stepParent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'unbornChild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'parentOfUnbornChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'partner') where id = (select id from dbo.sccv_personal_relationship_type where description = 'partner');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'exPartner') where id = (select id from dbo.sccv_personal_relationship_type where description = 'exPartner');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'sibling') where id = (select id from dbo.sccv_personal_relationship_type where description = 'sibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'halfSibling') where id = (select id from dbo.sccv_personal_relationship_type where description = 'halfSibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'stepSibling') where id = (select id from dbo.sccv_personal_relationship_type where description = 'stepSibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'unbornSibling') where id = (select id from dbo.sccv_personal_relationship_type where description = 'siblingOfUnbornChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'spouse') where id = (select id from dbo.sccv_personal_relationship_type where description = 'spouse');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'cousin') where id = (select id from dbo.sccv_personal_relationship_type where description = 'cousin');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'nieceNephew') where id = (select id from dbo.sccv_personal_relationship_type where description = 'auntUncle');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'fosterCarer') where id = (select id from dbo.sccv_personal_relationship_type where description = 'fosterChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'friend') where id = (select id from dbo.sccv_personal_relationship_type where description = 'friend');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'other') where id = (select id from dbo.sccv_personal_relationship_type where description = 'other');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'exSpouse') where id = (select id from dbo.sccv_personal_relationship_type where description = 'exSpouse');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'parentOfUnbornChild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'unbornChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'siblingOfUnbornChild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'unbornSibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'fosterCarerSupportCarer') where id = (select id from dbo.sccv_personal_relationship_type where description = 'supportCarerFosterCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'privateFosterCarer') where id = (select id from dbo.sccv_personal_relationship_type where description = 'privateFosterChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'privateFosterChild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'privateFosterCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'fosterChild') where id = (select id from dbo.sccv_personal_relationship_type where description = 'fosterCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'supportCarerFosterCarer') where id = (select id from dbo.sccv_personal_relationship_type where description = 'fosterCarerSupportCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'neighbour') where id = (select id from dbo.sccv_personal_relationship_type where description = 'neighbour');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'inContactWith') where id = (select id from dbo.sccv_personal_relationship_type where description = 'inContactWith');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description = 'acquaintance') where id = (select id from dbo.sccv_personal_relationship_type where description = 'acquaintance');

-- insert into new personal relationship table
INSERT INTO dbo.sccv_personal_relationship (fk_person_id, fk_other_person_id, fk_personal_relationship_type_id)
SELECT hpr.person_id, hpr.other_person_id, rtl.type_id
FROM dbo.dm_personal_relationships hpr
LEFT JOIN dbo.dm_relationship_historic_types_lookup rtl ON hpr.personal_rel_type_id = rtl.historic_type_id;
```

## Useful resources

N/A
