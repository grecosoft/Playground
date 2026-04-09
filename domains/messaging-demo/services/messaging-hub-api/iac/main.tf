locals {
  solution_name = "playground"
  service_name = "messaging-hub-api"

  # Service configurations consisting of common and module level settings:
  service_configs = concat(
    local.common_configs,
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