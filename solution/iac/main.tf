# The EntraId group containing the solution developers.
data "azuread_group" "solution_developers" {
  display_name     = var.developer_group_name
  security_enabled = true
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