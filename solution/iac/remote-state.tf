data "terraform_remote_state" "workload" {
  backend = "azurerm"

  config = {
    resource_group_name  = var.storage_resource_group_name
    storage_account_name = var.storage_account_name
    container_name       = var.workload_container_name
    key                  = "terraform.tfstate"
  }
}
