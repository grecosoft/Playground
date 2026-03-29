locals {
  default_redirect_uris = ["https://login.microsoftonline.com/common/oauth2/nativeclient"]
}

# The EntraId group containing the solution developers.
data "azuread_group" "solution_developers" {
  display_name     = var.developer_group_name
  security_enabled = true
}

# Defines an App Registration for the solution's api with its corresponding roles
# and groups.  Also defines a Client App Registration used to obtain JTW tokens.
module "solution_auth" {
  source          = "./modules/solution_auth"

  workload_config = local.workload_config
  solution_name   = var.solution_name

  redirect_uris = distinct(concat(
    local.default_redirect_uris,
    var.environment_overrides.solution_auth.redirect_uris))

  solution_roles = {
    "apiDataReader" = {
      display_name         = "Solution Data Reader"
      description          = "Allowed to read any data for the solution service's."
      allowed_member_types = ["Application", "User"]
    }
  }

  role_user_assignments = {
    "apiDataReader" = ["greco.brian_hotmail.com#EXT#@grecobrianhotmail.onmicrosoft.com"]
  }
}

# Services implementing the solution.
module "service_one" {
  source              = "./services/service-one"
  
  workload_config     = local.workload_config
  env_service_configs = var.environment_overrides.service_configs
}

module "service_two" {
  source          = "./services/service-two"

  workload_config = local.workload_config
}

# Defines the resources allowing the solution's services to 
# communicate by sending commands and domain-events.
module "solution_messaging" {
  source = "./modules/solution_messaging"

  workload_config = local.workload_config
  solution_name = var.solution_name
  service_names = [
    module.service_one.service_name,
    module.service_two.service_name
  ]
}