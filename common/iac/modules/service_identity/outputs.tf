output "service_id" {
  value = random_uuid.service_id.result
}

output "client_id" {
  value = azurerm_user_assigned_identity.service_identity.client_id
}

output "tenant_id" {
  value = azurerm_user_assigned_identity.service_identity.tenant_id
}

output "principal_id" {
  description = "The principal ID of the workload identity federated credential."
  value       = azurerm_user_assigned_identity.service_identity.principal_id
}

