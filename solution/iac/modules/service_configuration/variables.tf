variable "app_config_id" {
    type = string
}

variable "key_vault_uri" {
  type = string
}

variable "key_vault_id" {
  type = string
}

variable "label_name" {
  description = "The default configuration label if not specified on the configs or secrets."
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
    key = string
    secret = string
    label = optional(string)
  }))
  default = []
}