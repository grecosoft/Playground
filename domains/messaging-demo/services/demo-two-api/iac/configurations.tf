locals {
  common_configs = [
     {
      key   = "update_sentinel"
      value = "0"
    },
    {
      key   = "Serilog:MinimumLevel:Default"
      value = var.Serilog_Log_Level
    }
  ]

  vault_secrets = [
    {
      key    = "secret__value2",
      secret = "some value2"
    }
  ]
}