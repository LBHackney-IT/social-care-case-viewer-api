resource "aws_secretsmanager_secret" "fix_historic_case_notes_response_feature_flag" {
  name = "social_care_case_viewer_api_fix_historic_case_notes_response"
}

resource "aws_secretsmanager_secret_version" "fix_historic_case_notes_response_feature_flag" {
  secret_id     = aws_secretsmanager_secret.fix_historic_case_notes_response_feature_flag.id
  secret_string = "false"
}
