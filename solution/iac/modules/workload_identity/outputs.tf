output "principal_id" {
  description = "The principal ID of the workload identity federated credential."
  value       = azurerm_user_assigned_identity.solution-identity.principal_id
}