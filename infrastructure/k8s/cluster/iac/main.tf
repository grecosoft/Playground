locals {
  location = var.location
  rg_name  = lower("${var.resource_group_name}-${var.environment}-${var.location}")
}

# Defines the resource group containing the infrastructure resources
# used by a given set of workloads.
resource "azurerm_resource_group" "infrastructure" {
  name     = local.rg_name
  location = local.location
}

# TODO change this to unique prefix...
resource "random_pet" "container_registry_name" {
  prefix    = var.container_registry_name
  separator = ""
  length    = 1
}

resource "azurerm_container_registry" "container_registry" {
  name                = random_pet.container_registry_name.id
  resource_group_name = azurerm_resource_group.infrastructure.name
  location            = azurerm_resource_group.infrastructure.location
  sku                 = var.container_registry_sku
}

resource "azurerm_kubernetes_cluster" "aks_cluster" {
  name                = var.cluster_name
  location            = azurerm_resource_group.infrastructure.location
  resource_group_name = azurerm_resource_group.infrastructure.name
  dns_prefix          = "${var.cluster_name}-dns"
  node_resource_group = "${var.cluster_name}-node"
  oidc_issuer_enabled       = true
  workload_identity_enabled = true

  # Free tier control plane
  sku_tier = "Free"

  default_node_pool {
    name       = "default"
    node_count = 1               # single node
    vm_size    = "Standard_B2s"  # cheapest viable size ~$30/month

    upgrade_settings {
      max_surge = "10%"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  # Disable expensive add-ons
  network_profile {
    network_plugin = "kubenet"  # cheaper than azure CNI
    load_balancer_sku = "standard" 
  }
}

resource "azurerm_kubernetes_cluster_extension" "app_configuration" {
  name           = "app-configuraton-extension"
  cluster_id     = azurerm_kubernetes_cluster.aks_cluster.id
  extension_type = "Microsoft.AppConfiguration"
}

resource "azurerm_kubernetes_cluster_node_pool" "spot_node_pool" {
  name                  = "spot"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.aks_cluster.id
  vm_size               = "Standard_D2s_v3"
  node_count            = 1

  priority        = "Spot"
  eviction_policy = "Delete"
  spot_max_price  = -1  # -1 means pay up to on-demand price
}

# Allow the nodes to pull container for registry:
resource "azurerm_role_assignment" "ArchPull" {
  principal_id                     = azurerm_kubernetes_cluster.aks_cluster.kubelet_identity[0].object_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.container_registry.id
  skip_service_principal_aad_check = true
}