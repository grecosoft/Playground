resource "azurerm_resource_group" "terraform" {
  name     = var.resource_group_name
  location = "eastus"
}

resource "azurerm_storage_account" "terraform" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.terraform.name
  location                 = azurerm_resource_group.terraform.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "microservice" {
  for_each = toset(var.microservice_names)
  name                  = lower(replace(each.key, ".", "-"))  
  storage_account_name  = azurerm_storage_account.terraform.name
  container_access_type = "private"
}