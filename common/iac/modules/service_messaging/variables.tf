variable "solution_servicebus" {
  description = "Information about the Azure Service Bus defined at the solution level used to send message between services."
  type = map(any)
}

variable "solution_messaging" {
  description = "Information about the defined Service Bus Topic and Queue entities used to send message between services."
  type = map(any)
}

variable "solution_name" {
  description = "The name of the solution the service is associated."
  type = string
}

variable "service_name" {
  description = "The name of the service, used for naming resources and routing messages. This should be unique across the entire solution."
  type = string
}

variable "service_id" {
  description = "The unique identifier for the service, used to route messages to the correct service instance. This should be unique across the entire solution."
  type = string
}

variable "service_principal_id" {
  description = "The identity of the under which the services executes."
  type = string
}

variable "developers_group_id" {
  description = "The principal ID of the EntraId group containing developers granted access to solution level resources."
  type = string
}

variable "rpc_reply_timeout_seconds" {
  description = "The number of seconds to wait for a response to a RPC style of command."
  type = number
  default = 5
}

variable "dependent_services" {
  type        = map(string)
  default     = {}
  description = <<-EOT
      The list of services to which the service being configured can send messages. The key of the map is the service's name referenced in code.
      The value is the identity of the service.
  EOT
}

