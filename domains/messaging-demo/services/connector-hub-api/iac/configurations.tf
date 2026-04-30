locals {
  common_configs = [
     {
      key   = "update_sentinel"
      value = "0"
    },
    {
      key   = "Serilog:MinimumLevel:Default"
      value = var.serilog_log_level
    },
    {
      key    = "SignalREndpoint"
      value = local.solution.signalr.endpoint
    },
    {
      key   = "ServiceName"
      value = local.service_name
    },
    {
      key   = "SolutionEnvironment"
      value = local.solution_env_name
    },
    {
      key   = "ConnectorReplyTimeoutSeconds"
      value = var.connector_reply_timeout_seconds
    }
  ]

  vault_secrets = []
}