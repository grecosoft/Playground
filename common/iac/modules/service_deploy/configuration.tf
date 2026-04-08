output "deploy_variables" {
  description = "Workload configurations used when deploying the service."
  value = [
    { key = "WORKLOAD_CLIENT_ID", "value" = var.workload_client_id, label = "${var.service_name}-deploy" },
    { key = "WORKLOAD_TENANT_ID", "value" = var.workload_tenant_id, label = "${var.service_name}-deploy" }
  ]
}