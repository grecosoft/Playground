# Defines an user defined identity used by github actions to access azure resources.

# The following federated identity credential allows github actions running on the main branch:
resource "azurerm_user_assigned_identity" "github_build_identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "github-${var.identity_name}-build"
}

resource "azurerm_federated_identity_credential" "github_main_branch" {
  name                = "github-build-federated-credential"
  parent_id           = azurerm_user_assigned_identity.github_build_identity.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_account_name}/${var.workload_name}:ref:refs/heads/main"
}

# Defines an user defined identity used by github actions to deploy to specific environment:
resource "azurerm_user_assigned_identity" "github_deploy_identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "github-${var.identity_name}-${var.environment}"
}

resource "azurerm_federated_identity_credential" "github_actions" {
  name                = "github-${var.environment}-federated-credential"
  parent_id           = azurerm_user_assigned_identity.github_deploy_identity.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.github_account_name}/${var.workload_name}:environment:${var.environment}"
}
