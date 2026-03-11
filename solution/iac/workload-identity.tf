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

# Workload identity role assignments:
resource "azurerm_role_assignment" "key_vault_users" {
  principal_id         = local.solution_identity_principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = local.configuration.key_vault_id
}

resource "azurerm_role_assignment" "app_config_users" {
  principal_id         = local.solution_identity_principal_id
  role_definition_name = "App Configuration Data Reader"
  scope                = local.configuration.app_config_id
}

