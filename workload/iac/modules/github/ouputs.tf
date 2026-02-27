# The outputs for build identity:
output "azure_build_client_id" {
  value       = azurerm_user_assigned_identity.github_build_identity.client_id
  description = "The client ID used by github build actions to access azure resources."
}

output "azure_build_tenant_id" {
  value       = azurerm_user_assigned_identity.github_build_identity.tenant_id
  description = "The tenant ID used by github build actions to access azure resources."
}

output "azure_build_principal_id" {
  value       = azurerm_user_assigned_identity.github_build_identity.principal_id
  description = "The principal ID grated access to azure resources for github build actions."
}

# The outputs for deploy identity:
output "azure_deploy_client_id" {
  value       = azurerm_user_assigned_identity.github_deploy_identity.client_id
  description = "The client ID used by github deploy actions to access azure resources."
}

output "azure_deploy_tenant_id" {
  value       = azurerm_user_assigned_identity.github_deploy_identity.tenant_id
  description = "The tenant ID used by github deploy actions to access azure resources."
}

output "azure_deploy_principal_id" {
  value       = azurerm_user_assigned_identity.github_deploy_identity.principal_id
  description = "The principal ID grated access to azure resources for github deploy actions."
}
