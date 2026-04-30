subscription_id = "c47473b5-b6e9-476b-853e-a1f5b826e95b"
solution_name   = "solution"
environment     = "dev"
location        = "eastus"


# Developer related variables:
developer_group_name = "solution-developers"

# Reference to the remote state of the solution to which the service will be deployed.
storage_resource_group_name = "terraform-state"
storage_account_name        = "terraformstatestorage07"
solution_container_name     = "playground-dev-eastus"

# Environment Overrides:
env_app_configs = []
serilog_log_level = "Information"
