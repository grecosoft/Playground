data "azurerm_client_config" "current" {}

data "azuread_group" "solution_developers" {
  display_name     = var.developer_group_name
  security_enabled = true
}

resource "random_string" "resource_postfix" {
  length  = 6
  upper   = false
  special = false
}

locals {
  location             = var.location
  tenant_id = data.azurerm_client_config.current.tenant_id
  solution_env_name    = lower("${var.solution_name}-${var.environment}-${var.location}")
  resource_postfix     = random_string.resource_postfix.result
}

# Defines the resource group containing the solution's resources.
resource "azurerm_resource_group" "solution" {
  name     = local.solution_env_name
  location = local.location
}

# Defines the GitHub Actions identity and federated credential to access azure resources.
module "github" {
  source = "../../common/iac/modules/solution_github"

  resource_group_name = azurerm_resource_group.solution.name 
  solution_name       = var.solution_name
  environment         = var.environment
  location            = local.location
  identity_base_name  = var.github.identity_base_name
  account_name        = var.github.account_name 
}

# Defines resources used for solution configurations.
module "configuration" {
  source = "../../common/iac/modules/solution_configuration"

  resource_group_name = azurerm_resource_group.solution.name 
  location = local.location
  environment         = var.environment
  tenant_id = local.tenant_id 
  resource_postfix = local.resource_postfix
  key_vault_name =  var.configuration.key_vault_name
  app_config_name = var.configuration.app_config_name
  developers_group_id = data.azuread_group.solution_developers.object_id    
}

# TODO:  Add solution_authorization module to create Enterprise Application, App Roles, and Groups for solution access control.

# Defines resources used for solution messaging between services.
module "messaging" {
  source = "../../common/iac/modules/solution_messaging"
 
  solution_name = var.solution_name
  servicebus_namespace_id = azurerm_servicebus_namespace.service_bus.id
  solution_developers_group_id = data.azuread_group.solution_developers.object_id 
}