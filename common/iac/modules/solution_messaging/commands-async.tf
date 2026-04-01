# Topic used to route RPC style commands between a solution's services:
resource "azurerm_servicebus_topic" "command_async_topic" {
  name         = "${var.solution_name}-command-async-topic"
  namespace_id = var.servicebus_namespace_id

  partitioning_enabled = true
}

resource "azurerm_role_assignment" "command_async_topic_sender" {
  principal_id         = var.solution_developers_group_id
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_topic.command_async_topic.id
}

resource "azurerm_role_assignment" "command_async_topic_receiver" {
  principal_id         = var.solution_developers_group_id
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_topic.command_async_topic.id
}