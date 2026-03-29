data "azuread_client_config" "current" {}

resource "azuread_application_registration" "solution_api" {
  display_name                       = "${var.solution_env_name}-api"
  description                        = "Application for the services belonging to the solution."
  sign_in_audience                   = "AzureADMyOrg"
  implicit_id_token_issuance_enabled = true
  requested_access_token_version     = 2
  group_membership_claims            = ["SecurityGroup"]
}

# Create service principal (Enterprise Application)
resource "azuread_service_principal" "solution_api" {
  client_id                    = azuread_application_registration.solution_api.client_id
  app_role_assignment_required = false
  owners                       = [data.azuread_client_config.current.object_id]
}

resource "azuread_application_owner" "solution_api" {
  application_id  = azuread_application_registration.solution_api.id
  owner_object_id = data.azuread_client_config.current.object_id
}

# Configure identifier URIs
resource "azuread_application_identifier_uri" "solution_api" {
  application_id = azuread_application_registration.solution_api.id
  identifier_uri = "api://${azuread_application_registration.solution_api.client_id}"
}

resource "random_uuid" "app_role_ids" {
  for_each = var.solution_roles
}

# Expose API - define user_impersonation scope
resource "random_uuid" "user_impersonation_scope_id" {}

resource "azuread_application_permission_scope" "user_impersonation_scope" {
  application_id = azuread_application_registration.solution_api.id
  scope_id       = random_uuid.user_impersonation_scope_id.result
  value          = "user_impersonation"
  type           = "User"

  admin_consent_display_name = "Access solution API"
  admin_consent_description  = "Allows the app to access the solution API on behalf of the signed-in user"
  user_consent_display_name  = "Access solution API"
  user_consent_description   = "Allows the app to access the solution API on your behalf"
}

# The following is creating an application roles.  Then roles can then 
# be added to EntraId groups/users.  * If using the free Azure subscription,
# only users can be added to roles and not groups. *.
resource "azuread_application_app_role" "solution_api_role" {
  for_each = var.solution_roles

  application_id = azuread_application_registration.solution_api.id
  role_id        = random_uuid.app_role_ids[each.key].result

  allowed_member_types = each.value.allowed_member_types
  description          = each.value.description
  display_name         = each.value.display_name
  value                = each.key
}

# Create a group for each role if enabled:
resource "azuread_group" "solution_api_role_group" {
  for_each = var.create_groups ? var.solution_roles : {}

  display_name            = each.value.display_name
  description             = each.value.description
  owners                  = [data.azuread_client_config.current.object_id]
  security_enabled        = true
  prevent_duplicate_names = true
}

# The following is saying that the the role is assigned to the group for the Enterprise application.
resource "azuread_app_role_assignment" "solution_api_group" {
  for_each = var.create_groups ? var.solution_roles : {}

  resource_object_id  = azuread_service_principal.solution_api.object_id                 # Enterprise Application
  app_role_id         = azuread_application_app_role.solution_api_role[each.key].role_id # Role
  principal_object_id = azuread_group.solution_api_role_group[each.key].object_id        # Group
}

resource "azuread_application_optional_claims" "solution_api_groups" {
  count          = var.create_groups ? 1 : 0
  application_id = azuread_application_registration.solution_api.id

  # For ID tokens
  id_token {
    name                  = "groups"
    additional_properties = ["emit_as_roles"]
  }

  # For Access tokens - CRITICAL for role values
  access_token {
    name                  = "groups"
    additional_properties = ["emit_as_roles"]
  }
}


locals {
  assignments = flatten([
    for role_key, users in var.role_user_assignments :
    [
      for user in users : {
        role_key = role_key
        user     = user
      }
    ]
  ])
}

resource "azuread_app_role_assignment" "user_assignments" {
  for_each = { for assignment in local.assignments : assignment.role_key => assignment.user }

  app_role_id         = random_uuid.app_role_ids[each.key].result
  principal_object_id = data.azuread_user.assigned_users[each.value].object_id
  resource_object_id  = azuread_service_principal.solution_api.object_id
}
