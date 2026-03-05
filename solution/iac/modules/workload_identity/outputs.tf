output "solution_identity_client_id" {
  description = "The client ID of the workload identity federated credential."
  value       = azurerm_user_assigned_identity.solution-identity.client_id
}