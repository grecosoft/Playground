variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type = map(any)
}

variable "env_service_configs" {
  description = "Contains a mapping over environment overrides.  The key will most often be the service name."
  type = map(
      list(object({
        key = string                    # The key of the configuration
        value = any                     # The value.  This can be a simple value or jsonencode 
        label = optional(string)        # The label of the value.  If not specified, label_name is used
        isJson = optional(bool, false)  # Indicates that the value contains encoded json
      }))
    )
  default = {}
}

