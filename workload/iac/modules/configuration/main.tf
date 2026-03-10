resource "random_string" "unique_postfix" {
  length  = 7
  special = false
}

# User Assigned Managed Identity for App Configuration encryption
resource "azurerm_user_assigned_identity" "appconfig_encryption" {
  name                = "uami-appconfig-encryption"
  resource_group_name = var.resource_group_name
  location            = var.location
}

resource "azurerm_key_vault" "kv" {
  name                        = "${var.key_vault_name}-${random_string.unique_postfix.result}"
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = true
  tenant_id                   = var.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = true # required for encryption key scenarios
  rbac_authorization_enabled  = true
  sku_name = "standard"
}

# Allow the UAMI to use the key for encryption/decryption
resource "azurerm_role_assignment" "appconfig_kv_crypto" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_user_assigned_identity.appconfig_encryption.principal_id
}

# Terraform Developers Role:
resource "azurerm_role_assignment" "kv_owner" {
  scope                = azurerm_key_vault.kv.id
  principal_type = "Group"
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.workload_developers_group_id
}

# Encryption key in Key Vault
resource "azurerm_key_vault_key" "encryption_key" {
  name         = "appconfig-encryption-key"
  key_vault_id = azurerm_key_vault.kv.id
  key_type     = "RSA"
  key_size     = 2048

  key_opts = [
    "unwrapKey",
    "wrapKey",
  ]

  depends_on = [azurerm_role_assignment.kv_owner]
}

resource "azurerm_app_configuration" "app_configuration" {
  name                       = "${var.app_config_name}-${random_string.unique_postfix.result}"
  resource_group_name        = var.resource_group_name
  location                   = var.location
  sku                        = "standard"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.appconfig_encryption.id]
  }

  encryption {
    key_vault_key_identifier = azurerm_key_vault_key.encryption_key.id
    identity_client_id       = azurerm_user_assigned_identity.appconfig_encryption.client_id
  }

  depends_on = [azurerm_role_assignment.appconfig_kv_crypto]
}

# Terraform Developers Role:
resource "azurerm_role_assignment" "app_config_owners" {
  principal_id         = var.workload_developers_group_id
  principal_type =      "Group"
  role_definition_name = "App Configuration Data Owner"
  scope                = azurerm_app_configuration.app_configuration.id
}