# Defines an user defined identity used by github actions to access azure resources.

# The following federated identity credential allows github actions running on the main branch:
resource "azurerm_user_assigned_identity" "github_build_identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "uai-${var.identity_base_name}-build"
}

resource "azurerm_federated_identity_credential" "github_main_branch" {
  name                = "github-build-federated-credential"
  user_assigned_identity_id = azurerm_user_assigned_identity.github_build_identity.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.account_name}/${var.solution_name}:ref:refs/heads/main"
}

# Defines an user defined identity used by github actions to deploy to specific environment:
resource "azurerm_user_assigned_identity" "github_environment_identity" {
  resource_group_name = var.resource_group_name
  location            = var.location
  name                = "uai-${var.identity_base_name}-${var.environment}"
}

resource "azurerm_federated_identity_credential" "github_environment" {
  name                = "github-${var.environment}-federated-credential"
  user_assigned_identity_id = azurerm_user_assigned_identity.github_environment_identity.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://token.actions.githubusercontent.com"
  subject             = "repo:${var.account_name}/${var.solution_name}:environment:${var.environment}"
}
