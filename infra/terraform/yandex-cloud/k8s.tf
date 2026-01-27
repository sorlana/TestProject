# Создание кластера Kubernetes
resource "yandex_kubernetes_cluster" "testproject_k8s" {
  name        = "${local.resource_prefix}-k8s-cluster"
  description = "Managed Kubernetes cluster for TestProject"
  network_id  = yandex_vpc_network.main.id
  
  cluster_ipv4_range = local.k8s_pod_cidr
  service_ipv4_range = local.k8s_service_cidr
  
  master {
    version   = var.k8s_version
    public_ip = true
    
    regional {
      region = "ru-central1"
      
      location {
        zone      = local.availability_zones[0]
        subnet_id = yandex_vpc_subnet.private[0].id
      }
      
      location {
        zone      = local.availability_zones[1]
        subnet_id = yandex_vpc_subnet.private[1].id
      }
      
      location {
        zone      = local.availability_zones[2]
        subnet_id = yandex_vpc_subnet.private[2].id
      }
    }
    
    security_group_ids = [
      yandex_vpc_security_group.k8s.id,
      yandex_vpc_security_group.internal.id
    ]
    
    maintenance_policy {
      auto_upgrade = true
      
      maintenance_window {
        start_time = "03:00"
        duration   = "3h"
      }
    }
  }
  
  service_account_id = yandex_iam_service_account.k8s_cluster_sa.id
  node_service_account_id = yandex_iam_service_account.k8s_cluster_sa.id
  
  kms_provider {
    key_id = yandex_kms_symmetric_key.k8s_secrets_key.id
  }
  
  release_channel = "REGULAR"
  
  network_policy_provider = "CALICO"
  
  labels = merge(local.common_tags, {
    "k8s.io/cluster-name" = "${local.resource_prefix}-k8s-cluster"
  })
  
  depends_on = [
    yandex_resourcemanager_folder_iam_member.k8s_cluster_roles,
    yandex_vpc_subnet.private
  ]
}

# Создание групп узлов Kubernetes
resource "yandex_kubernetes_node_group" "k8s_groups" {
  for_each = var.k8s_node_groups
  
  cluster_id = yandex_kubernetes_cluster.testproject_k8s.id
  name       = "${local.resource_prefix}-${each.key}-node-group"
  
  instance_template {
    platform_id = "standard-v3"
    
    resources {
      cores         = each.value.cpu
      memory        = each.value.memory
      core_fraction = 100
    }
    
    boot_disk {
      type = each.value.disk_type
      size = each.value.disk_size
    }
    
    network_interface {
      subnet_ids          = [for subnet in yandex_vpc_subnet.private : subnet.id]
      nat                 = true
      security_group_ids  = [
        yandex_vpc_security_group.k8s.id,
        yandex_vpc_security_group.internal.id
      ]
    }
    
    scheduling_policy {
      preemptible = each.value.preemptible
    }
    
    metadata = {
      ssh-keys = "ubuntu:${file("~/.ssh/id_rsa.pub")}"
      user-data = templatefile("${path.module}/templates/cloud-init.yaml", {
        node_labels = each.value.node_labels
        node_taints = each.value.node_taints
        docker_version = "24.0"
        containerd_version = "1.7"
      })
    }
    
    labels = merge(local.common_tags, each.value.node_labels, {
      "node-group" = each.key
    })
  }
  
  scale_policy {
    fixed_scale {
      size = each.value.node_count
    }
  }
  
  allocation_policy {
    location {
      zone = local.availability_zones[0]
    }
    
    location {
      zone = local.availability_zones[1]
    }
    
    location {
      zone = local.availability_zones[2]
    }
  }
  
  maintenance_policy {
    auto_upgrade = true
    auto_repair  = true
    
    maintenance_window {
      start_time = "02:00"
      duration   = "4h"
    }
  }
  
  deploy_policy {
    max_expansion   = 1
    max_unavailable = 0
  }
  
  labels = merge(local.common_tags, {
    "node-group" = each.key
  })
  
  depends_on = [
    yandex_kubernetes_cluster.testproject_k8s
  ]
}

