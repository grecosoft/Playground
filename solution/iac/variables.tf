variable "subscription_id" {
  description = "The Subscription ID where the solution's resource group resources will be created."
  type        = string
}

variable "workload_name" {
  description = "Name of the workload to which the solution belongs. A workload is a collection of services that work together to deliver value. For example, an e-commerce workload can be composed of a web application, an API, and a database."
  type        = string
}

variable "solution_name" {
  description = "Name of the solution deployed to the workload."
  type        = string
}

variable "environment" {
  description = "The environment to deploy the solution in."
  type        = string
}

variable "location" {
  description = "The Azure region to deploy the resources in."
  type        = string
}

# Kubernetes workload identity variables:
variable "namespace" {
  type        = string
  description = "The Kubernetes namespace to create ServiceAccount for the workload identity."
}

# Remote state configuration variables:
variable "storage_resource_group_name" {
  description = "The name of the resource group where the storage account for remote state is located."
  type        = string
}

variable "storage_account_name" {
  description = "The name of the storage account for remote state."
  type        = string
}

variable "workload_container_name" {
  description = "The name of the container containing the state of the workload."
  type        = string
}