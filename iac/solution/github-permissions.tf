# Github build identity permissions:
resource "azurerm_role_assignment" "github_actions_build_resource_group_contributor" {
  scope                = azurerm_resource_group.solution.id
  role_definition_name = "Contributor"
  principal_id         = module.github_identity.azure_build_principal_id
}

resource "azurerm_role_assignment" "github_actions_build_container_registry_push" {
  scope                = data.azurerm_container_registry.acr.id
  role_definition_name = "AcrPush"
  principal_id         = module.github_identity.azure_build_principal_id
}


# Github deploy identity permissions:
resource "azurerm_role_assignment" "github_actions_deploy_resource_group_contributor" {
  scope                = azurerm_resource_group.solution.id
  role_definition_name = "Contributor"
  principal_id         = module.github_identity.azure_deploy_principal_id
}
