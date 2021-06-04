# Updating the database schema

We currently manage database changes manually as we don't have database migrations set up. Below are the steps to take for when you need to make changes to the PostgreSQL database schema.

- In the database repository update the [schema.sql](https://github.com/LBHackney-IT/social-care-case-viewer-api/blob/master/database/schema.sql) file
- In SocialCareCaseViewerAPI/Infrastructure either update an existing class or create a new class for the schema changes
- To test locally run `make restart-db`
- To deploy changes to AWS:
    - Go to AWS account (staging or prod)
    - Go to Systems Manager
    - Go to Session Manager
    - Choose `RDS jump box-Platform APIs (new)` and click `Start Session`, this allows us to have a CLI into the instance hosting our database
    - Connect to PostgreSQL `psql --host=<hostname> --port=5600 --username=<username> --password=<password> --dbname=social_care`
    - Backup the table you are going to apply changes to `create table_backup as table_copied`, it can be useful use the same table name for the backup but to append the date to the table name
    - If we later make breaking changes to the table in use we rename the backup table to make it our `active` version of the table
    - Manually apply schema changes to the table we are interested in
