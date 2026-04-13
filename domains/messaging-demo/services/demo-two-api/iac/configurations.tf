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
    }
  ]

  vault_secrets = [
    {
      key    = "secret__value",
      secret = "some value2"
    }
  ]
}