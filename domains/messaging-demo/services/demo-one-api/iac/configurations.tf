locals {
  common_configs = [
    {
      key   = "update_sentinel"
      value = "0"
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
    },
    {
      key   = "DatabasePort"
      value = 5432
      label = "test-label"
    }
  ]

  vault_secrets = [
    {
      key    = "secret__value",
      secret = "some value"
    }
  ]
}