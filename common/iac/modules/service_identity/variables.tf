variable "resource_group_name" {
  type = string
}

variable "service_name" {
  type = string
}

variable "location" {
  type = string
}

# The name of the AKS cluster to integrate with workload identity.
variable "oidc_issuer_url" {
  description = "The OIDC issuer url of the AKS cluster."
  type        = string
}

variable "namespace" {
  type        = string
  description = "The Kubernetes namespace to create ServiceAccount for the workload identity."
}