terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=4.1.0"
    }
  }
  backend "azurerm" {
    key = "aks.tfstate"
  }
}

provider "azurerm" {
  subscription_id = var.subscription_id
  features {}
}