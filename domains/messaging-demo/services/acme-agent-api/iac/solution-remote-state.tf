# Reference to state of the solution environment to which the
# service will be deployed:
data "terraform_remote_state" "solution" {
  backend = "azurerm"

  config = {
    resource_group_name  = var.storage_resource_group_name
    storage_account_name = var.storage_account_name
    container_name       = var.solution_container_name
    key                  = "solution.tfstate"
  }
}

locals {
    solution                      = data.terraform_remote_state.solution.outputs
    solution_developers_group_id = local.solution.solution_developers_group_id
    solution_location             = local.solution.solution_location
    solution_env_name             = local.solution.solution_env_name
  
}