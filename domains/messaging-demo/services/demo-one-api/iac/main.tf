locals {
  solution_name = "playground"
  service_name = "demo-one-api"

  # Service configurations consisting of common and module level settings:
  service_configs = concat(
    local.common_configs, 
    module.messaging.service_config,
    module.deploy.deploy_variables
  )
}

# The identity used by the deployed service used to access resources.
module "identity" {
  source = "../../../../../common/iac/modules/service_identity"

  resource_group_name = local.solution_env_name
  service_name = local.service_name
  location = local.solution_location
  namespace = local.solution_env_name
  oidc_issuer_url = local.solution.kubernetes.oidc_issuer_url
}

# Allows the service to send messages to over solutions services over Service Bus.
module "messaging" {  
  source = "../../../../../common/iac/modules/service_messaging"

  solution_name = local.solution_name
  service_name = local.service_name
  service_id = module.identity.service_id
  service_principal_id = module.identity.principal_id
  developers_group_id = local.solution_developers_group_id 
  solution_messaging = local.solution.messaging
  solution_servicebus = local.solution.servicebus 
  rpc_reply_timeout_seconds = 50
  dependent_services = {
    "demo-two-api": "bae5fa24-b552-a42c-0e16-1d94b2c62f2d"
    "connector-hub-api": "92475bce-091c-33da-6e0a-f104fc39e64b"
  }
}

module "deploy" {
  source = "../../../../../common/iac/modules/service_deploy"

  service_name = local.service_name
  workload_client_id = module.identity.client_id
  workload_tenant_id = module.identity.tenant_id
}

# The configuration for the service.
module "configuration" {
  source               = "../../../../../common/iac/modules/service_configuration"
  
  solution_configuration = local.solution.configuration
  label_name           = local.service_name
  app_configs          = local.service_configs
  app_config_overrides = var.env_app_configs
  vault_secrets        = local.vault_secrets 
}

