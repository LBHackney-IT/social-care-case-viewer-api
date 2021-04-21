resource "aws_secretsmanager_secret" "show_historic_data_feature_flag" {
  name = "social_care_case_viewer_api_show_historic_data"
}

resource "aws_secretsmanager_secret_version" "show_historic_data_feature_flag" {
  secret_id     = aws_secretsmanager_secret.show_historic_data_feature_flag.id
  secret_string = "true"
}

resource "aws_secretsmanager_secret" "fix_historic_case_note_response_feature_flag" {
  name = "social_care_case_viewer_api_fix_historic_case_note_response"
}

resource "aws_secretsmanager_secret_version" "fix_historic_case_note_response_feature_flag" {
  secret_id     = aws_secretsmanager_secret.fix_historic_case_note_response_feature_flag.id
  secret_string = "false"
}
