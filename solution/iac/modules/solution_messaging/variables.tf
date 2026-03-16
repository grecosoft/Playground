variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type        = map(any)
}

variable "solution_name" {
  description = "The name of the solution for which messaging resources should be created."
  type = string
  
}

variable "service_names" {
  description = "Name of the services within the solution that can communicated with each other."
  type = list(string)
}