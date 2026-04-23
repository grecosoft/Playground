variable "signalr_name" {
  type = string
  default = null
}

locals {
  signalr_name = coalesce(var.signalr_name, "sr-${var.environment}-${var.location}-${local.resource_postfix}")
}

resource "azurerm_signalr_service" "signalr" {
  name                = local.signalr_name
  location            = var.location
  resource_group_name =  azurerm_resource_group.solution.name
  sku {
    name     = "Standard_S1"
    capacity = 1
  }

  public_network_access_enabled = true

  connectivity_logs_enabled = true
  messaging_logs_enabled    = true
  service_mode              = "Default"
}

resource "azurerm_role_assignment" "signalr_owner" {
  scope                = azurerm_signalr_service.signalr.id
  principal_type = "Group"
  role_definition_name = "SignalR Service Owner"
  principal_id         = data.azuread_group.solution_developers.object_id
}