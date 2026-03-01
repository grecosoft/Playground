variable "subscription_id" {
  description = "The Subscription ID where the solution's resource group resources will be created."
  type        = string
}

variable "workload_name" {
  description = "Name of the workload to be developed containing resources shared between microservices"
  type        = string
}

variable "environment" {
  description = "The environment to deploy the solution in."
  type        = string
}

variable "location" {
  type = string
}

variable "github_account_name" {
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