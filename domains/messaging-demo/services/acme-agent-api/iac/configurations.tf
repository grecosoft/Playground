locals {
  common_configs = [
    {
      key   = "update_sentinel"
      value = "0"
    },
    {
      key   = "Serilog:MinimumLevel:Default"
      value = var.Serilog_Log_Level
    },
    {
      key = "MessagingHubApi"
      value = "http://messaging-hub-api-service"
    }
  ]

  vault_secrets = []
}