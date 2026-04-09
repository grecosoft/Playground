terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.4"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "3.1.1"
    }
  }
  backend "azurerm" {
    key = "solution.tfstate"
  }
}

provider "azurerm" {
  subscription_id = var.subscription_id
  features {}
}

provider "helm" {
  kubernetes = {
    host                   = data.azurerm_kubernetes_cluster.k8s.kube_config.0.host

    client_certificate     = base64decode(data.azurerm_kubernetes_cluster.k8s.kube_config.0.client_certificate)
    client_key             = base64decode(data.azurerm_kubernetes_cluster.k8s.kube_config.0.client_key)
    cluster_ca_certificate = base64decode(data.azurerm_kubernetes_cluster.k8s.kube_config.0.cluster_ca_certificate)
  }
}