output "service_id" {
  value = random_uuid.service_id.result
}

output "principal_id" {
  description = "The principal ID of the workload identity federated credential."
  value       = azurerm_user_assigned_identity.service_identity.principal_id
}

