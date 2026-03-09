output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "key_vault_id" {
  value = azurerm_key_vault.kv.id
}

output "app_config_name" {
    value = azurerm_app_configuration.app_configuration.name
}

output "app_config_id" {
  value = azurerm_app_configuration.app_configuration.id
}