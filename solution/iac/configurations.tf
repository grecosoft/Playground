locals {
  common_configs = [
    {
      key = "Serilog:MinimumLevel:Default"
      value = var.Serilog_Log_Level
    },
    {
      key   = "Logging:SeqUrl"
      value = var.Logging_SEQ_URL
    }
  ]
}


module "solution_configs" {
  source               = "../../common/iac/modules/service_configuration"
  
  solution_configuration = local.solution_configuration
  app_configs          = local.common_configs
  label_name = "solution-configs"
}