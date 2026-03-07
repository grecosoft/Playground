// Create an user identity and federate with the cluster:
resource "azurerm_user_assigned_identity" "solution-identity" { # todo RENAME
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "${lower(var.solution_name)}-identity"
}

resource "azurerm_federated_identity_credential" "solution-federated-identity" {
  name                = "${azurerm_user_assigned_identity.solution-identity.name}-federated"
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.oidc_issuer_url
  parent_id           = azurerm_user_assigned_identity.solution-identity.id
  subject             = "system:serviceaccount:solution:solution-identity"
}