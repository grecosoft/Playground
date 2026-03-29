locals {
  subscription_id = "c47473b5-b6e9-476b-853e-a1f5b826e95b"
}

module "terraform_state" {
  source = "../../../common/iac/modules/terraform_state"  
  subscription_id      = local.subscription_id
  resource_group_name  = "terraform-state"
  storage_account_name = "terraformstatestorage07"
  solution_name        = "playground"
  environments         = ["dev-eastus", "stg-eastus"]
}

