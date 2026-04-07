locals {
  common_configs = [
    {
      key    = "DatabaseHost22"
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
      key   = "DatabasePort22"
      value = 5432
      label = "test-label"
    }
  ]

  vault_secrets = [

  ]
}