output "GITHUB_SECRETS" {
  value = {
    Description           = "Add the following as Github Repository Secrets for Environment: ${var.environment}"
    AZURE_TENANT_ID       = module.github_identity.azure_tenant_id
    AZURE_CLIENT_ID       = module.github_identity.azure_client_id
    AZURE_SUBSCRIPTION_ID = var.subscription_id
  }
}