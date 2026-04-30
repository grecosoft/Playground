terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.4"
    }
  }
  backend "azurerm" {
    key = "csharp-connector.tfstate"
  }
}

provider "azurerm" {
  subscription_id = var.subscription_id
  features {}
}