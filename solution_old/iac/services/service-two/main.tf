locals {
  service_name  = "service-two"
  app_configs   = []
  vault_secrets = []
}

module "configuration" {
  source          = "../../modules/service_configuration"
  workload_config = var.workload_config
  label_name      = local.service_name
  app_configs     = local.app_configs
  vault_secrets   = local.vault_secrets
}

output "service_name" {
  description = "The name used to identify the service within the solution."
  value = local.service_name
}
