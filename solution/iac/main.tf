locals {
  resource_group_name = "${var.workload_name}-${var.environment}-${var.location}"
}

# Workload identity for the solution, used by the services in the solution to access 
# Azure resources securely without needing to manage credentials.
module workload_identity {
  source = "./modules/workload_identity"
  resource_group_name = local.resource_group_name
  location            = var.location
  solution_name       = var.solution_name  
  oidc_issuer_url     = local.kubernetes_cluster.oidc_issuer_url
  namespace           = "solution"
}

# Configurations for the services in the solution.
module service_one {
  source = "./services/service-one"
  solution_identity_client_id = module.workload_identity.solution_identity_client_id  
  servicebus_namespace_id = local.servicebus_namespace.id 
  solution_developers_group_id = data.azuread_group.solution_developers.object_id
}

module service_two {
  source = "./services/service-two"
  solution_identity_client_id = module.workload_identity.solution_identity_client_id  
  servicebus_namespace_id = local.servicebus_namespace.id 
  solution_developers_group_id = data.azuread_group.solution_developers.object_id
}