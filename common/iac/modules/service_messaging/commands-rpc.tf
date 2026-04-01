# Allow the service principal to send and receive messages from the Service Bus topic used for solution commands.
# This allows the service to participate in the solution's messaging architecture by sending and receiving messages.
resource "azurerm_role_assignment" "command_rpc_topic_sender" {
  principal_id         = var.service_principal_id
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = var.solution_messaging.command_rpc_topic_id
}

resource "azurerm_role_assignment" "command_rpc_topic_receiver" {
  principal_id         = var.service_principal_id
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = var.solution_messaging.command_rpc_topic_id
}

# Allow the service principal to send and receive messages from the Service Bus queue used for solution replies.
# This allows the service to send and receive reply messages for RPC type interactions with other services in
# the solution.
resource "azurerm_role_assignment" "command_rpc_reply_queue_sender" {
  principal_id         = var.service_principal_id
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = var.solution_messaging.command_rpc_reply_queue_id
}

resource "azurerm_role_assignment" "command_rpc_reply_queue_receiver" {
  principal_id         = var.service_principal_id
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = var.solution_messaging.command_rpc_reply_queue_id
}

# Create a subscription for the service on the solution commands topic,
# with a filter to only receive messages intended for this service.
resource "azurerm_servicebus_subscription" "command_rpc_topic_subscription" {  
  name               = "${var.service_name}-rpc-commands"
  topic_id           = var.solution_messaging.command_rpc_topic_id
  max_delivery_count = 1
}

resource "azurerm_servicebus_subscription_rule" "command_rpc_topic_rule" {
  name            = "${var.service_name}-rpc-commands-rule"
  subscription_id = azurerm_servicebus_subscription.command_rpc_topic_subscription.id
  filter_type     = "SqlFilter"
  sql_filter      = "service_id = '${var.service_id}'"
}
