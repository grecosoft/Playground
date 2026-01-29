data "azurerm_container_registry" "acr" {
  name                = var.container_registry_name
  resource_group_name = var.infrastructure_resource_group_name
}