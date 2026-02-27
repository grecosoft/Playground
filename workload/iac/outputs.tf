output "GITHUB_BUILD_SECRETS" {
  value = {
    Description           = "Add the following as Github Repository Secrets"
    AZURE_TENANT_ID       = module.github_identity.azure_build_tenant_id
    AZURE_CLIENT_ID       = module.github_identity.azure_build_client_id
    AZURE_SUBSCRIPTION_ID = var.subscription_id
  }
}

output "GITHUB_ENVIRONMENT_SECRETS" {
  value = {
    Description           = "Add the following as Github Repository Secrets for Environment: ${var.environment}"
    AZURE_TENANT_ID       = module.github_identity.azure_deploy_tenant_id
    AZURE_CLIENT_ID       = module.github_identity.azure_deploy_client_id
    AZURE_SUBSCRIPTION_ID = var.subscription_id
  }
}

output "servicebus_namespace" {
  value = {
    id   = module.servicebus_namespace.id
    name = module.servicebus_namespace.name
  }
}