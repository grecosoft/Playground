data "azurerm_resource_group" "terraform" {
  name     = var.resource_group_name
}

data "azurerm_storage_account" "terraform" {
  name                = var.storage_account_name
  resource_group_name = data.azurerm_resource_group.terraform.name
}

resource "azurerm_storage_container" "microservice" {
  for_each = toset(var.environments)
  name                  = replace(lower("${var.solution_name}-${each.key}"), ".", "-")
  storage_account_name  = data.azurerm_storage_account.terraform.name
  container_access_type = "private"
}