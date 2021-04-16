resource "aws_secretsmanager_secret" "show_historic_data_feature_flag" {
  name = "social_care_case_viewer_api_show_historic_data"
}

resource "aws_secretsmanager_secret_version" "show_historic_data_feature_flag" {
  secret_id     = aws_secretsmanager_secret.show_historic_data_feature_flag.id
  secret_string = "true"
}

resource "aws_secretsmanager_secret" "deallocation_date_feature_flag" {
    name = "social_care_case_viewer_api_deallocation_date"
}

resource "aws_secretsmanager_secret_version" "social_care_case_viewer_api_deallocation_date" {
    secret_id     = aws_secretsmanager_secret.deallocation_date_feature_flag.id
    secret_string = "true"
}
