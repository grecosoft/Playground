data "azurerm_resource_group" "terraform" {
  name = var.resource_group_name
}

data "azurerm_storage_account" "terraform" {
  name                = var.storage_account_name
  resource_group_name = data.azurerm_resource_group.terraform.name
}

resource "azurerm_storage_container" "infrastructure-common" {
  name                  = "infrastructure-common"  # TODO: Rename to infrastructure-common
  storage_account_id    = data.azurerm_storage_account.terraform.id
  container_access_type = "private"
}

# resource "azurerm_storage_container" "infrastructure-aks" {
#   name                  = "infrastructure-aks"
#   storage_account_id    = data.azurerm_storage_account.terraform.id
#   container_access_type = "private"
# }

resource "azurerm_storage_container" "microservice" {
  for_each              = toset(var.environments)
  name                  = replace(lower("${var.solution_name}-${each.key}"), ".", "-")
  storage_account_id    = data.azurerm_storage_account.terraform.id
  container_access_type = "private"
}