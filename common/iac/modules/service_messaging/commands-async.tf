resource "azurerm_role_assignment" "command_async_topic_sender" {
  principal_id         = var.service_principal_id
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = var.solution_messaging.command_async_topic_id
}

resource "azurerm_role_assignment" "command_async_topic_receiver" {
  principal_id         = var.service_principal_id
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = var.solution_messaging.command_async_topic_id
}


resource "azurerm_servicebus_subscription" "command_async_topic_subscription" {  
  name               = "${var.service_name}-async-commands"
  topic_id           = var.solution_messaging.command_async_topic_id
  max_delivery_count = var.async_max_delivery_count
}

resource "azurerm_servicebus_subscription_rule" "command_async_topic_rule" {
  name            = "${var.service_name}-async-commands-rule"
  subscription_id = azurerm_servicebus_subscription.command_async_topic_subscription.id
  filter_type     = "SqlFilter"
  sql_filter      = "service_id = '${var.service_id}'"
}
