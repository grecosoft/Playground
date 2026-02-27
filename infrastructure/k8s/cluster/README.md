terraform init -backend-config ./envs/dev-eastus/backend.tfvars
terraform apply -var-file ./envs/dev-eastus/configuration.tfvars