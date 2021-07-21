# Add soft delete support

## The problem we're trying to solve

This update is related to duplicate person records removal work.

## Justification for doing a manual update

We don't have automated database migrations yet, so we need to apply these changes manually

## The plan

1. Run the SQL script below

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-396

## SQL statement(s)

This update is run in three steps to ensure backwards compatibility with the changes already made on staging for testing

```sql
--- add new column
ALTER TABLE dbo.dm_persons
  ADD COLUMN marked_for_deletion boolean;  
  
ALTER TABLE dbo.dm_addresses
  ADD COLUMN marked_for_deletion boolean; 
  
ALTER TABLE dbo.dm_telephone_numbers
  ADD COLUMN marked_for_deletion boolean; 
  
ALTER TABLE dbo.sccv_person_other_name
  ADD COLUMN marked_for_deletion boolean; 
  
ALTER TABLE dbo.sccv_allocations_combined
  ADD COLUMN marked_for_deletion boolean; 
  
ALTER TABLE dbo.sccv_warning_note
  ADD COLUMN marked_for_deletion boolean; 
  
ALTER TABLE dbo.sccv_personal_relationship
  ADD COLUMN marked_for_deletion boolean;   
  
-- set default values for new records
ALTER TABLE dbo.dm_persons
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE;  
  
ALTER TABLE dbo.dm_addresses
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE; 
  
ALTER TABLE dbo.dm_telephone_numbers
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE; 
  
ALTER TABLE dbo.sccv_person_other_name
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE; 
  
ALTER TABLE dbo.sccv_allocations_combined
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE; 
  
ALTER TABLE dbo.sccv_warning_note
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE; 
  
ALTER TABLE dbo.sccv_personal_relationship
  ALTER COLUMN marked_for_deletion SET DEFAULT FALSE;   

--update existing records
UPDATE dbo.dm_persons
  SET marked_for_deletion = FALSE;  
  
UPDATE dbo.dm_addresses
  SET marked_for_deletion = FALSE; 
  
UPDATE dbo.dm_telephone_numbers
  SET marked_for_deletion = FALSE; 
  
UPDATE dbo.sccv_person_other_name
  SET marked_for_deletion = FALSE; 
  
UPDATE dbo.sccv_allocations_combined
  SET marked_for_deletion = FALSE; 
  
UPDATE dbo.sccv_warning_note
  SET marked_for_deletion = FALSE; 
  
UPDATE dbo.sccv_personal_relationship
  SET marked_for_deletion = FALSE; 
```
