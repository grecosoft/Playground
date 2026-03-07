locals {
  location             = var.location
  workload_env_name    = lower("${var.workload_name}-${var.environment}-${var.location}")
  github_identity_name = "actions-identity"
}

# Defines the resource group containing the workload's environment resources.
resource "azurerm_resource_group" "workload" {
  name     = local.workload_env_name
  location = local.location
}

# Defines the GitHub Actions identity and federated credential to access azure resources.
module "github_identity" {
  source = "./modules/github"

  resource_group_name = azurerm_resource_group.workload.name
  workload_name       = var.workload_name
  location            = local.location
  environment         = var.environment
  identity_name       = local.github_identity_name
  github_account_name = var.github_account_name

}

# Defines the service bus namespace used by workload environment services.
module "servicebus_namespace" {
  source = "./modules/servicebus-namespace"

  resource_group_name = azurerm_resource_group.workload.name
  name                = "sb-namespace"
  location            = local.location
}



