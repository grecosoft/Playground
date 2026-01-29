# Defines an user defined identity used by github actions to access azure resources.
resource "azurerm_user_assigned_identity" "github_actions" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = var.identity_name
}

resource "azurerm_federated_identity_credential" "github_main_branch" {
  name                = "github-build-federated-credential"
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.github_actions.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_account_name}/${var.solution_name}:ref:refs/heads/main"
}

resource "azurerm_federated_identity_credential" "github_actions" {
  name                = "github-${var.environment}-federated-credential"
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.github_actions.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_account_name}/${var.solution_name}:environment:${var.environment}"
}
