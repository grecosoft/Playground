# Storage

This Terraform configuration is responsible for creating the storage account containers used to store Terraform state.
This configuration assumes the storage account used to store this configuration's state already exists and is created outside
of Terraform.


terraform init -backend-config ./backend-config.tfvars
terraform apply

