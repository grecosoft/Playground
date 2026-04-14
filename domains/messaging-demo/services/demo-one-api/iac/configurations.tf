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