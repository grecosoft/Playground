locals {
  postfix = "${var.environment}-${var.location}-${var.resource_postfix}"
  key_vault_name = coalesce(var.key_vault_name, "kv-${local.postfix}")
  app_config_name = coalesce(var.app_config_name, "ac-${local.postfix}")
}

# User Assigned Managed Identity for App Configuration encryption
resource "azurerm_user_assigned_identity" "appconfig_encryption" {
  name                = "uai-appconfig-encryption"
  resource_group_name = var.resource_group_name
  location            = var.location
}

resource "azurerm_key_vault" "key_vault" {
  name                        = local.key_vault_name
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
  scope                = azurerm_key_vault.key_vault.id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_user_assigned_identity.appconfig_encryption.principal_id
}

# Terraform Developers Role:
resource "azurerm_role_assignment" "kv_owner" {
  scope                = azurerm_key_vault.key_vault.id
  principal_type = "Group"
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.developers_group_id
}

# Encryption key in Key Vault
resource "azurerm_key_vault_key" "encryption_key" {
  name         = "appconfig-encryption-key"
  key_vault_id = azurerm_key_vault.key_vault.id
  key_type     = "RSA"
  key_size     = 2048

  key_opts = [
    "unwrapKey",
    "wrapKey",
  ]

  depends_on = [azurerm_role_assignment.kv_owner]
}

resource "azurerm_app_configuration" "app_config" {
  name                       = local.app_config_name
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
  principal_id         = var.developers_group_id
  principal_type =      "Group"
  role_definition_name = "App Configuration Data Owner"
  scope                = azurerm_app_configuration.app_config.id
}