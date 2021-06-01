# Add missing gender data

## The problem we're trying to solve

Most of our person records that we've imported into the database are missing
gender values.

## Justification for doing a manual update

As there are approximately 200,000 records are missing a value, it's not
feasible to update them through the frontend.

## The plan

1. Install the AWS S3 PostgreSQL extension
2. Upload a CSV file containing `person_id` and `gender` to an S3 bucket
3. Create a temporary table for the gender data
4. Import the gender data into the temporary table
5. Make a backup of the `dm_persons` table
6. Update `dm_persons` using the temporary table

## Link to Jira ticket

https://hackney.atlassian.net/browse/SCT-12

## SQL statement(s)

```sql
CREATE TABLE dbo.sccv_gender_values_import (person_id bigint,gender varchar(1));

SELECT aws_commons.create_s3_uri('<S3_BUCKET_NAME>','dm_persons_gender.csv','eu-west-2') AS s3_uri_dm_persons_gender \gset;

SELECT aws_s3.table_import_from_s3('dbo.sccv_gender_values_import','','(format csv, HEADER)',:'s3_uri_dm_persons_gender');

-- Replace <yyyy_mm_dd> with current date
CREATE TABLE dbo.dm_persons_<yyyy_mm_dd> as table dbo.dm_persons;
```
### Updating person records

**Timestamp:** <yyyy_mm_dd_hh_mm>

```sql
UPDATE dbo.dm_persons p
set gender = gi.gender
from dbo.sccv_gender_values_import gi
where p.person_id = gi.person_id and p.gender is null;
```

## Useful resources

N/A
