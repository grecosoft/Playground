resource "azurerm_servicebus_queue" "reply_queue" {
  name         = local.reply_queue_name
  namespace_id = var.servicebus_namespace_id

  partitioning_enabled = true
}