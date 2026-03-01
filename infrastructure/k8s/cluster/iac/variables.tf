variable "subscription_id" {
  description = "The Subscription ID where the resources will be created."
  type        = string
}

variable "location" {
  type = string
}

variable "environment" {
  description = "The environment to deploy the solution in."
  type        = string
}

variable "resource_group_name" {
  description = "The resource group to create service account for Terraform state storage."
  type        = string
}

variable "cluster_name" {
  description = "The name of the Kubernetes cluster."
  type        = string
}

variable "container_registry_name" {
  type = string
}

variable "container_registry_sku" {
  type = string
}
