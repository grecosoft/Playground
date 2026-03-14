resource "azuread_application_registration" "solution_client" {
  display_name     = "${var.solution_name}-client-${var.workload_config.workload_env_name}"
  description      = "Client application for accessing the solution's services."
  sign_in_audience = "AzureADMyOrg"
  requested_access_token_version = 2
}

# Create service principal for client
resource "azuread_service_principal" "solution_client" {
  client_id = azuread_application_registration.solution_client.client_id
  owners = [data.azuread_client_config.current.object_id]
}

resource "azuread_application_fallback_public_client" "solution_client" {
  application_id = azuread_application_registration.solution_client.id
  enabled        = true
}

# Configure as public client (native/mobile/desktop) with redirect URIs
resource "azuread_application_redirect_uris" "solution_client" {
  application_id = azuread_application_registration.solution_client.id
  type           = "PublicClient"

  redirect_uris = [
    "http://localhost",
    "http://localhost:3000",
    "http://localhost:8080",
    "https://login.microsoftonline.com/common/oauth2/nativeclient"
  ]
}





data "azuread_application_published_app_ids" "well_known" {}

locals {
  az_cli_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftAzureCli
  msgraph_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
}

data "azuread_service_principal" "msgraph" {
  client_id = local.msgraph_id
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

// TODO:
// - have only data in the outer most directory and pass into modules.... (see above)
// - Add property called create_groups.  If true, create the groups and assign the roles etc.
// - Off of each role allow a given user to be specified and add them to the role.  Or pass in separate lookup.
// - Add the CLI access permission...
// - Allow passing in redirect urls and add the MS automatically.