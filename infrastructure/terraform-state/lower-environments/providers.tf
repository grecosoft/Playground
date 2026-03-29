terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.4"
    }
  }
  backend "azurerm" {
    resource_group_name  = "terraform-state"
    storage_account_name = "terraformstatestorage07"
    container_name       = "infrastructure-lower-environments"
    key = "terraform.tfstate"
  }
}

provider "azurerm" {
  subscription_id = local.subscription_id
  features {}
}