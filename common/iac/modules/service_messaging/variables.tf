variable "solution_servicebus" {
  type = map(any)
}

variable "solution_messaging" {
  type = map(any)
}

variable "solution_name" {
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
  type = string
}

variable "developers_group_id" {
  description = "The principal ID of the EntraId group containing developers granted access to solution level resources."
  type = string
}

variable "rpc_reply_timeout_seconds" {
  type = number
  default = 5
}
