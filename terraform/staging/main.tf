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
locals {
   parameter_store = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
}

data "aws_iam_role" "ec2_container_service_role" {
  name = "ecsServiceRole"
}

data "aws_iam_role" "ecs_task_execution_role" {
  name = "ecsTaskExecutionRole"
}

terraform {
  backend "s3" {
    bucket  = "terraform-state-staging-apis" 
    encrypt = true
    region  = "eu-west-2"
    key     = "services/mosaic-resident-information-api/state" 
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
 data "aws_ssm_parameter" "mosaic_postgres_db_password" {
   name = "/mosaic-api/staging/postgres-password"
 }

 data "aws_ssm_parameter" "mosaic_postgres_username" {
   name = "/mosaic-api/staging/postgres-username"
 }
 
module "postgres_db_staging" {
  source = "github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres"
  environment_name = "staging"
  vpc_id = data.aws_vpc.staging_vpc.id
  db_identifier = "mosaic-mirror"
  db_name = "mosaic_mirror"
  db_port  = 5002
  subnet_ids = data.aws_subnet_ids.staging.ids
  db_engine = "postgres"
  db_engine_version = "11.1" //DMS does not work well with v12
  db_instance_class = "db.t2.micro"
  db_allocated_storage = 20
  maintenance_window ="sun:10:00-sun:10:30"
  db_username = data.aws_ssm_parameter.mosaic_postgres_username.value
  db_password = data.aws_ssm_parameter.mosaic_postgres_db_password.value
  storage_encrypted = false
  multi_az = false //only true if production deployment
  publicly_accessible = false
  project_name = "platform apis"
}

/*    DMS SET UP    */

data "aws_ssm_parameter" "mosaic_test_username" {
   name = "/mosaic/test-server/username"
}
data "aws_ssm_parameter" "mosaic_test_password" {
   name = "/mosaic/test-server/password"
}
data "aws_ssm_parameter" "mosaic_test_hostname" {
   name = "/mosaic/test-server/hostname"
}
 data "aws_ssm_parameter" "mosaic_postgres_hostname" {
   name = "/mosaic-api/staging/postgres-hostname"
}

/* ONE OFF ACCOUNT SET UP FOR DMS REQUIRED ROLES */

 data "aws_iam_policy_document" "dms_assume_role" {
   statement {
     actions = ["sts:AssumeRole"]

     principals {
       identifiers = ["dms.amazonaws.com"]
       type        = "Service"
     }
   }
 }

 resource "aws_iam_role" "dms-access-for-endpoint" {
   assume_role_policy = "${data.aws_iam_policy_document.dms_assume_role.json}"
   name               = "dms-access-for-endpoint"
 }
 resource "aws_iam_role_policy_attachment" "dms-access-for-endpoint-AmazonDMSRedshiftS3Role" {
   policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonDMSRedshiftS3Role"
   role       = "${aws_iam_role.dms-access-for-endpoint.name}"
 }
 resource "aws_iam_role" "dms-vpc-role" {
   assume_role_policy = "${data.aws_iam_policy_document.dms_assume_role.json}"
   name               = "dms-vpc-role"
 }
 resource "aws_iam_role_policy_attachment" "dms-vpc-role-AmazonDMSVPCManagementRole" {
   policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonDMSVPCManagementRole"
   role       = "${aws_iam_role.dms-vpc-role.name}"
 }

/* DMS SET UP INCLUDING DMS INSTANCE AS NONE EXISTS */

module "dms_setup_staging" {
  source = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_full_setup" 
  environment_name = "staging" //used for resource tags
  project_name = "resident-information-apis" //used for resource tags
  //target db for dms endpoint
  target_db_name = "mosaic_mirror" 
  target_endpoint_identifier = "target-mosaic-test-endpoint" 
  target_db_engine_name = "postgres"
  target_db_port = 5002
  target_db_username = data.aws_ssm_parameter.mosaic_postgres_username.value
  target_db_password = data.aws_ssm_parameter.mosaic_postgres_db_password.value
  target_db_server = data.aws_ssm_parameter.mosaic_postgres_hostname.value
  target_endpoint_ssl_mode = "none"
  //source db for dms endpoint
  source_db_name = "mostest" 
  source_endpoint_identifier = "source-mosaic-test-endpoint" 
  source_db_engine_name = "sqlserver"
  source_db_port = 1433
  source_db_username = data.aws_ssm_parameter.mosaic_test_username.value //ensure you save your on-prem credentials to the Parameter store and reference it here
  source_db_password = data.aws_ssm_parameter.mosaic_test_password.value //ensure you save your on-prem credentials to the Parameter store and reference it here
  source_db_server = data.aws_ssm_parameter.mosaic_test_hostname.value
  source_endpoint_ssl_mode = "none"
  //replication instance set up -> IF SOURCE IS 'dms_full_setup'
  allocated_storage = 20 //in GB
  maintenance_window = "sun:07:00-sun:07:30" 
  replication_instance_class = "dms.t2.small" 
  replication_instance_identifier = "staging-dms-instance" 
  vpc_name = "vpc-staging-apis" 
  dms_instance_publicly_accessible = false
  //dms task set up
  migration_type = "full-load-and-cdc" 
  replication_task_indentifier = "mosaic-api-dms-task" 
  task_settings = file("${path.module}/task_settings.json") 
  task_table_mappings = file("${path.module}/selection_rules.json")
}

# /*   ADD SECURITY RULE TO POSTGRES DB TO ALLOW DMS TRAFFIC   */
data "aws_security_group" "mosaic_postgres_sg" {
   filter {
    name   = "tag:Name"
    values = ["mosaic_mirror-staging"] 
  }
}

resource "aws_security_group_rule" "allow_dms_traffic" {
  type              = "ingress"
  from_port         = 0
  to_port           = 0
  protocol          = "all"
  source_security_group_id = module.dms_setup_staging.dms_instance_sg
  security_group_id = data.aws_security_group.mosaic_postgres_sg.id
}
