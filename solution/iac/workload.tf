locals {
  servicebus_namespace = data.terraform_remote_state.workload.outputs.servicebus_namespace
  kubernetes_cluster   = data.terraform_remote_state.workload.outputs.kubernetes_cluster
}