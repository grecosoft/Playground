variable "resource_group_name" {
  type = string
}

variable "solution_name" {
  type = string
}

variable "location" {
  type = string
}

variable "oidc_issuer_url" {
  description = "The OIDC issuer url of the AKS cluster."
  type        = string
}

variable "namespace" {
  type        = string
  description = "The Kubernetes namespace to create ServiceAccount for the workload identity."
}