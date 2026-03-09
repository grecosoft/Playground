# Azure Key Vault Roles:
resource "azurerm_role_assignment" "key_vault_users" {
  principal_id         = local.solution_identity_principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = local.configuration.key_vault_id
}

# Azure App Configuration Roles:
resource "azurerm_role_assignment" "app_config_users" {
  principal_id         = local.solution_identity_principal_id
  role_definition_name = "App Configuration Data Reader"
  scope                = local.configuration.app_config_id
}

