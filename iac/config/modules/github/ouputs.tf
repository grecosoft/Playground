output "azure_client_id" {
  value       = azurerm_user_assigned_identity.github_actions.client_id
  description = "The client ID used by github actions to access azure resources."
}

output "azure_tenant_id" {
  value       = azurerm_user_assigned_identity.github_actions.tenant_id
  description = "The tenant ID used by github actions to access azure resources."
}

output "azure_principal_id" {
  value       = azurerm_user_assigned_identity.github_actions.principal_id
  description = "The principal ID grated access to azure resources for github actions."
}
