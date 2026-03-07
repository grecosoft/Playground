data "azuread_group" "solution_developers" {
  display_name     = var.developer_group_name
  security_enabled = true
}

