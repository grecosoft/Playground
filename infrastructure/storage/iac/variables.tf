variable "subscription_id" {
  description = "The Subscription ID where the resources will be created."
  type        = string
}

variable "resource_group_name" {
  description = "The resource group to create service account for Terraform state storage."
  type        = string
}

variable "storage_account_name" {
  description = "The storage account name to create for Terraform state storage."
  type        = string
}

variable "workload_name" {
  description = "The workload name to use for naming the storage containers."
  type        = string
}

#Note:  make this a list in the future if we want to support multiple solutions per workload
variable "solution_name" {
  description = "The name of the solution contained within a workload."
}

variable "environments" {
  description = "The list of environments to create storage containers for."
  type        = list(string)
}