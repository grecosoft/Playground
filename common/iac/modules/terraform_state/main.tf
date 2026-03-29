locals {
  solution_containers = [for env in var.environments : replace(lower("${var.solution_name}-${env}"), ".", "-")]
}

data "azurerm_resource_group" "terraform" {
  name = var.resource_group_name
}

data "azurerm_storage_account" "terraform" {
  name                = var.storage_account_name
  resource_group_name = data.azurerm_resource_group.terraform.name
}

resource "azurerm_storage_container" "solutions" {
  for_each              = toset(local.solution_containers)
  name                  = each.key
  storage_account_id    = data.azurerm_storage_account.terraform.id
  container_access_type = "private"
}