variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "key_vault_name" {
  type = string
}

variable "app_config_name" {
    type = string
}

variable "workload_developers_group_id" {
  description = "The identity of the EntraId group for the workload developers."
  type        = string
}

