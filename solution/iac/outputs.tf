 output "github_build_secrets" {
  value = {
    Description           = "Add the following as Github Repository Secrets"
    AZURE_TENANT_ID       = module.github.azure_build_tenant_id
    AZURE_CLIENT_ID       = module.github.azure_build_client_id
    AZURE_SUBSCRIPTION_ID = var.subscription_id
  }
}

output "github_environment_secrets" {
  value = {
    Description           = "Add the following as Github Repository Secrets for Environment: ${var.environment}"
    AZURE_TENANT_ID       = module.github.azure_environment_tenant_id
    AZURE_CLIENT_ID       = module.github.azure_environment_client_id
    AZURE_SUBSCRIPTION_ID = var.subscription_id 
  }
}

output "solution_env_name" {
  value = local.solution_env_name
}

output "solution_location" {
  value = local.location
}

output "solution_developers_group_id" {
  value = data.azuread_group.solution_developers.object_id
}

output "kubernetes" {
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

output "servicebus" {
  value = {
    id   = azurerm_servicebus_namespace.service_bus.id
    name = azurerm_servicebus_namespace.service_bus.name
  }
}

output "messaging" {
  value = {
    command_rpc_topic_name = module.messaging.command_rpc_topic_name
    command_rpc_topic_id = module.messaging.command_rpc_topic_id
    command_rpc_reply_queue_name = module.messaging.command_rpc_reply_queue_name
    command_rpc_reply_queue_id = module.messaging.command_rpc_reply_queue_id
  }
}