# Topic used to route RPC style commands between a solution's services:
resource "azurerm_servicebus_topic" "command_rpc_topic" {
  name         = "${var.solution_name}-command-rpc-topic"
  namespace_id = var.servicebus_namespace_id

  partitioning_enabled = true
}

resource "azurerm_role_assignment" "command_rpc_topic_sender" {
  principal_id         = var.solution_developers_group_id
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_topic.command_rpc_topic.id
}

resource "azurerm_role_assignment" "command_rpc_topic_receiver" {
  principal_id         = var.solution_developers_group_id
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_topic.command_rpc_topic.id
}


# Queue used by services to await responses for sent RPC style commands:
resource "azurerm_servicebus_queue" "command_rpc_reply_queue" {
  name             = "${var.solution_name}-reply-queue"
  namespace_id     = var.servicebus_namespace_id
  requires_session = true
}

resource "azurerm_role_assignment" "command_rpc_reply_queue_sender" {
  principal_id         = var.solution_developers_group_id
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_queue.command_rpc_reply_queue.id
}


resource "azurerm_role_assignment" "command_rpc_reply_queue_receiver" {
  principal_id         = var.solution_developers_group_id
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_queue.command_rpc_reply_queue.id
}











# # Services within a solution that communicate with other services each
# # define a subscription based on its name:
# resource "azurerm_servicebus_subscription" "service_commands_topic_subscription" {
#   for_each = { for srv in var.service_names : srv => srv }
  
#   name               = "${var.solution_name}-${each.key}-commands"
#   topic_id           = azurerm_servicebus_topic.solution_commands_topic.id
#   max_delivery_count = 1
# }

# resource "azurerm_servicebus_subscription_rule" "service_commands_topic_rule" {
#   for_each = { for srv in var.service_names : srv => srv }

#   name            = "${var.solution_name}-${each.key}-rule"
#   subscription_id = azurerm_servicebus_subscription.service_commands_topic_subscription[each.key].id
#   filter_type     = "SqlFilter"
#   sql_filter      = "service = '${var.solution_name}:${each.key}'"
# }











