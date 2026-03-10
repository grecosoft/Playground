resource "azurerm_app_configuration_key" "service_config" {
  for_each = { for item in var.app_configs : item.key => item }

  configuration_store_id = var.app_config_id
  key                    = each.value.key
  value                  = each.value.value
  label                  = each.value.label == null ? var.label_name : each.value.label
  content_type = each.value.isJson ? "application/json" : ""
}

resource "azurerm_key_vault_secret" "service_secrets" {
  for_each = { for item in var.vault_secrets : item.key => item }
  name = replace(each.key, "/[^a-zA-Z0-9]/", "")
  value        = each.value.secret
  key_vault_id = var.key_vault_id
}

resource "azurerm_app_configuration_key" "service_secrets" {
  for_each = { for item in var.vault_secrets : item.key => item }

  configuration_store_id = var.app_config_id
  key                    = each.value.key
  type                   = "vault"
  label                  = each.value.label == null ? var.label_name : each.value.label
  vault_key_reference    = "${var.key_vault_uri}${azurerm_key_vault_secret.service_secrets[each.key].resource_versionless_id}"  
}  

