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
    },
    {
      key    = "DatabaseHost"
      isJson = true
      value = jsonencode({
        value1 = 1000
        value2 = 2000
        settings = [
          { v1 = 12 },
          { v2 = 44 }
        ]
      })
    }
  ]

  vault_secrets = [
    {
      key    = "secret__value",
      secret = "some value"
    }
  ]
}