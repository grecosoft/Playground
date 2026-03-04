subscription_id = "c47473b5-b6e9-476b-853e-a1f5b826e95b"
workload_name   = "Playground"
solution_name   = "solution"
environment     = "dev"
location        = "eastus"

# Kubernetes workload identity variables:
namespace       = "solution"

# A given solution extends the core workload resources. For example, a solution can define EventHubs
# specific to the services from which it is implemented, on the common EventHub Namespace defined by
# the workload.
storage_resource_group_name = "terraform-state"
storage_account_name        = "terraformstatestorage07"
workload_container_name     = "playground-dev"