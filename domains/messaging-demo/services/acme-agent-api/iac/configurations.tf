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
    },
    {
      key = "CustomerId"
      value = "E6B2BFA1-851B-4DCC-B4A3-CBCAF8FFE138"
    },
    {
      key = "AgentIdentity"
      value = "agent1"
    }
  ]

  vault_secrets = []
}