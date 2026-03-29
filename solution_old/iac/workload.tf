# Reference to state of the workload environment to which the
# solution will be deployed:
data "terraform_remote_state" "workload" {
  backend = "azurerm"

  config = {
    resource_group_name  = var.storage_resource_group_name
    storage_account_name = var.storage_account_name
    container_name       = var.workload_container_name
    key                  = "terraform.tfstate"
  }
}

locals {
  solution_identity_principal_id = module.workload_identity.principal_id
  developers_principal_id        = data.azuread_group.solution_developers.object_id
  workload_env_name              = data.terraform_remote_state.workload.outputs.workload_env_name

  # Reference to the workload output:
  servicebus_namespace = data.terraform_remote_state.workload.outputs.servicebus_namespace
  kubernetes_cluster   = data.terraform_remote_state.workload.outputs.kubernetes_cluster
  configuration        = data.terraform_remote_state.workload.outputs.configuration

  # Workload resource values on which the solution is dependent and can
  # be passed into modules.
  workload_config = {
    workload_env_name              = data.terraform_remote_state.workload.outputs.workload_env_name
    solution_identity_principal_id = module.workload_identity.principal_id
    developers_group_principal_id  = data.azuread_group.solution_developers.object_id

    app_config_id = local.configuration.app_config_id
    key_vault_uri = local.configuration.key_vault_uri
    key_vault_id  = local.configuration.key_vault_id

    servicebus_namespace_id = local.servicebus_namespace.id

  }
}



