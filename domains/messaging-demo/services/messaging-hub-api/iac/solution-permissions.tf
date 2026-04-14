# Service identity role assignments:
locals {
  service_principal_id = module.identity.principal_id
}

resource "azurerm_role_assignment" "key_vault_users" {
  principal_id         = local.service_principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = local.solution.configuration.key_vault_id
}

resource "azurerm_role_assignment" "app_config_users" {
  principal_id         = local.service_principal_id
  role_definition_name = "App Configuration Data Reader"
  scope                = local.solution.configuration.app_config_id
}

resource "azurerm_role_assignment" "signalr_server" {
  principal_id         = local.service_principal_id
  role_definition_name = "SignalR App Server"
  scope                = local.solution.signalr.id
}