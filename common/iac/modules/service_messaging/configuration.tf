
output "service_config" {
  description = "Messaging configuration used by a service to send messages to other solution services."
  value = concat([
    { key = "ServiceMessaging:ServiceBusHostName", value = lower(var.solution_servicebus.host_name) },
    { key = "ServiceMessaging:ServiceId", value = var.service_id },
    { key = "ServiceMessaging:ServiceName", value = lower(var.service_name) },
    { key = "ServiceMessaging:SolutionName", value = lower(var.solution_name) },
    { key = "ServiceMessaging:RpcReplyTimeoutSeconds", value = var.rpc_reply_timeout_seconds }
  ],
  [
    for k, v in var.dependent_services : 
    { key = "ServiceMessaging:DependentService:${k}", value = v }
  ])
}