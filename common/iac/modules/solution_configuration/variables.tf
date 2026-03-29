variable "resource_group_name" {
  type = string
}

variable "environment" {
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

variable "resource_postfix" {
  type = string
}

variable "developers_group_id" {
  description = "The identity of the EntraId group for the solution developers."
  type        = string
}

