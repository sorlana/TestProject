# Создание VPC
resource "yandex_vpc_network" "main" {
  name        = "${local.resource_prefix}-network"
  description = "Main VPC for TestProject infrastructure"
  
  labels = local.common_tags
}

# Создание публичных подсетей
resource "yandex_vpc_subnet" "public" {
  count = length(var.public_subnet_cidrs)
  
  name           = "${local.resource_prefix}-public-subnet-${count.index}"
  description    = "Public subnet in ${local.availability_zones[count.index]}"
  zone           = local.availability_zones[count.index]
  network_id     = yandex_vpc_network.main.id
  v4_cidr_blocks = [var.public_subnet_cidrs[count.index]]
  
  route_table_id = yandex_vpc_route_table.public.id
  
  labels = merge(local.common_tags, {
    "subnet-type" = "public"
    "zone"        = local.availability_zones[count.index]
  })
}

# Создание приватных подсетей
resource "yandex_vpc_subnet" "private" {
  count = length(var.private_subnet_cidrs)
  
  name           = "${local.resource_prefix}-private-subnet-${count.index}"
  description    = "Private subnet in ${local.availability_zones[count.index]}"
  zone           = local.availability_zones[count.index]
  network_id     = yandex_vpc_network.main.id
  v4_cidr_blocks = [var.private_subnet_cidrs[count.index]]
  
  labels = merge(local.common_tags, {
    "subnet-type" = "private"
    "zone"        = local.availability_zones[count.index]
  })
}

# Создание подсетей для базы данных
resource "yandex_vpc_subnet" "database" {
  count = length(var.database_subnet_cidrs)
  
  name           = "${local.resource_prefix}-database-subnet-${count.index}"
  description    = "Database subnet in ${local.availability_zones[count.index]}"
  zone           = local.availability_zones[count.index]
  network_id     = yandex_vpc_network.main.id
  v4_cidr_blocks = [var.database_subnet_cidrs[count.index]]
  
  labels = merge(local.common_tags, {
    "subnet-type" = "database"
    "zone"        = local.availability_zones[count.index]
  })
}

# Таблица маршрутизации для публичных подсетей
resource "yandex_vpc_route_table" "public" {
  name        = "${local.resource_prefix}-public-route-table"
  description = "Route table for public subnets"
  network_id  = yandex_vpc_network.main.id
  
  static_route {
    destination_prefix = "0.0.0.0/0"
    next_hop_address   = yandex_compute_instance.nat_gateway.network_interface[0].ip_address
  }
  
  labels = local.common_tags
  
  depends_on = [
    yandex_compute_instance.nat_gateway
  ]
}

# Создание NAT Gateway
resource "yandex_compute_instance" "nat_gateway" {
  name        = "${local.resource_prefix}-nat-gateway"
  description = "NAT Gateway for private subnets"
  platform_id = "standard-v3"
  zone        = local.availability_zones[0]
  
  resources {
    cores  = 2
    memory = 2
  }
  
  boot_disk {
    initialize_params {
      image_id = "fd80mrhj8fl2oe87o4e1" # Ubuntu 22.04
      size     = 20
      type     = "network-ssd"
    }
  }
  
  network_interface {
    subnet_id = yandex_vpc_subnet.public[0].id
    nat       = true
  }
  
  metadata = {
    user-data = templatefile("${path.module}/templates/nat-gateway-userdata.yaml", {})
  }
  
  scheduling_policy {
    preemptible = true
  }
  
  labels = merge(local.common_tags, {
    "role" = "nat-gateway"
  })
  
  depends_on = [
    yandex_vpc_subnet.public
  ]
}

# Security Group для Kubernetes
resource "yandex_vpc_security_group" "k8s" {
  name        = "${local.resource_prefix}-k8s-sg"
  description = "Security group for Kubernetes cluster"
  network_id  = yandex_vpc_network.main.id
  
  labels = merge(local.common_tags, {
    "service" = "kubernetes"
  })
  
  # Разрешаем все исходящие соединения
  egress {
    protocol       = "ANY"
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow all outgoing traffic"
  }
  
  # Разрешаем входящий SSH
  ingress {
    protocol       = "TCP"
    port           = 22
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow SSH from anywhere"
  }
  
  # Разрешаем входящий HTTPS
  ingress {
    protocol       = "TCP"
    port           = 443
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow HTTPS from anywhere"
  }
  
  # Разрешаем входящий HTTP
  ingress {
    protocol       = "TCP"
    port           = 80
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow HTTP from anywhere"
  }
  
  # Разрешаем Kubernetes API
  ingress {
    protocol       = "TCP"
    port           = 6443
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow Kubernetes API from anywhere"
  }
  
  # Разрешаем NodePort диапазон
  ingress {
    protocol       = "TCP"
    from_port      = 30000
    to_port        = 32767
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow NodePort range"
  }
  
  # Разрешаем ICMP для диагностики
  ingress {
    protocol       = "ICMP"
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow ICMP for diagnostics"
  }
}

