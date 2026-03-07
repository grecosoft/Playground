// Create an user identity and federate with the cluster:
resource "azurerm_user_assigned_identity" "solution_identity" { # todo RENAME
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "${lower(var.solution_name)}-sa"
}

resource "azurerm_federated_identity_credential" "solution_federated_identity" {
  name                = "${azurerm_user_assigned_identity.solution_identity.name}-fed"
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.oidc_issuer_url
  parent_id           = azurerm_user_assigned_identity.solution_identity.id
  subject             = "system:serviceaccount:${var.namespace}:${var.solution_name}-sa"
}