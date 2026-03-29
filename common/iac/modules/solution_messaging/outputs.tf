output "command_rpc_topic_name" {
  value = azurerm_servicebus_topic.command_rpc_topic.name
}

output "command_rpc_topic_id" {
  value = azurerm_servicebus_topic.command_rpc_topic.id
}

output "command_rpc_reply_queue_name" {
  value = azurerm_servicebus_queue.command_rpc_reply_queue.name
}

output "command_rpc_reply_queue_id" {
  value = azurerm_servicebus_queue.command_rpc_reply_queue.id
}