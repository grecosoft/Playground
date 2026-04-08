# Github build identity permissions:
resource "azurerm_role_assignment" "github_build_resource_group_contributor" {
  scope                = azurerm_resource_group.solution.id
  role_definition_name = "Contributor"
  principal_id         = module.github.azure_build_principal_id 
}

resource "azurerm_role_assignment" "github_build_container_registry_push" {
  scope                = data.azurerm_container_registry.acr.id
  role_definition_name = "AcrPush"
  principal_id         = module.github.azure_build_principal_id 
}


# Github environment identity permissions:
resource "azurerm_role_assignment" "github_environment_resource_group_contributor" {
  scope                = azurerm_resource_group.solution.id
  role_definition_name = "Contributor"
  principal_id         = module.github.azure_environment_principal_id 
}

resource "azurerm_role_assignment" "AksContributor" {
  principal_id                     = module.github.azure_environment_principal_id
  role_definition_name             = "Azure Kubernetes Service Cluster User Role"
  scope                            = data.azurerm_kubernetes_cluster.k8s.id
  skip_service_principal_aad_check = true 
}

#  -- The deployment workflow needs permission to read any variables to be
#  -- used during deployment, such as the WORKLOAD_CLIENT_ID and WORKLOAD_TENANT_ID 
#  -- output by the service_deploy module.
resource "azurerm_role_assignment" "github_environment_app_config_reader" {
  principal_id         = module.github.azure_environment_principal_id
  role_definition_name = "App Configuration Data Reader"
  scope                = module.configuration.app_config_id
}