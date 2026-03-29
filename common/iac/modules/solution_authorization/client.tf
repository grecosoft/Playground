resource "azuread_application_registration" "solution_client" {
  display_name                   = "${var.solution_env_name}-client"
  description                    = "Client application for accessing the solution's services."
  sign_in_audience               = "AzureADMyOrg"
  requested_access_token_version = 2
}

# Create service principal for client
resource "azuread_service_principal" "solution_client" {
  client_id = azuread_application_registration.solution_client.client_id
  owners    = [data.azuread_client_config.current.object_id]
}

resource "azuread_application_fallback_public_client" "solution_client" {
  application_id = azuread_application_registration.solution_client.id
  enabled        = true
}

# Configure as public client (native/mobile/desktop) with redirect URIs
resource "azuread_application_redirect_uris" "solution_client" {
  application_id = azuread_application_registration.solution_client.id
  type           = "PublicClient"

  redirect_uris = var.redirect_uris
}

resource "azuread_application_api_access" "client_api_access" {
  application_id = azuread_application_registration.solution_client.id
  api_client_id  = azuread_application_registration.solution_api.client_id

  scope_ids = [random_uuid.user_impersonation_scope_id.result]
}

resource "azuread_application_api_access" "solution_client_msgraph_access" {
  application_id = azuread_application_registration.solution_client.id
  api_client_id  = data.azuread_service_principal.msgraph.client_id

  scope_ids = [
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"]
  ]
}
