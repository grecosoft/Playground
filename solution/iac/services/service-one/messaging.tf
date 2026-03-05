resource "azurerm_servicebus_queue" "reply_queue" {
  name         = local.reply_queue_name
  namespace_id = var.servicebus_namespace_id

  partitioning_enabled = true
}

resource "azurerm_role_assignment" "AksContributor" {
  principal_id                     = var.solution_identity_client_id
  role_definition_name             = "Azure Service Bus Data Receiver"
  scope                            = azurerm_servicebus_queue.reply_queue.id
  skip_service_principal_aad_check = true
}