# KMS ключ для шифрования Kubernetes секретов
resource "yandex_kms_symmetric_key" "k8s_secrets_key" {
  name              = "${local.resource_prefix}-k8s-secrets-key"
  description       = "KMS key for encrypting Kubernetes secrets"
  default_algorithm = "AES_256"
  rotation_period   = "8760h" # 1 год
  
  tags = local.common_tags
}

# Роль для шифрования секретов
resource "yandex_kms_symmetric_key_iam_binding" "k8s_secrets_key_binding" {
  symmetric_key_id = yandex_kms_symmetric_key.k8s_secrets_key.id
  role             = "kms.keys.encrypterDecrypter"
  
  members = [
    "serviceAccount:${yandex_iam_service_account.k8s_cluster_sa.id}",
  ]
}

# Установка ingress-nginx через Helm
resource "helm_release" "ingress_nginx" {
  name       = "ingress-nginx"
  repository = "https://kubernetes.github.io/ingress-nginx"
  chart      = "ingress-nginx"
  version    = "4.7.1"
  
  namespace        = "ingress-nginx"
  create_namespace = true
  
  values = [templatefile("${path.module}/templates/ingress-nginx-values.yaml", {
    load_balancer_ip = yandex_lb_network_load_balancer.ingress.listener[0].external_address_spec[0].address
    internal         = false
  })]
  
  set {
    name  = "controller.service.loadBalancerIP"
    value = yandex_lb_network_load_balancer.ingress.listener[0].external_address_spec[0].address
  }
  
  depends_on = [
    yandex_kubernetes_node_group.k8s_groups["system"]
  ]
}

# Установка cert-manager через Helm
resource "helm_release" "cert_manager" {
  name       = "cert-manager"
  repository = "https://charts.jetstack.io"
  chart      = "cert-manager"
  version    = "1.13.0"
  
  namespace        = "cert-manager"
  create_namespace = true
  
  set {
    name  = "installCRDs"
    value = "true"
  }
  
  set {
    name  = "prometheus.enabled"
    value = "true"
  }
  
  values = [file("${path.module}/templates/cert-manager-values.yaml")]
  
  depends_on = [
    helm_release.ingress_nginx
  ]
}

# Установка external-dns через Helm
resource "helm_release" "external_dns" {
  count = var.dns_zone_id != "" ? 1 : 0
  
  name       = "external-dns"
  repository = "https://kubernetes-sigs.github.io/external-dns"
  chart      = "external-dns"
  version    = "1.13.0"
  
  namespace        = "external-dns"
  create_namespace = true
  
  values = [templatefile("${path.module}/templates/external-dns-values.yaml", {
    dns_zone_id = var.dns_zone_id
    folder_id   = var.yc_folder_id
  })]
  
  depends_on = [
    helm_release.cert_manager
  ]
}

# Установка prometheus-stack через Helm
resource "helm_release" "prometheus_stack" {
  count = var.enable_monitoring ? 1 : 0
  
  name       = "prometheus-stack"
  repository = "https://prometheus-community.github.io/helm-charts"
  chart      = "kube-prometheus-stack"
  version    = "46.8.0"
  
  namespace        = "monitoring"
  create_namespace = true
  
  values = [file("${path.module}/templates/prometheus-stack-values.yaml")]
  
  set {
    name  = "grafana.adminPassword"
    value = random_password.grafana_password.result
  }
  
  depends_on = [
    yandex_kubernetes_node_group.k8s_groups["monitoring"]
  ]
}

# Создание namespace для приложения
resource "kubernetes_namespace" "application" {
  metadata {
    name = "testproject-namespace-${var.environment}"
    
    labels = {
      name        = "testproject-namespace-${var.environment}"
      environment = var.environment
      managed-by  = "terraform"
    }
    
    annotations = {
      "description" = "Namespace for TestProject application in ${var.environment} environment"
    }
  }
  
  depends_on = [
    yandex_kubernetes_cluster.testproject_k8s
  ]
}

