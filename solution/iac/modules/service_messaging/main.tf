resource "azurerm_servicebus_queue" "request_queue" {
  name         = local.request_queue_name
  namespace_id = var.workload_config.servicebus_namespace_id
}

resource "azurerm_servicebus_queue" "reply_queue" {
  name             = local.reply_queue_name
  namespace_id     = var.workload_config.servicebus_namespace_id
  requires_session = true
}

locals {
  reply_queue_name   = lower("${var.service_name}-reply-queue")
  request_queue_name = lower("${var.service_name}-request-queue")

  principals = {
    group    = { principal_id = var.workload_config.developers_group_principal_id,  principal_type = "Group" }
    identity = { principal_id = var.workload_config.solution_identity_principal_id, principal_type = "ServicePrincipal" }
  }

  queue_role_assignments = flatten([
    for queue_key, queue_id in {
      request = azurerm_servicebus_queue.request_queue.id
      reply   = azurerm_servicebus_queue.reply_queue.id
    } : [
      for role in ["Azure Service Bus Data Sender", "Azure Service Bus Data Receiver"] : [
        for principal_key, principal in local.principals : {
          key            = "${queue_key}-${role}-${principal_key}"
          principal_id   = principal.principal_id
          principal_type = principal.principal_type
          role           = role
          scope          = queue_id
        }
      ]
    ]
  ])
}

resource "azurerm_role_assignment" "queue_assignments" {
  for_each = { for item in local.queue_role_assignments : item.key => item }

  principal_id         = each.value.principal_id
  principal_type       = each.value.principal_type
  role_definition_name = each.value.role
  scope                = each.value.scope
}