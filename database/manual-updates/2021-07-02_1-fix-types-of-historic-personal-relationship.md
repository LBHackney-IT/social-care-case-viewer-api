# Fix types of historic personal relationships

## The problem we're trying to solve

In the [manual update to transform the historic personal relationships](2021-06-16_2-transform-historic-personal-relationships.md),
we incorrectly set the `fk_personal_relationship_type_id`.

## Justification for doing a manual update

This is the simplest and quickest way to transform the data, using DMS for this
is overkill.

## The plan

1. Make a backup of the `dbo.sccv_personal_relationship` table
2. Run an update statement to set the `fk_personal_relationship_type_id` to the
   `inverse_type_id` of the current value for a relationship for the person
3. Run an update statement to set the `fk_personal_relationship_type_id` to the
   `inverse_type_id` of the current value for a relationship for the other person
4. Check the frontend correctly displays the relationship
5. Run an update statement to set the `fk_personal_relationship_type_id` to the
   `inverse_type_id` of the current value for all relationships

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-764

## SQL statement(s)

```sql
-- Create back up and replace <yyyy_mm_dd> with current date
CREATE TABLE dbo.sccv_personal_relationship_<yyyy_mm_dd> as table dbo.sccv_personal_relationship;

-- First, test update for one person's relationship
-- Update personal relationship type for one relationship
UPDATE dbo.sccv_personal_relationship pr
SET fk_personal_relationship_type_id = prt.inverse_type_id
FROM dbo.sccv_personal_relationship_type prt
WHERE pr.fk_personal_relationship_type_id = prt.id AND pr.id = <PERSONAL_RELATIONSHIP_ID>;

-- Update personal relationship type for inverse relationship
UPDATE dbo.sccv_personal_relationship pr
SET fk_personal_relationship_type_id = prt.inverse_type_id
FROM dbo.sccv_personal_relationship_type prt
WHERE pr.fk_personal_relationship_type_id = prt.id AND pr.id = <OTHER_PERSONAL_RELATIONSHIP_ID>;

-- Then, update all relationships apart from the ones we've already done
UPDATE dbo.sccv_personal_relationship pr
SET fk_personal_relationship_type_id = prt.inverse_type_id
FROM dbo.sccv_personal_relationship_type prt
WHERE pr.fk_personal_relationship_type_id = prt.id AND NOT (pr.id = <PERSONAL_RELATIONSHIP_ID> OR pr.id = <OTHER_PERSONAL_RELATIONSHIP_ID>);
```

## Useful resources

N/A
