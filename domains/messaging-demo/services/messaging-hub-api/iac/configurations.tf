locals {
  common_configs = [
     {
      key   = "update_sentinel"
      value = "0"
    },
    {
      key    = "SignalREndpoint"
      value = local.solution.signalr.endpoint
    }
  ]

  vault_secrets = []
}