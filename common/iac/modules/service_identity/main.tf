resource "random_uuid" "service_id" {
}

// Create an user identity and federate with the cluster:
resource "azurerm_user_assigned_identity" "service_identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "${lower(var.service_name)}-sa"
}

resource "azurerm_federated_identity_credential" "service_federated_identity" {
  name      = "${azurerm_user_assigned_identity.service_identity.name}-fed"
  audience  = ["api://AzureADTokenExchange"]
  issuer    = var.oidc_issuer_url
  user_assigned_identity_id = azurerm_user_assigned_identity.service_identity.id
  subject   = "system:serviceaccount:${var.namespace}:${var.service_name}-sa"
}