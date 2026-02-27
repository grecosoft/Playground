module service_one {
  source = "./services/service-one"
  
  servicebus_namespace_id = local.servicebus_namespace.id
}