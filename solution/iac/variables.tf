variable "subscription_id" {
  description = "The Subscription ID where the solution's resource group resources will be created."
  type        = string
}

variable "solution_name" {
  description = "Name of the solution to be developed containing resources shared between microservices"
  type        = string
}

variable "developer_group_name" {
  description = "The name of the EntraId group containing developer granted access to workload level resources."
  type = string
}

variable "environment" {
  description = "The environment to deploy the solution in."
  type        = string
}

variable "location" {
  type = string
}


# External resource dependencies:
variable "infrastructure_resource_group_name" {
  description = "The name of the resource group containing the Azure container registry and Kubernetes cluster."
  type        = string
}

variable "container_registry_name" {
  description = "The name of the Azure Container Registry to store container images."
  type        = string
}

variable "cluster_name" {
  description = "The name of the Kubernetes cluster."
  type        = string
}

# Referenced Module Variable Overrides:
variable "github" {
  type = object({
    identity_base_name = optional(string, "github-actions")
    account_name = string
  })
}

variable "configuration" {
  type = object({
    key_vault_name = optional(string)
    app_config_name = optional(string)
  })
  default = {}
}

variable "authorization" {
  type = object({
    redirect_uris = list(string)
  })
}