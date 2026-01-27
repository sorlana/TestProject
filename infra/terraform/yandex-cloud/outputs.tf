# Output для сети
output "vpc_id" {
  description = "ID of the created VPC"
  value       = yandex_vpc_network.main.id
}

output "public_subnet_ids" {
  description = "IDs of public subnets"
  value       = [for subnet in yandex_vpc_subnet.public : subnet.id]
}

output "private_subnet_ids" {
  description = "IDs of private subnets"
  value       = [for subnet in yandex_vpc_subnet.private : subnet.id]
}

output "database_subnet_ids" {
  description = "IDs of database subnets"
  value       = [for subnet in yandex_vpc_subnet.database : subnet.id]
}

output "security_group_ids" {
  description = "IDs of security groups"
  value = {
    k8s     = yandex_vpc_security_group.k8s.id
    postgres = yandex_vpc_security_group.postgres.id
    internal = yandex_vpc_security_group.internal.id
    load_balancer = yandex_vpc_security_group.load_balancer.id
  }
}

# Output для Kubernetes
output "k8s_cluster_id" {
  description = "ID of the Kubernetes cluster"
  value       = yandex_kubernetes_cluster.testproject_k8s.id
}

output "k8s_cluster_name" {
  description = "Name of the Kubernetes cluster"
  value       = yandex_kubernetes_cluster.testproject_k8s.name
}

output "k8s_external_endpoint" {
  description = "External endpoint of Kubernetes cluster"
  value       = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint
}

output "k8s_internal_endpoint" {
  description = "Internal endpoint of Kubernetes cluster"
  value       = yandex_kubernetes_cluster.testproject_k8s.master[0].internal_v4_endpoint
}

output "k8s_cluster_ca_certificate" {
  description = "CA certificate of Kubernetes cluster"
  value       = yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate
  sensitive   = true
}

output "k8s_node_groups" {
  description = "Information about Kubernetes node groups"
  value = {
    for name, group in yandex_kubernetes_node_group.k8s_groups : name => {
      id           = group.id
      name         = group.name
      node_count   = group.node_count
      instance_ids = group.instance_ids
      status       = group.status
    }
  }
}

output "k8s_service_account_id" {
  description = "ID of the Kubernetes service account"
  value       = yandex_iam_service_account.k8s_cluster_sa.id
}

# Output для PostgreSQL
output "postgres_cluster_id" {
  description = "ID of the managed PostgreSQL cluster"
  value       = yandex_mdb_postgresql_cluster.main.id
}

output "postgres_cluster_name" {
  description = "Name of the managed PostgreSQL cluster"
  value       = yandex_mdb_postgresql_cluster.main.name
}

output "postgres_host_fqdns" {
  description = "FQDNs of PostgreSQL hosts"
  value       = yandex_mdb_postgresql_cluster.main.host[*].fqdn
}

output "postgres_host_ips" {
  description = "IP addresses of PostgreSQL hosts"
  value       = yandex_mdb_postgresql_cluster.main.host[*].assign_public_ip ? yandex_mdb_postgresql_cluster.main.host[*].public_ip : yandex_mdb_postgresql_cluster.main.host[*].private_ip
  sensitive   = true
}

output "postgres_users" {
  description = "PostgreSQL users with passwords"
  value = {
    for user in yandex_mdb_postgresql_user.users : user.name => {
      name     = user.name
      password = user.password
      conn_limit = user.conn_limit
      grants   = user.grants
    }
  }
  sensitive = true
}

output "postgres_databases" {
  description = "PostgreSQL databases"
  value       = yandex_mdb_postgresql_database.databases[*].name
}

output "postgres_connection_strings" {
  description = "Connection strings for PostgreSQL"
  value = {
    admin = "postgresql://${yandex_mdb_postgresql_user.users[0].name}:${yandex_mdb_postgresql_user.users[0].password}@${yandex_mdb_postgresql_cluster.main.host[0].fqdn}:6432/${yandex_mdb_postgresql_database.databases[0].name}?sslmode=verify-full"
    app   = "postgresql://${yandex_mdb_postgresql_user.users[1].name}:${yandex_mdb_postgresql_user.users[1].password}@${yandex_mdb_postgresql_cluster.main.host[0].fqdn}:6432/${yandex_mdb_postgresql_database.databases[0].name}?sslmode=verify-full"
  }
  sensitive = true
}

# Output для Container Registry
output "container_registry_id" {
  description = "ID of the Container Registry"
  value       = yandex_container_registry.main.id
}

output "container_registry_name" {
  description = "Name of the Container Registry"
  value       = yandex_container_registry.main.name
}

