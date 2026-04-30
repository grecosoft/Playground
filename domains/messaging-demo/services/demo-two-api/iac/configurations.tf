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
      key   = "ServiceName"
      value = local.service_name
    },
    {
      key   = "SolutionEnvironment"
      value = local.solution_env_name
    }
  ]

  vault_secrets = [
    {
      key    = "secret__value2",
      secret = "some value2"
    }
  ]
}