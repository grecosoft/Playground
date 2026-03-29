variable "resource_group_name" {
  description = "The name of the resource group where the GitHub federated identity will be created."
  type = string
}

variable "solution_name" {
  description = "The name of the solution."
  type = string
}

variable "location" {
  description = "The Azure region where the resources will be deployed."
  type = string
}

variable "environment" {
  description = "The environment for the solution (e.g., dev, stg, prod)."
  type = string
}

variable "identity_base_name" {
  description = "The base name of the identity to create for GitHub Actions."
  type = string

}

variable "account_name" {
  description = "The name of the github account where the solution's source repository is located."
  type = string
}