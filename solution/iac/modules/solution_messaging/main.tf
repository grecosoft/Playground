locals {
  principals = {
   developers_group  = { principal_id = var.workload_config.developers_group_principal_id, principal_type = "Group" }
   solution_identity = { principal_id = var.workload_config.solution_identity_principal_id, principal_type = "ServicePrincipal" }
  }
}

# Topic used to route commands between a solution's services:
resource "azurerm_servicebus_topic" "solution_commands_topic" {
  name         = "${var.solution_name}-command-topic"
  namespace_id = var.workload_config.servicebus_namespace_id

  partitioning_enabled = true
}

resource "azurerm_role_assignment" "solution_commands_topic_sender" {
  for_each = local.principals 

  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_topic.solution_commands_topic.id
}

resource "azurerm_role_assignment" "solution_commands_topic_receiver" {
  for_each = local.principals 

  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_topic.solution_commands_topic.id
}



# Services within a solution that communicate with other services each
# define a subscription based on its name:
resource "azurerm_servicebus_subscription" "service_commands_topic_subscription" {
  for_each = { for srv in var.service_names : srv => srv }
  
  name               = "${var.solution_name}-${each.key}-commands"
  topic_id           = azurerm_servicebus_topic.solution_commands_topic.id
  max_delivery_count = 1
}

resource "azurerm_servicebus_subscription_rule" "service_commands_topic_rule" {
  for_each = { for srv in var.service_names : srv => srv }

  name            = "${var.solution_name}-${each.key}-rule"
  subscription_id = azurerm_servicebus_subscription.service_commands_topic_subscription[each.key].id
  filter_type     = "SqlFilter"
  sql_filter      = "service = '${var.solution_name}:${each.key}'"
}

# Queue used by services to await responses for sent RPC type of commands:
resource "azurerm_servicebus_queue" "solution_reply_queue" {
  name             = "${var.solution_name}-reply-queue"
  namespace_id     = var.workload_config.servicebus_namespace_id
  requires_session = true
}

resource "azurerm_role_assignment" "solution_reply_queue_sender" {
  for_each = local.principals 

  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Sender"
  scope                = azurerm_servicebus_queue.solution_reply_queue.id
}

resource "azurerm_role_assignment" "solution_reply_queue_receiver" {
  for_each = local.principals 

  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = "Azure Service Bus Data Receiver"
  scope                = azurerm_servicebus_queue.solution_reply_queue.id
}




