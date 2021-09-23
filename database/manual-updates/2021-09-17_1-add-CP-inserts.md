# Adding data for Child Protection

## The problem we're trying to solve

Populating the various Case Status tables for Child Protection

## Justification for doing a manual update

We don't have database migrations set up for the API.

## The plan

1. Run SQL statements to create the tables in Staging
2. Run SQL statements to create the tables in Production

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-1232

## SQL statement(s)

```sql

INSERT INTO DBO.SCCV_CASE_STATUS_TYPE(NAME, DESCRIPTION) VALUES ('CP','Child protection');

INSERT INTO DBO.SCCV_CASE_STATUS_FIELD(FK_CASE_STATUS_TYPE_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_TYPE WHERE NAME ILIKE 'CP'), 'category', 'Category of Child Protection Plan');

INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'category'), 'C1','Neglect' );
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'category'), 'C2','Physical Abuse' );
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'category'), 'C3','Emotional Abuse' );
INSERT INTO DBO.SCCV_CASE_STATUS_FIELD_OPTION(FK_SCCV_CASE_STATUS_FIELD_ID, NAME, DESCRIPTION) VALUES ((SELECT ID FROM DBO.SCCV_CASE_STATUS_FIELD WHERE NAME ILIKE 'category'), 'C4','Sexual Abuse' );
