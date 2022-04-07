# Delete the new ILDS team and rename the Integrated Learning Disabilities one to ILDS

## The problem we're trying to solve

To avoid redundant deallocations and reallocations
## Justification for doing a manual update

No other way of deleting and renaming teams

## The plan

1. Run the SQL scripts
2. Run appropriate queries to check everything went well

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1811

## SQL statement(s)

```sql

delete from dbo.sccv_team where id = 145;

update dbo.sccv_team set name = 'ILDS' where id = 107;

```