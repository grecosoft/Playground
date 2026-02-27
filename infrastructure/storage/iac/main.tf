data "azurerm_resource_group" "terraform" {
  name = var.resource_group_name
}

data "azurerm_storage_account" "terraform" {
  name                = var.storage_account_name
  resource_group_name = data.azurerm_resource_group.terraform.name
}

# This container is used to store common infrastructure state.  
resource "azurerm_storage_container" "infrastructure" {
  for_each              = toset(var.environments)
  name                  = "infrastructure-${each.key}"  
  storage_account_id    = data.azurerm_storage_account.terraform.id
  container_access_type = "private"
}

resource "azurerm_storage_container" "solution" {
  for_each              = toset(var.environments)
  name                  = replace(lower("${var.workload_name}-${each.key}"), ".", "-")
  storage_account_id    = data.azurerm_storage_account.terraform.id
  container_access_type = "private"
}