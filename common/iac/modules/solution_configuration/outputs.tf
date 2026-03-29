output "key_vault_name" {
  value = azurerm_key_vault.key_vault.name
}

output "key_vault_id" {
  value = azurerm_key_vault.key_vault.id
}

output "key_vault_uri" {
  value = azurerm_key_vault.key_vault.vault_uri
}

output "app_config_name" {
    value = azurerm_app_configuration.app_config.name
}

output "app_config_id" {
  value = azurerm_app_configuration.app_config.id
}