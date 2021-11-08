# Update start_date and end_date columns data type

## The problem we're trying to solve

Currently the start_date and end_date columns are defined as timestamp wihout time zone. For consistency these should be defined as timestamp like other timestamp columns in the schema.

## Justification for doing a manual update

We don't have automated db migrations

## The plan

1. Run the script

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1451

## SQL statement(s)

```sql
ALTER TABLE IF EXISTS dbo.sccv_person_case_status
	ALTER start_date type timestamp;

ALTER TABLE IF EXISTS dbo.sccv_person_case_status
	ALTER end_date type timestamp;
```