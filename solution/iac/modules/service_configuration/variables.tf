variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type = map(any)
}

variable "label_name" {
  description = "The default label used if not specified directly on the app_configs or vault_secrets objects."
  type = string
  default = ""
}

variable "app_configs" {
  type = list(object({
    key = string                    # The key of the configuration
    value = any                     # The value.  This can be a simple value or jsonencode 
    label = optional(string)        # The label of the value.  If not specified, label_name is used
    isJson = optional(bool, false)  # Indicates that the value contains encoded json
  }))
  default = []
}

variable "vault_secrets" {
  type = list(object({
    key = string                    # The key of the configuration.
    secret = any                    # The value of the secret.                 
    label = optional(string)        # The label of the value.  If not specified, label_name is used
  }))
  default = []
}