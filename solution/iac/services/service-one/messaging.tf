resource "azurerm_servicebus_queue" "reply_queue" {
  name         = local.reply_queue_name
  namespace_id = var.servicebus_namespace_id

  partitioning_enabled = true
}

resource "azurerm_role_assignment" "reply_queue_receivers" {
  for_each = {
    group    = { principal_id = var.solution_developers_group_id, principal_type = "Group" }
    identity = { principal_id = var.solution_identity_client_id,  principal_type = "ServicePrincipal" }
  }

  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_queue.reply_queue.id
}