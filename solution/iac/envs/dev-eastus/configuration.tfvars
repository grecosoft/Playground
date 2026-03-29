subscription_id     = "c47473b5-b6e9-476b-853e-a1f5b826e95b"
solution_name       = "Playground"
environment         = "dev"
location            = "eastus"

// Accounts:
developer_group_name = "solution-developers"

# External resource dependencies:
infrastructure_resource_group_name = "infrastructure-dev-eastus"
container_registry_name            = "registryworkingstork"
cluster_name                       = "aks-cluster"

# Referenced Module Variable Overrides:
github = {
  account_name = "grecosoft"
}
