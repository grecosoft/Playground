locals {
  location             = var.location
  rg_name              = lower("${var.workload_name}-${var.environment}-${var.location}")
  github_identity_name = "github-workflow-identity"
}

# TODO: Renamed to workload
# Defines the resource group containing the workroom's resources.
resource "azurerm_resource_group" "solution" {
  name     = local.rg_name
  location = local.location
}

# Defines the GitHub Actions identity and federated credential to access azure resources.
module "github_identity" {
  source = "./modules/github"

  resource_group_name = azurerm_resource_group.solution.name
  workload_name       = var.workload_name
  location            = local.location
  environment         = var.environment
  identity_name       = local.github_identity_name
  github_account_name = var.github_account_name

}

# Defines the service bus namespace used by workload solution services.
module "servicebus_namespace" {
  source = "./modules/servicebus-namespace"

  resource_group_name = azurerm_resource_group.solution.name
  name                = "sb-namespace"
  location            = local.location
}