# Security Group для PostgreSQL
resource "yandex_vpc_security_group" "postgres" {
  name        = "${local.resource_prefix}-postgres-sg"
  description = "Security group for PostgreSQL cluster"
  network_id  = yandex_vpc_network.main.id
  
  labels = merge(local.common_tags, {
    "service" = "postgresql"
  })
  
  # Разрешаем входящий PostgreSQL от Kubernetes
  ingress {
    protocol          = "TCP"
    port              = 6432
    security_group_id = yandex_vpc_security_group.k8s.id
    description       = "Allow PostgreSQL from Kubernetes"
  }
  
  # Разрешаем входящий PostgreSQL от внутренних сетей
  ingress {
    protocol       = "TCP"
    port           = 6432
    v4_cidr_blocks = concat(
      [for subnet in yandex_vpc_subnet.private : subnet.v4_cidr_blocks[0]],
      [for subnet in yandex_vpc_subnet.public : subnet.v4_cidr_blocks[0]]
    )
    description    = "Allow PostgreSQL from internal networks"
  }
  
  # Разрешаем все исходящие соединения
  egress {
    protocol       = "ANY"
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow all outgoing traffic"
  }
}

# Security Group для внутренней коммуникации
resource "yandex_vpc_security_group" "internal" {
  name        = "${local.resource_prefix}-internal-sg"
  description = "Security group for internal communication"
  network_id  = yandex_vpc_network.main.id
  
  labels = merge(local.common_tags, {
    "service" = "internal"
  })
  
  # Разрешаем весь трафик внутри security group
  ingress {
    protocol       = "ANY"
    v4_cidr_blocks = [
      yandex_vpc_network.main.id == yandex_vpc_network.main.id ? "10.0.0.0/8" : "0.0.0.0/0"
    ]
    description    = "Allow all traffic within VPC"
  }
  
  # Разрешаем все исходящие соединения
  egress {
    protocol       = "ANY"
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow all outgoing traffic"
  }
}

# Security Group для Load Balancer
resource "yandex_vpc_security_group" "load_balancer" {
  name        = "${local.resource_prefix}-lb-sg"
  description = "Security group for Load Balancer"
  network_id  = yandex_vpc_network.main.id
  
  labels = merge(local.common_tags, {
    "service" = "load-balancer"
  })
  
  # Разрешаем входящий HTTP/HTTPS
  ingress {
    protocol       = "TCP"
    port           = 80
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow HTTP from anywhere"
  }
  
  ingress {
    protocol       = "TCP"
    port           = 443
    v4_cidr_blocks = ["0.0.0.0/0"]
    description    = "Allow HTTPS from anywhere"
  }
  
  # Разрешаем исходящий трафик к Kubernetes
  egress {
    protocol          = "TCP"
    port              = 30080
    security_group_id = yandex_vpc_security_group.k8s.id
    description       = "Allow to Kubernetes NodePort"
  }
  
  egress {
    protocol          = "TCP"
    port              = 30443
    security_group_id = yandex_vpc_security_group.k8s.id
    description       = "Allow to Kubernetes NodePort (HTTPS)"
  }
}

# Создание Network Load Balancer
resource "yandex_lb_network_load_balancer" "ingress" {
  name        = "${local.resource_prefix}-ingress-lb"
  description = "Network Load Balancer for ingress traffic"
  
  labels = merge(local.common_tags, {
    "service" = "load-balancer"
  })
  
  listener {
    name = "http"
    port = 80
    
    external_address_spec {
      address = yandex_vpc_address.ingress_ip.external_ipv4_address[0].address
    }
  }
  
  listener {
    name = "https"
    port = 443
    
    external_address_spec {
      address = yandex_vpc_address.ingress_ip.external_ipv4_address[0].address
    }
  }
  
  attached_target_group {
    target_group_id = yandex_lb_target_group.ingress.id
    
    healthcheck {
      name = "http"
      
      http_options {
        port = 30080
        path = "/health"
      }
    }
  }
  
  depends_on = [
    yandex_vpc_address.ingress_ip
  ]
}

# Target Group для Load Balancer
resource "yandex_lb_target_group" "ingress" {
  name        = "${local.resource_prefix}-ingress-tg"
  description = "Target group for ingress traffic"
  
  labels = local.common_tags
  
  dynamic "target" {
    for_each = yandex_kubernetes_node_group.k8s_groups["system"].instance_ids
    
    content {
      subnet_id  = yandex_vpc_subnet.private[0].id
      address    = yandex_compute_instance.kubernetes_nodes[target.key].network_interface[0].ip_address
    }
  }
  
  depends_on = [
    yandex_kubernetes_node_group.k8s_groups
  ]
}

# Выделение статического IP адреса
resource "yandex_vpc_address" "ingress_ip" {
  name = "${local.resource_prefix}-ingress-ip"
  
  external_ipv4_address {
    zone_id = local.availability_zones[0]
  }
  
  labels = local.common_tags
}

# DNS зона (если не указана внешняя)
resource "yandex_dns_zone" "main" {
  count = var.dns_zone_id == "" ? 1 : 0
  
  name        = "${local.resource_prefix}-zone"
  description = "DNS zone for TestProject"
  
  zone             = "testproject.com."
  public           = true
  private_networks = [yandex_vpc_network.main.id]
  
  labels = local.common_tags
}

