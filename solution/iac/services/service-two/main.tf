locals {
  service_name = "service-two"
  app_configs = []
  vault_secrets = []
}

module "configuration" {
  source = "../../modules/service_configuration" 
  workload_config = var.workload_config
  label_name =  local.service_name 
  app_configs = local.app_configs
  vault_secrets = local.vault_secrets 
}

# Enable receiving message from other services in the solution over Azure Service Bus queues. 
module "messaging" {
  source = "../../modules/service_messaging"
  workload_config = var.workload_config
  service_name = local.service_name
}