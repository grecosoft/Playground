variable "subscription_id" {
  description = "The Subscription ID where the solution's resource group resources will be created."
  type        = string
}

variable "solution_name" {
  description = "Name of the solution to be developed containing resources shared between microservices"
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

