locals {
  service_name = "service-one"  
  reply_queue_name = "${local.service_name}-reply-queue"
}