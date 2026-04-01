locals {
  app_configs = [
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
      key   = "DatabasePort2"
      value = 5432
      label = "test-label2"
    }
  ]

  vault_secrets = [
    {
      key    = "secret:value2",
      secret = "some value2"
    }
  ]
}