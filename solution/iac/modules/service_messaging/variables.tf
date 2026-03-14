variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type        = map(any)
}

variable "service_name" {
  type = string
}