output "container_registry_url" {
  description = "URL of the Container Registry"
  value       = "cr.yandex/${yandex_container_registry.main.id}"
}

# Output для Load Balancer
output "load_balancer_id" {
  description = "ID of the Network Load Balancer"
  value       = yandex_lb_network_load_balancer.ingress.id
}

output "load_balancer_name" {
  description = "Name of the Network Load Balancer"
  value       = yandex_lb_network_load_balancer.ingress.name
}

output "load_balancer_external_ip" {
  description = "External IP address of the Load Balancer"
  value       = yandex_lb_network_load_balancer.ingress.listener[*].external_address_spec[*].address
}

# Output для сертификатов
output "certificate_id" {
  description = "ID of the managed SSL certificate"
  value       = yandex_cm_certificate.ssl_cert.id
}

output "certificate_domains" {
  description = "Domains covered by the certificate"
  value       = yandex_cm_certificate.ssl_cert.domains
}

output "certificate_status" {
  description = "Status of the certificate"
  value       = yandex_cm_certificate.ssl_cert.status
}

output "certificate_issuer" {
  description = "Certificate issuer"
  value       = yandex_cm_certificate.ssl_cert.issuer
}

output "certificate_expires_at" {
  description = "Certificate expiration date"
  value       = yandex_cm_certificate.ssl_cert.expired_at
}

# Output для DNS
output "dns_zone_id" {
  description = "ID of the DNS zone"
  value       = var.dns_zone_id != "" ? var.dns_zone_id : yandex_dns_zone.main[0].id
}

output "dns_records" {
  description = "Created DNS records"
  value = {
    for record in yandex_dns_recordset.records : record.name => {
      type = record.type
      ttl  = record.ttl
      data = record.data
    }
  }
}

# Output для мониторинга
output "dashboard_id" {
  description = "ID of the monitoring dashboard"
  value       = yandex_monitoring_dashboard.main.id
}

output "alerting_channels" {
  description = "Configured alerting channels"
  value = {
    email = yandex_monitoring_notification_channel.email[*].labels.email
    slack = length(yandex_monitoring_notification_channel.slack) > 0 ? yandex_monitoring_notification_channel.slack[0].labels.url : []
  }
}

# Output для сервисных аккаунтов
output "service_accounts" {
  description = "Created service accounts"
  value = {
    terraform = {
      id           = yandex_iam_service_account.terraform_sa.id
      name         = yandex_iam_service_account.terraform_sa.name
      access_key   = yandex_iam_service_account_static_access_key.terraform_sa_key.access_key
      secret_key   = yandex_iam_service_account_static_access_key.terraform_sa_key.secret_key
      key_id       = yandex_iam_service_account_key.terraform_sa_key.id
    }
    k8s = {
      id   = yandex_iam_service_account.k8s_cluster_sa.id
      name = yandex_iam_service_account.k8s_cluster_sa.name
    }
    postgres = {
      id   = yandex_iam_service_account.postgres_sa.id
      name = yandex_iam_service_account.postgres_sa.name
    }
  }
  sensitive = true
}

# Output для KMS
output "kms_key_id" {
  description = "ID of the KMS key for Terraform state"
  value       = yandex_kms_symmetric_key.terraform_state_key.id
}

output "kms_key_arn" {
  description = "ARN of the KMS key"
  value       = yandex_kms_symmetric_key.terraform_state_key.id
}

# Output для хранилища
output "storage_bucket_name" {
  description = "Name of the storage bucket for Terraform state"
  value       = yandex_storage_bucket.terraform_state.bucket
}

# Output для стоимости
output "monthly_cost_estimate" {
  description = "Estimated monthly cost of infrastructure"
  value = {
    kubernetes = sum([
      for group in yandex_kubernetes_node_group.k8s_groups : 
      group.node_count * (
        # CPU cost: ~₽1.92 per vCPU/hour
        var.k8s_node_groups[group.name].cpu * 1.92 * 24 * 30 +
        # Memory cost: ~₽0.51 per GB/hour
        var.k8s_node_groups[group.name].memory * 0.51 * 24 * 30 +
        # Disk cost: ~₽4.20 per GB/month
        var.k8s_node_groups[group.name].disk_size * 4.20
      )
    ])
    
    postgresql = (
      # Disk cost
      var.postgres_resources.disk_size * 4.20 +
      # Instance cost based on resource preset
      (var.postgres_resources.resource_preset_id == "s2.micro" ? 1500 : 
       var.postgres_resources.resource_preset_id == "s2.small" ? 3000 :
       var.postgres_resources.resource_preset_id == "s2.medium" ? 6000 : 12000)
    ) * (var.postgres_high_availability.enabled ? (1 + var.postgres_high_availability.replicas) : 1)
    
    load_balancer = 300 # Fixed monthly cost
    
    container_registry = 100 # Fixed monthly cost
    
    total = (
      sum([
        for group in yandex_kubernetes_node_group.k8s_groups : 
        group.node_count * (
          var.k8s_node_groups[group.name].cpu * 1.92 * 24 * 30 +
          var.k8s_node_groups[group.name].memory * 0.51 * 24 * 30 +
          var.k8s_node_groups[group.name].disk_size * 4.20
        )
      ]) +
      (var.postgres_resources.disk_size * 4.20 +
        (var.postgres_resources.resource_preset_id == "s2.micro" ? 1500 : 
         var.postgres_resources.resource_preset_id == "s2.small" ? 3000 :
         var.postgres_resources.resource_preset_id == "s2.medium" ? 6000 : 12000)
      ) * (var.postgres_high_availability.enabled ? (1 + var.postgres_high_availability.replicas) : 1) +
      300 + 100
    )
  }
}

