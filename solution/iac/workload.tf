# Defines common contextual variables and reference to workload common state for a given environment. 
locals {
  solution_identity_principal_id = module.workload_identity.principal_id 
  developers_principal_id = data.azuread_group.solution_developers.object_id

  workload_env_name = data.terraform_remote_state.workload.outputs.workload_env_name
  servicebus_namespace = data.terraform_remote_state.workload.outputs.servicebus_namespace
  kubernetes_cluster   = data.terraform_remote_state.workload.outputs.kubernetes_cluster
  configuration = data.terraform_remote_state.workload.outputs.configuration
}