variable "subscription_id" {
  description = "The Subscription ID where the resources will be created."
  type = string
}

variable "resource_group_name" {
  description = "The resource group to create service account for Terraform state storage."
  type = string
}

variable "storage_account_name" {
  description = "The storage account name to create for Terraform state storage."
  type = string
}

variable "solution_name" {
  description = "The solution name to use for naming the storage containers."
}

variable "environments" {
  description = "The list of environments to create storage containers for."
  type = list(string)
}