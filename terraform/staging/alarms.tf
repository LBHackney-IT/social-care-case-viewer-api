resource "aws_sns_topic" "social_care_alerts" {
  name = "social-care-application-alerts"
}

data "aws_cloudwatch_log_group" "social_care_frontend" {
  name = "/aws/lambda/lbh-social-care-staging"
}

resource "aws_cloudwatch_log_metric_filter" "social_care_frontend_critical_errors" {
  name           = "social-care-frontend-critical-errors"
  pattern        = "ERROR -400 -422 -\"status: 404\""
  log_group_name = data.aws_cloudwatch_log_group.social_care_frontend.name

  metric_transformation {
    name          = "SocialCareFrontendCriticalErrors"
    namespace     = "SocialCareFrontend"
    value         = "1"
    default_value = "0"
  }
}

resource "aws_cloudwatch_metric_alarm" "social_care_frontend_critical_errors" {

  alarm_name          = "social-care-frontend-critical-errors"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "1"
  metric_name         = "SocialCareFrontendCriticalErrors"
  namespace           = "SocialCareFrontend"
  period              = "60"
  statistic           = "Average"
  threshold           = "2"
  alarm_description   = "Triggers an alarm every time there are 2 critical errors in a minute for the Social Care Frontend."
  alarm_actions       = [aws_sns_topic.social_care_alerts.arn]
  datapoints_to_alarm = "1"
}

resource "aws_cloudwatch_dashboard" "social_care" {
  dashboard_name = "social-care-system"

  dashboard_body = <<EOF
{
    "widgets": [
        {
            "height": 6,
            "width": 6,
            "y": 0,
            "x": 0,
            "type": "metric",
            "properties": {
                "metrics": [
                    [ "SocialCareFrontend", "SocialCareFrontendCriticalErrors" ]
                ],
                "region": "eu-west-2",
                "view": "timeSeries",
                "stacked": false,
                "period": 60,
                "annotations": {
                    "horizontal": [
                        {
                            "label": "SocialCareFrontendCriticalErrors > 0 for 1 datapoints within 1 minute",
                            "value": 0
                        }
                    ]
                },
                "title": "Social Care Frontend Critical Errors",
                "stat": "Sum"
            }
        }
    ]
}
EOF
}
