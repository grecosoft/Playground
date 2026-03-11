# This module implements configurations/secrets that are specific
# to a given service.  

locals {
  unique_keys = distinct(concat(
    [for c in var.app_configs : c.key],
    [for c in var.app_config_overrides : c.key]
  ))

  keyed_app_configs = { for c in var.app_configs : c.key => c }
  keyed_app_config_overrides = { for c in var.app_config_overrides : c.key => c }

  # Environment specific overrides take precedence 
  # over those defined at the service level:

  app_configs = [for key in local.unique_keys : merge(
    lookup(local.keyed_app_configs, key, {}),
    lookup(local.keyed_app_config_overrides, key, {})
  )]

}

resource "azurerm_app_configuration_key" "service_config" {
  for_each = { for item in local.app_configs : item.key => item }

  configuration_store_id = var.workload_config.app_config_id
  key                    = each.value.key
  value                  = each.value.value
  label                  = each.value.label == null ? var.label_name : each.value.label
  content_type = each.value.isJson ? "application/json" : ""
}

resource "azurerm_key_vault_secret" "service_secrets" {
  for_each = { for item in var.vault_secrets : item.key => item }

  key_vault_id = var.workload_config.key_vault_id
  name = replace(each.key, "/[^a-zA-Z0-9]/", "")
  value        = each.value.secret
  
}

resource "azurerm_app_configuration_key" "service_secrets" {
  for_each = { for item in var.vault_secrets : item.key => item }

  configuration_store_id = var.workload_config.app_config_id
  key                    = each.value.key
  type                   = "vault"
  label                  = each.value.label == null ? var.label_name : each.value.label
  vault_key_reference    = "${var.workload_config.key_vault_uri}${azurerm_key_vault_secret.service_secrets[each.key].resource_versionless_id}"  
}  

