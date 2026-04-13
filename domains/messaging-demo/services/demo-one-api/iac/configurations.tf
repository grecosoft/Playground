locals {
  common_configs = [
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
    },
    {
      key   = "DatabasePort"
      value = 5432
      label = "test-label"
    }
  ]

  vault_secrets = [
    {
      key    = "secret:value",
      secret = "some value"
    }
  ]
}