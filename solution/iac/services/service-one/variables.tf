variable "servicebus_namespace_id" {
  description = "The ID of the Service Bus namespace"
  type        = string
}

variable "solution_identity_client_id" {
  description = "The client ID of the workload identity federated credential."
  type        = string
}