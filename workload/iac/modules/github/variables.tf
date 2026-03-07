variable "resource_group_name" {
  description = "The name of the resource group where the GitHub federated identity will be created."
  type = string
}

variable "workload_name" {
  description = "The name of the workload."
  type = string
}

variable "location" {
  description = "The Azure region where the resources will be deployed."
  type = string
}

variable "environment" {
  description = "The environment for the workload (e.g., dev, stg, prod)."
  type = string
}

variable "identity_name" {
  description = "The name of the identity to create for GitHub Actions."
  type = string

}

variable "github_account_name" {
  description = "The name of the github account where the workgroup source repository is located."
  type = string
}