# Создание service account для приложения
resource "kubernetes_service_account" "application" {
  metadata {
    name      = "testproject-service-account"
    namespace = kubernetes_namespace.application.metadata[0].name
    
    annotations = {
      "iam.gke.io/gcp-service-account" = yandex_iam_service_account.k8s_cluster_sa.email
    }
  }
  
  automount_service_account_token = true
  
  depends_on = [
    kubernetes_namespace.application
  ]
}

# Роли для service account приложения
resource "kubernetes_cluster_role" "application" {
  metadata {
    name = "testproject-cluster-role"
    
    labels = {
      app = "testproject"
    }
  }
  
  rule {
    api_groups = [""]
    resources  = ["pods", "services", "endpoints", "persistentvolumeclaims", "configmaps", "secrets"]
    verbs      = ["get", "list", "watch", "create", "update", "patch", "delete"]
  }
  
  rule {
    api_groups = ["apps"]
    resources  = ["deployments", "statefulsets", "replicasets"]
    verbs      = ["get", "list", "watch", "create", "update", "patch", "delete"]
  }
  
  rule {
    api_groups = ["autoscaling"]
    resources  = ["horizontalpodautoscalers"]
    verbs      = ["get", "list", "watch", "create", "update", "patch", "delete"]
  }
  
  rule {
    api_groups = ["networking.k8s.io"]
    resources  = ["ingresses"]
    verbs      = ["get", "list", "watch", "create", "update", "patch", "delete"]
  }
  
  depends_on = [
    yandex_kubernetes_cluster.testproject_k8s
  ]
}

# Привязка роли к service account
resource "kubernetes_cluster_role_binding" "application" {
  metadata {
    name = "testproject-cluster-role-binding"
  }
  
  role_ref {
    api_group = "rbac.authorization.k8s.io"
    kind      = "ClusterRole"
    name      = kubernetes_cluster_role.application.metadata[0].name
  }
  
  subject {
    kind      = "ServiceAccount"
    name      = kubernetes_service_account.application.metadata[0].name
    namespace = kubernetes_namespace.application.metadata[0].name
  }
  
  depends_on = [
    kubernetes_cluster_role.application,
    kubernetes_service_account.application
  ]
}

# Secret для доступа к Container Registry
resource "kubernetes_secret" "container_registry" {
  metadata {
    name      = "yandex-registry-secret"
    namespace = kubernetes_namespace.application.metadata[0].name
  }
  
  type = "kubernetes.io/dockerconfigjson"
  
  data = {
    ".dockerconfigjson" = jsonencode({
      auths = {
        "cr.yandex" = {
          username = "json_key"
          password = jsonencode({
            id                = yandex_iam_service_account_key.registry_key.id
            service_account_id = yandex_iam_service_account.terraform_sa.id
            private_key       = yandex_iam_service_account_key.registry_key.private_key
            created_at        = yandex_iam_service_account_key.registry_key.created_at
          })
          email = "no-reply@testproject.com"
        }
      }
    })
  }
  
  depends_on = [
    kubernetes_namespace.application
  ]
}

# Случайный пароль для Grafana
resource "random_password" "grafana_password" {
  length  = 32
  special = true
}

# Файл конфигурации для kubectl
resource "local_file" "kubeconfig" {
  filename = "${path.module}/kubeconfig.yaml"
  content = templatefile("${path.module}/templates/kubeconfig.tpl", {
    cluster_name     = yandex_kubernetes_cluster.testproject_k8s.name
    endpoint         = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint
    ca_certificate   = base64encode(yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate)
    service_account  = yandex_iam_service_account.k8s_cluster_sa.id
  })
  
  file_permission = "0600"
  
  depends_on = [
    yandex_kubernetes_cluster.testproject_k8s
  ]
}