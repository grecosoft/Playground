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

output "workload_env_name" {
  value = local.workload_env_name
}

output "kubernetes_cluster" {
  value = {
    id   = data.azurerm_kubernetes_cluster.k8s.id
    name = data.azurerm_kubernetes_cluster.k8s.name
    oidc_issuer_url = data.azurerm_kubernetes_cluster.k8s.oidc_issuer_url
  }
}

output "configuration" {
  value = {
    key_vault_name = module.configuration.key_vault_name
    key_vault_id = module.configuration.key_vault_id
    key_vault_uri = module.configuration.key_vault_uri
    app_config_name = module.configuration.app_config_name
    app_config_id = module.configuration.app_config_id
  }
}

output "servicebus_namespace" {
  value = {
    id   = module.messaging.servicebus_namespace_id
    name = module.messaging.servicebus_namespace_name
  }
}