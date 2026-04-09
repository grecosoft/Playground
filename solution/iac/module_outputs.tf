# This file defines the outputs for the infrastructure as code (IaC) modules. 
# It includes a local variable that aggregates the outputs from modules into 
# a single object for easier access and management.
locals {
    solution_configuration = {
        key_vault_name = module.configuration.key_vault_name
        key_vault_id = module.configuration.key_vault_id
        key_vault_uri = module.configuration.key_vault_uri
        app_config_name = module.configuration.app_config_name
        app_config_id = module.configuration.app_config_id
    }
}
