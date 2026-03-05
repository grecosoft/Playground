locals {
  resource_group_name = "${var.workload_name}-${var.environment}-${var.location}"
}


module workload_identity {
  source = "./modules/workload_identity"
  resource_group_name = local.resource_group_name
  location            = var.location
  solution_name       = var.solution_name  
  oidc_issuer_url     = local.kubernetes_cluster.oidc_issuer_url
  namespace           = "solution"
}

module service_one {
  source = "./services/service-one"
  solution_identity_client_id = module.workload_identity.solution_identity_client_id  
  servicebus_namespace_id = local.servicebus_namespace.id
}