# The EntraId group containing the solution developers.
data "azuread_group" "solution_developers" {
  display_name     = var.developer_group_name
  security_enabled = true
}

# Workload identity for the solution, used by the services in the solution
# to access Azure resources securely without needing to manage credentials.
module workload_identity {
  source = "./modules/workload_identity"
  resource_group_name = local.workload_env_name
  location            = var.location
  solution_name       = var.solution_name  
  oidc_issuer_url     = local.kubernetes_cluster.oidc_issuer_url
  namespace           = local.workload_env_name
}

# Configurations for the services implementing the solution.
module service_one {
  source = "./services/service-one"
  workload_config = local.workload_config 
  env_service_configs = var.environment_overrides.service_configs
}

module service_two {
  source = "./services/service-two"
  workload_config = local.workload_config
}