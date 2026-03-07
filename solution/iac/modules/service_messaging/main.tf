# This module creates Azure Service Bus queues for request/reply messaging and assigns 
#appropriate permissions to the solution's workload identity and developers group.
locals {
  reply_queue_name = lower("${var.service_name}-reply-queue")
  request_queue_name = lower("${var.service_name}-request-queue")

  principals = {
    group    = { principal_id = var.solution_developers_group_id,   principal_type = "Group" }
    identity = { principal_id = var.solution_identity_principal_id, principal_type = "ServicePrincipal" }
  }
}

# Enable receiving message from other services in the solution over Azure Service Bus queues.
resource "azurerm_servicebus_queue" "request_queue" {
  name         = local.request_queue_name
  namespace_id = var.servicebus_namespace_id
}

# Enable sending reply message to other services in the solution over Azure Service Bus queues.
resource "azurerm_servicebus_queue" "reply_queue" {
  name         = local.reply_queue_name
  namespace_id = var.servicebus_namespace_id
  requires_session = true
}

resource "azurerm_role_assignment" "request_queue_senders" {
  for_each = local.principals
  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_queue.request_queue.id
}

resource "azurerm_role_assignment" "request_queue_receivers" {
  for_each = local.principals
  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_queue.request_queue.id
}

resource "azurerm_role_assignment" "reply_queue_senders" {
  for_each = local.principals
  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_queue.reply_queue.id
}

resource "azurerm_role_assignment" "reply_queue_receivers" {
  for_each = local.principals
  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_queue.reply_queue.id
}