# Output для kubeconfig
output "kubeconfig" {
  description = "Kubeconfig file for cluster access"
  value = templatefile("${path.module}/templates/kubeconfig.tpl", {
    cluster_name     = yandex_kubernetes_cluster.testproject_k8s.name
    endpoint         = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint
    ca_certificate   = base64encode(yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate)
    service_account  = yandex_iam_service_account.k8s_cluster_sa.id
  })
  sensitive = true
}

# Output для подключения
output "connection_instructions" {
  description = "Instructions for connecting to the infrastructure"
  value = <<-EOT

  🚀 Infrastructure deployed successfully!

  📊 Summary:
  - Kubernetes Cluster: ${yandex_kubernetes_cluster.testproject_k8s.name}
  - PostgreSQL Cluster: ${yandex_mdb_postgresql_cluster.main.name}
  - Load Balancer IP: ${join(", ", yandex_lb_network_load_balancer.ingress.listener[*].external_address_spec[*].address)}
  - Container Registry: cr.yandex/${yandex_container_registry.main.id}

  🔗 Endpoints:
  - Kubernetes API: ${yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint}
  - Application: https://${length(yandex_dns_recordset.records) > 0 ? yandex_dns_recordset.records[0].name : "N/A"}
  - API: https://api.${length(yandex_dns_recordset.records) > 0 ? yandex_dns_recordset.records[0].name : "N/A"}

  🔐 Access:
  1. Configure kubectl:
     $ export KUBECONFIG=kubeconfig.yaml
     $ echo '${base64encode(templatefile("${path.module}/templates/kubeconfig.tpl", {
        cluster_name     = yandex_kubernetes_cluster.testproject_k8s.name,
        endpoint         = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint,
        ca_certificate   = base64encode(yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate),
        service_account  = yandex_iam_service_account.k8s_cluster_sa.id
      }))}' | base64 --decode > kubeconfig.yaml

  2. Verify cluster access:
     $ kubectl cluster-info
     $ kubectl get nodes

  📈 Monitoring:
  - Dashboard: https://console.cloud.yandex.ru/folders/${var.yc_folder_id}/monitoring/dashboard/${yandex_monitoring_dashboard.main.id}
  - PostgreSQL: https://console.cloud.yandex.ru/folders/${var.yc_folder_id}/managed-postgresql/cluster/${yandex_mdb_postgresql_cluster.main.id}
  - Kubernetes: https://console.cloud.yandex.ru/folders/${var.yc_folder_id}/managed-kubernetes/cluster/${yandex_kubernetes_cluster.testproject_k8s.id}

  💾 Database Connection:
  - Host: ${yandex_mdb_postgresql_cluster.main.host[0].fqdn}
  - Port: 6432
  - Database: ${yandex_mdb_postgresql_database.databases[0].name}
  - SSL: Required (verify-full)

  ⚠️  Important:
  - Store the PostgreSQL passwords securely (output marked as sensitive)
  - Regularly backup the Terraform state
  - Monitor costs in Yandex Cloud Billing

  EOT
}

# Output для CI/CD
output "ci_cd_variables" {
  description = "Variables for CI/CD configuration"
  value = {
    YC_FOLDER_ID = var.yc_folder_id
    YC_CLOUD_ID  = var.yc_cloud_id
    K8S_CLUSTER_ID = yandex_kubernetes_cluster.testproject_k8s.id
    POSTGRES_CLUSTER_ID = yandex_mdb_postgresql_cluster.main.id
    CONTAINER_REGISTRY_ID = yandex_container_registry.main.id
    LOAD_BALANCER_IP = join(", ", yandex_lb_network_load_balancer.ingress.listener[*].external_address_spec[*].address)
  }
  sensitive = true
}