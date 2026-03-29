variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type        = map(any)
}

variable "solution_name" {
  description = "The name of the solution deployed to the workload."
  type        = string
}

variable "solution_roles" {
  description = "List of roles added to the created solution app registration resource."
  type = map(object({
    allowed_member_types = list(string)
    description          = string
    display_name         = string
  }))
}

variable "role_user_assignments" {
  description = "Should only be used if EntraId groups are not being used.  If groups are being used, users should be added externally."
  type        = map(list(string))
  default     = {}
}

variable "create_groups" {
  description = "If specified, EntraId groups will be created for each role and have the role assigned."
  type        = bool
  default     = false
}

variable "redirect_uris" {
  type    = list(string)
  default = []
}