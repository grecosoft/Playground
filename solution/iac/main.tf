data "azuread_group" "solution_developers" {
  display_name     = var.developer_group_name
  security_enabled = true
}

# Workload identity for the solution, used by the services in the solution to access 
# Azure resources securely without needing to manage credentials.
module workload_identity {
  source = "./modules/workload_identity"
  resource_group_name = local.workload_env_name
  location            = var.location
  solution_name       = var.solution_name  
  oidc_issuer_url     = local.kubernetes_cluster.oidc_issuer_url
  namespace           = local.workload_env_name
}

# Configurations for the services in the solution.
module service_one {
  source = "./services/service-one"
  solution_identity_principal_id = module.workload_identity.principal_id  
  servicebus_namespace_id = local.servicebus_namespace.id 
  developers_principal_id = local.developers_principal_id
}

module service_two {
  source = "./services/service-two"
  solution_identity_principal_id = module.workload_identity.principal_id  
  servicebus_namespace_id = local.servicebus_namespace.id 
  developers_principal_id = local.developers_principal_id
}