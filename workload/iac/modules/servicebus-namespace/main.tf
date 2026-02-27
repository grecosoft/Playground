resource "random_string" "unique_postfix" {
  length  = 7
  special = false
}

resource "azurerm_servicebus_namespace" "servicebus_namespace" {
  name                = "${var.name}-${random_string.unique_postfix.result}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "Standard"
}