resource "random_password" "seq_password" {
  length           = 16
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?" # Custom special characters
}

resource "helm_release" "seq" {
  namespace        = local.solution_env_name
  create_namespace = false
  name             = "datalust"
  repository       = "https://helm.datalust.co"
  chart            = "seq"
  set = [ {
    name  = "firstRunAdminPassword"
    value = random_password.seq_password.result
  } ]
}

resource "azurerm_key_vault_secret" "seq_password" {
  name         = "secret-sauce"
  value        = random_password.seq_password.result
  key_vault_id = module.configuration.key_vault_id
}


