# Defines the resource group containing the infrastructure resources for solutions
resource "azurerm_resource_group" "infrastructure" {
  name     = var.resource_group_name
  location = var.location
}

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