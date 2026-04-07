# Solution level variables:
variable "subscription_id" {
  description = "The Subscription ID where the solution's resource group resources will be created."
  type        = string
}

variable "solution_name" {
  description = "Name of the solution deployed to the workload."
  type        = string
}

# Environment variables:

variable "environment" {
  description = "The environment to deploy the solution in."
  type        = string
}

variable "location" {
  description = "The Azure region to deploy the resources in."
  type        = string
}

# Configuration Environment Overrides:

variable "env_app_configs" {
  type = list(object({
    key    = string                # The key of the configuration
    value  = any                   # The value.  This can be a simple value or jsonencode 
    label  = optional(string)      # The label of the value.  If not specified, label_name is used
    isJson = optional(bool, false) # Indicates that the value contains encoded json
  }))
  default = []
}

# variable "environment_overrides" {
#   description = "Used to provide module and application configuration overrides for a specific environment."
#   type = object({

#     // Environment specific service configurations merged with common service configurations. 
#     service_configs = map(
#       list(object({
#         key    = string                # The key of the configuration
#         value  = any                   # The value.  This can be a simple value or jsonencode 
#         label  = optional(string)      # The label of the value.  If not specified, label_name is used
#         isJson = optional(bool, false) # Indicates that the value contains encoded json
#       }))
#     )

#     // overrides solution authorization module.
#     solution_auth = object({
#       redirect_uris = list(string)
#     })
#   })
# }

# Developer related variables:
variable "developer_group_name" {
  description = "The name of the Azure AD group that contains the developers who will have access to the solution's resources."
  type        = string
}

# # Kubernetes workload identity variables:
# variable "namespace" {
#   type        = string
#   description = "The Kubernetes namespace to create ServiceAccount for the workload identity."
# }

# Remote state configuration variables:
variable "storage_resource_group_name" {
  description = "The name of the resource group where the storage account for remote state is located."
  type        = string
}

variable "storage_account_name" {
  description = "The name of the storage account for remote state."
  type        = string
}

variable "solution_container_name" {
  description = "The name of the container containing the state of the solution."
  type        = string
}