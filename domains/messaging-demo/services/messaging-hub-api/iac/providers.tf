terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.4"
    }
  }
  backend "azurerm" {
    key = "messaging-hub-api.tfstate"
  }
}

provider "azurerm" {
  subscription_id = var.subscription_id
  features {}
}