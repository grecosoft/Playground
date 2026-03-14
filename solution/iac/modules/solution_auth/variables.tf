variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type = map(any)
}

variable "solution_name" {
  description = "The name of the solution deployed to the workload."
  type = string
}

variable "solution_roles" {
  description = "List of roles added to the created solution app registration resource."
  type = map(object({
    allowed_member_types = list(string)
    description = string
    display_name = string
  }))
}