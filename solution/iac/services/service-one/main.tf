locals {
  service_name = "service-one"
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

module "configuration" {
  source               = "../../modules/service_configuration"
  workload_config      = var.workload_config
  label_name           = "services.service-one"
  app_configs          = local.app_configs
  app_config_overrides = lookup(var.env_service_configs, local.service_name, [])
  vault_secrets        = local.vault_secrets
}

# Enable receiving message from other services in the solution over Azure Service Bus queues. 
module "messaging" {
  source          = "../../modules/service_messaging"
  workload_config = var.workload_config
  service_name    = local.service_name
}

