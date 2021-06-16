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
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'greatGrandchild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'greatGrandparent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'greatGrandparent') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'greatGrandchild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'grandchild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'grandparent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'grandparent') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'grandchild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'parent') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'child');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'stepParent') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'stepChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'auntUncle') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'nieceNephew');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'child') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'parent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'stepChild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'stepParent');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'unbornChild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'parentOfUnbornChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'partner') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'partner');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'exPartner') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'exPartner');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'sibling') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'sibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'halfSibling') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'halfSibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'stepSibling') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'stepSibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'unbornSibling') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'siblingOfUnbornChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'spouse') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'spouse');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'cousin') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'cousin');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'nieceNephew') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'auntUncle');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'fosterCarer') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'fosterChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'friend') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'friend');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'other') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'other');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'exSpouse') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'exSpouse');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'parentOfUnbornChild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'unbornChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'siblingOfUnbornChild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'unbornSibling');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'fosterCarerSupportCarer') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'supportCarerFosterCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'privateFosterCarer') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'privateFosterChild');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'privateFosterChild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'privateFosterCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'fosterChild') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'fosterCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'supportCarerFosterCarer') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'fosterCarerSupportCarer');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'neighbour') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'neighbour');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'inContactWith') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'inContactWith');
update dbo.sccv_personal_relationship_type set inverse_type_id = (select id from dbo.sccv_personal_relationship_type where description ilike 'acquaintance') where id = (select id from dbo.sccv_personal_relationship_type where description ilike 'acquaintance');

-- insert into new personal relationship table
INSERT INTO dbo.sccv_personal_relationship (fk_person_id, fk_other_person_id, fk_personal_relationship_type_id)
SELECT hpr.person_id, hpr.other_person_id, rtl.type_id
FROM dbo.dm_personal_relationships hpr
LEFT JOIN dbo.dm_relationship_historic_types_lookup rtl ON hpr.personal_rel_type_id = rtl.historic_type_id;
```

## Useful resources

N/A
