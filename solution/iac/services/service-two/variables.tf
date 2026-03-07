variable "servicebus_namespace_id" {
  description = "The ID of the Service Bus namespace"
  type        = string
}

variable "solution_identity_principal_id" {
  description = "The principal ID of the workload identity federated credential."
  type        = string
}

variable "solution_developers_group_id" {
  description = "The identity of the EntraId group for the solutions developers."
}