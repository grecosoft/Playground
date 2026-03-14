data "azuread_application_published_app_ids" "well_known" {}

data "azuread_service_principal" "msgraph" {
  client_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
}

data "azuread_user" "assigned_users" {
  for_each = toset(flatten(values(var.role_user_assignments)))
  
  user_principal_name = each.value
}