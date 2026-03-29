variable "solution_name" {
  description = "The name of the solution for which messaging resources should be created."
  type = string  
}

variable "servicebus_namespace_id" {
  type = string
}

variable "solution_developers_group_id" {
  description = "The principal ID of the EntraId group containing developers granted access to solution level resources."
  type = string
}

# variable "service_names" {
#   description = "Name of the services within the solution that can communicated with each other."
#   type = list(string)
# }