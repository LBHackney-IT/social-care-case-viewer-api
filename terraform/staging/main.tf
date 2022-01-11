# INSTRUCTIONS:
# 1) ENSURE YOU POPULATE THE LOCALS
# 2) ENSURE YOU REPLACE ALL INPUT PARAMETERS, THAT CURRENTLY STATE 'ENTER VALUE', WITH VALID VALUES
# 3) YOUR CODE WOULD NOT COMPILE IF STEP NUMBER 2 IS NOT PERFORMED!
# 4) ENSURE YOU CREATE A BUCKET FOR YOUR STATE FILE AND YOU ADD THE NAME BELOW - MAINTAINING THE STATE OF THE INFRASTRUCTURE YOU CREATE IS ESSENTIAL - FOR APIS, THE BUCKETS ALREADY EXIST
# 5) THE VALUES OF THE COMMON COMPONENTS THAT YOU WILL NEED ARE PROVIDED IN THE COMMENTS
# 6) IF ADDITIONAL RESOURCES ARE REQUIRED BY YOUR API, ADD THEM TO THIS FILE
# 7) ENSURE THIS FILE IS PLACED WITHIN A 'terraform' FOLDER LOCATED AT THE ROOT PROJECT DIRECTORY

provider "aws" {
  region  = "eu-west-2"
  version = "~> 2.0"
}
data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

terraform {
  backend "s3" {
    bucket  = "terraform-state-staging-apis"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/social-care-case-viewer-api/state"
  }
}

/*    POSTGRES SET UP    */
data "aws_vpc" "staging_vpc" {
  tags = {
    Name = "vpc-staging-apis-staging"
  }
}
data "aws_subnet_ids" "staging" {
  vpc_id = data.aws_vpc.staging_vpc.id
  filter {
    name   = "tag:Type"
    values = ["private"]
  }
}
data "aws_ssm_parameter" "social_care_postgres_db_password" {
  name = "/social-care-case-viewer-api/staging/postgres-password"
}

data "aws_ssm_parameter" "social_care_postgres_username" {
  name = "/social-care-case-viewer-api/staging/postgres-username"
}

module "postgres_db_staging" {
  source               = "git@github.com:LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres"
  environment_name     = "staging"
  vpc_id               = data.aws_vpc.staging_vpc.id
  db_identifier        = "social-care"
  db_name              = "social_care"
  db_port              = 5602
  subnet_ids           = data.aws_subnet_ids.staging.ids
  db_engine            = "postgres"
  db_engine_version    = "11." //DMS does not work well with v12, use 11. to ignore minor version upgrades
  db_instance_class    = "db.t3.medium"
  db_allocated_storage = 20
  maintenance_window   = "sun:10:00-sun:10:30"
  db_username          = data.aws_ssm_parameter.social_care_postgres_username.value
  db_password          = data.aws_ssm_parameter.social_care_postgres_db_password.value
  storage_encrypted    = false
  multi_az             = false //only true if production deployment
  publicly_accessible  = false
  project_name         = "social care viewer"
}

data "aws_ssm_parameter" "social_care_historical_postgres_username" {
  name = "/social-care-case-viewer-api/staging/historical-postgres-username"
}

resource "random_password" "historical_postgres_db_password" {
  length  = 16
  special = false
}

resource "aws_secretsmanager_secret" "historical_postgres_db_password" {
  name = "social_care_case_viewer_api_staging_historical_postgres_db_password"
}

resource "aws_secretsmanager_secret_version" "historical_postgres_db_password" {
  secret_id     = aws_secretsmanager_secret.historical_postgres_db_password.id
  secret_string = random_password.historical_postgres_db_password.result
}

module "historical_postgres_db_staging" {
  source               = "git@github.com:LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres"
  environment_name     = "staging"
  vpc_id               = data.aws_vpc.staging_vpc.id
  db_identifier        = "historical-social-care"
  db_name              = "historical_social_care"
  db_port              = 5432
  subnet_ids           = data.aws_subnet_ids.staging.ids
  db_engine            = "postgres"
  db_engine_version    = "11." //use 11. to ignore minor version upgrades
  db_instance_class    = "db.t2.micro"
  db_allocated_storage = 20
  maintenance_window   = "sun:10:00-sun:10:30"
  db_username          = data.aws_ssm_parameter.social_care_historical_postgres_username.value
  db_password          = aws_secretsmanager_secret_version.historical_postgres_db_password.secret_string
  storage_encrypted    = false
  multi_az             = false
  publicly_accessible  = false
  project_name         = "social care viewer"
}
