variable "service_bus_name" {
  type = string
  default = null
}

locals {
  postfix = "${var.environment}-${var.location}-${local.resource_postfix}"
  service_bus_name = coalesce(var.service_bus_name, "sb-${local.postfix}")
}

resource "azurerm_servicebus_namespace" "service_bus" {
  name                = local.service_bus_name
  location            = local.location
  resource_group_name = azurerm_resource_group.solution.name
  sku                 = "Standard"
}