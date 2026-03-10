variable "workload_config" {
  description = "Reference to the workflow configuration to which the solution belongs."
  type = map(any)
}

# variable "app_config_id" {
#   description = "The Id of the app configuration service."
#   type = string
# }

# variable "key_vault_uri" {
#   type = string
# }

# variable "key_vault_id" {
#   type = string
# }

# variable "servicebus_namespace_id" {
#   description = "The ID of the Service Bus namespace"
#   type        = string
# }

# variable "solution_identity_principal_id" {
#   description = "The client ID of the workload identity federated credential."
#   type        = string
# }

# variable "developers_principal_id" {
#   description = "The identity of the EntraId group for the solutions developers."
# }

