
# Enable receiving message from other services in the solution over Azure Service Bus queues. 
module "service_one_messaging" {
  source = "../../modules/service_messaging"
  service_name = "service-two"
  servicebus_namespace_id = var.servicebus_namespace_id
  solution_identity_principal_id = var.solution_identity_principal_id
  solution_developers_group_id = var.developers_principal_id
}