locals {
  location             = var.location
  rg_name              = lower("${var.solution_name}-${var.environment}-${var.location}")
  github_identity_name = "github-workflow-identity"
}

# Defines the resource group containing the solution's resources.
resource "azurerm_resource_group" "solution" {
  name     = local.rg_name
  location = local.location
}

# Defines the GitHub Actions identity and federated credential to access azure resources.
module "github_identity" {
  source = "./modules/github"

  resource_group_name = azurerm_resource_group.solution.name
  solution_name       = var.solution_name
  location            = local.location
  environment         = var.environment
  identity_name       = local.github_identity_name
  github_account_name = var.github_account_name

}

# Give GitHub Contributor access to the resource group.
resource "azurerm_role_assignment" "github_actions_resource_group_contributor" {
  scope                = azurerm_resource_group.solution.id
  role_definition_name = "Contributor"
  principal_id         = module.github_identity.azure_principal_id
}