# DNS записи
resource "yandex_dns_recordset" "records" {
  for_each = {
    for record in var.dns_records : record.name => record
  }
  
  zone_id = var.dns_zone_id != "" ? var.dns_zone_id : yandex_dns_zone.main[0].id
  name    = "${each.value.name}.${var.dns_zone_id != "" ? "" : "testproject.com."}"
  type    = each.value.type
  ttl     = each.value.ttl
  
  dynamic "data" {
    for_each = each.value.data != [] ? each.value.data : [yandex_vpc_address.ingress_ip.external_ipv4_address[0].address]
    content {
      data = data.value
    }
  }
  
  depends_on = [
    yandex_vpc_address.ingress_ip
  ]
}

# Container Registry
resource "yandex_container_registry" "main" {
  name = "${local.resource_prefix}-registry"
  
  labels = merge(local.common_tags, {
    "service" = "container-registry"
  })
}

# Ключ доступа к Container Registry
resource "yandex_iam_service_account_key" "registry_key" {
  service_account_id = yandex_iam_service_account.terraform_sa.id
  description        = "Key for Container Registry access"
}

# Сертификат SSL
resource "yandex_cm_certificate" "ssl_cert" {
  name        = "${local.resource_prefix}-ssl-cert"
  description = "SSL certificate for TestProject domains"
  
  domains = var.certificate_domains
  
  labels = local.common_tags
}

# Challenge для Let's Encrypt
resource "yandex_cm_certificate_content" "ssl_cert_content" {
  certificate_id = yandex_cm_certificate.ssl_cert.id
  cert           = file("${path.module}/certs/certificate.pem")
  key            = file("${path.module}/certs/private-key.pem")
  chain          = file("${path.module}/certs/ca.pem")
}

# Dashboard для мониторинга
resource "yandex_monitoring_dashboard" "main" {
  count = var.enable_monitoring ? 1 : 0
  
  dashboard_json = templatefile("${path.module}/templates/dashboard.json", {
    cluster_id    = yandex_kubernetes_cluster.testproject_k8s.id
    postgres_id   = yandex_mdb_postgresql_cluster.main.id
    load_balancer_id = yandex_lb_network_load_balancer.ingress.id
    title         = "TestProject - ${local.environment}"
  })
  
  depends_on = [
    yandex_kubernetes_cluster.testproject_k8s,
    yandex_mdb_postgresql_cluster.main
  ]
}

# Каналы оповещений
resource "yandex_monitoring_notification_channel" "email" {
  count = length(var.alert_contacts.email) > 0 ? 1 : 0
  
  name = "${local.resource_prefix}-email-alerts"
  
  labels = {
    email = join(",", var.alert_contacts.email)
  }
}

resource "yandex_monitoring_notification_channel" "slack" {
  count = length(var.alert_contacts.slack) > 0 ? 1 : 0
  
  name = "${local.resource_prefix}-slack-alerts"
  
  labels = {
    url = var.alert_contacts.slack[0]
  }
}

# Compute instances для Kubernetes узлов (для target group)
resource "yandex_compute_instance" "kubernetes_nodes" {
  for_each = toset(flatten([
    for group_name, group in yandex_kubernetes_node_group.k8s_groups : [
      for i in range(group.node_count) : "${group_name}-${i}"
    ]
  ]))
  
  name        = "${local.resource_prefix}-${each.key}"
  description = "Kubernetes node instance"
  platform_id = "standard-v3"
  zone        = local.availability_zones[0]
  
  resources {
    cores  = 2
    memory = 4
  }
  
  boot_disk {
    initialize_params {
      image_id = "fd8ivs792dv2svh2mue0" # Ubuntu 22.04 with Docker
      size     = 50
      type     = "network-ssd"
    }
  }
  
  network_interface {
    subnet_id = yandex_vpc_subnet.private[0].id
    nat       = false
  }
  
  scheduling_policy {
    preemptible = true
  }
  
  metadata = {
    ssh-keys = "ubuntu:${file("~/.ssh/id_rsa.pub")}"
  }
  
  labels = merge(local.common_tags, {
    "kubernetes-node" = "true"
  })
  
  lifecycle {
    ignore_changes = [
      boot_disk[0].initialize_params[0].image_id
    ]
  }
}

# Бюджет и алерты стоимости
resource "yandex_billing_cloud_binding" "budget" {
  billing_account_id = var.yc_billing_account_id
  cloud_id           = var.yc_cloud_id
}

resource "yandex_billing_budget" "monthly" {
  name             = "${local.resource_prefix}-monthly-budget"
  billing_account_id = var.yc_billing_account_id
  amount {
    units = var.budget_limit
    currency_code = "RUB"
  }
  
  filter {
    cloud_ids = [var.yc_cloud_id]
  }
  
  threshold_rules {
    threshold_percent = 50
  }
  
  threshold_rules {
    threshold_percent = 80
  }
  
  threshold_rules {
    threshold_percent = 100
  }
  
  notification_channels = [
    for channel in yandex_monitoring_notification_channel.email : channel.id
  ]
}