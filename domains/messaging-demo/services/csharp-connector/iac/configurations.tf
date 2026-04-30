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
      key = "ConnectorHubApi"
      value = "http://connector-hub-api-service"
    },
    {
      key = "CustomerId"
      value = "E6B2BFA1-851B-4DCC-B4A3-CBCAF8FFE138"
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
      key = "ConnectorId"
      value = "csharp-connector"
    }
  ]

  vault_secrets = []
}