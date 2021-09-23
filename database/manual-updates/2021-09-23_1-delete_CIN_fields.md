# Delete statements

## The problem we're trying to solve

Remove CIN fields (not used in new design iteration)

## Justification for doing a manual update

We don't have database migrations set up for the API.

## The plan

1. Run SQL statements to delete the rows in Staging
2. Run SQL statements to delete the rows in Production

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1231

## SQL statement(s)

```sql
delete from dbo.sccv_case_status_field_option where fk_sccv_case_status_field_id in (select id from dbo.sccv_case_status_type where name = 'CIN' );

delete from dbo.sccv_case_status_field where fk_case_status_type_id in (select id from dbo.sccv_case_status_type where name = 'CIN' );
```
