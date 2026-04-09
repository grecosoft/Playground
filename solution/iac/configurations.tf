locals {
  common_configs = [
    {
      key   = "Logging:SeqUrl"
      value = var.Logging_SEQ_URL
      label = "solution-configs"
    }
  ]
}


module "solution_configs" {
  source               = "../../common/iac/modules/service_configuration"
  
  solution_configuration = local.solution_configuration
  app_configs          = local.common_configs
}