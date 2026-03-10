locals {
  app_configs = [
    {
      key   = "DatabaseHost"
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
      key = "secret:value",
      secret = "some value"
    }
  ] 
}

# Enable receiving message from other services in the solution over Azure Service Bus queues. 
module "service_one_messaging" {
  source = "../../modules/service_messaging"
  service_name = "service-one" 
  servicebus_namespace_id = var.servicebus_namespace_id
  solution_identity_principal_id = var.solution_identity_principal_id
  solution_developers_group_id = var.developers_principal_id
}

module "service_one_configuration" {
  source = "../../modules/service_configuration" 
  app_config_id = var.app_config_id
  key_vault_uri = var.key_vault_uri
  key_vault_id = var.key_vault_id
  label_name = "services.service-one" 
  app_configs = local.app_configs
  vault_secrets = local.vault_secrets
}