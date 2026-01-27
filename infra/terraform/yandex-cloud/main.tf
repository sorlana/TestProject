terraform {
  required_version = ">= 1.5.0"
  
  required_providers {
    yandex = {
      source  = "yandex-cloud/yandex"
      version = "~> 0.100.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.11.0"
    }
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5.0"
    }
  }

  backend "s3" {
    endpoint   = "storage.yandexcloud.net"
    bucket     = "testproject-tfstate"
    region     = "ru-central1"
    key        = "terraform/prod/terraform.tfstate"
    access_key = "${var.YC_STORAGE_ACCESS_KEY}"
    secret_key = "${var.YC_STORAGE_SECRET_KEY}"
    
    skip_region_validation      = true
    skip_credentials_validation = true
  }
}

provider "yandex" {
  cloud_id  = var.yc_cloud_id
  folder_id = var.yc_folder_id
  zone      = var.yc_zone
  
  service_account_key_file = var.yc_service_account_key_file
  token                    = var.yc_token
}

provider "kubernetes" {
  host                   = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint
  cluster_ca_certificate = yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate
  token                  = data.yandex_client_config.client.iam_token
  
  dynamic "exec" {
    for_each = var.yc_token != "" ? [] : [1]
    content {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = "yc"
      args = [
        "k8s",
        "create-token"
      ]
    }
  }
}

provider "helm" {
  kubernetes {
    host                   = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint
    cluster_ca_certificate = yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate
    token                  = data.yandex_client_config.client.iam_token
    
    exec {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = "yc"
      args = [
        "k8s",
        "create-token"
      ]
    }
  }
}

provider "kubectl" {
  host                   = yandex_kubernetes_cluster.testproject_k8s.master[0].external_v4_endpoint
  cluster_ca_certificate = yandex_kubernetes_cluster.testproject_k8s.master[0].cluster_ca_certificate
  token                  = data.yandex_client_config.client.iam_token
  load_config_file       = false
  
  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "yc"
    args = [
      "k8s",
      "create-token"
    ]
  }
}

# Локальные переменные для именования ресурсов
locals {
  project_name      = "testproject"
  environment       = var.environment
  resource_prefix   = "${local.project_name}-${local.environment}"
  
  # Регион и зоны
  availability_zones = ["ru-central1-a", "ru-central1-b", "ru-central1-c"]
  
  # Теги для всех ресурсов
  common_tags = {
    Project     = local.project_name
    Environment = local.environment
    ManagedBy   = "Terraform"
    Team        = "Platform"
    CostCenter  = "Engineering"
  }
  
  # Список CIDR для VPC
  vpc_cidr_blocks = {
    "ru-central1-a" = "10.10.0.0/16"
    "ru-central1-b" = "10.20.0.0/16"
    "ru-central1-c" = "10.30.0.0/16"
  }
  
  # CIDR для Kubernetes PODs и Services
  k8s_pod_cidr     = "10.96.0.0/16"
  k8s_service_cidr = "10.112.0.0/16"
  
  # Настройки для managed PostgreSQL
  postgres_settings = {
    max_connections                   = 100
    shared_buffers                    = "256MB"
    effective_cache_size              = "768MB"
    maintenance_work_mem              = "64MB"
    checkpoint_completion_target      = 0.9
    wal_buffers                       = "16MB"
    default_statistics_target         = 100
    random_page_cost                  = 1.1
    effective_io_concurrency          = 200
    work_mem                          = "4MB"
    min_wal_size                      = "80MB"
    max_wal_size                      = "1GB"
    max_worker_processes              = 8
    max_parallel_workers_per_gather   = 2
    max_parallel_workers              = 8
    max_parallel_maintenance_workers  = 2
  }
}

# Получение информации о текущем клиенте
data "yandex_client_config" "client" {}

# Создание сервисного аккаунта для управления инфраструктурой
resource "yandex_iam_service_account" "terraform_sa" {
  name        = "${local.resource_prefix}-terraform-sa"
  description = "Service account for Terraform to manage infrastructure"
  folder_id   = var.yc_folder_id
}

# Назначение ролей сервисному аккаунту
resource "yandex_resourcemanager_folder_iam_member" "terraform_roles" {
  for_each = toset([
    "editor",
    "k8s.admin",
    "vpc.user",
    "compute.admin",
    "load-balancer.admin",
    "certificate-manager.admin",
    "dns.editor",
    "storage.admin",
    "managed-postgresql.admin"
  ])
  
  folder_id = var.yc_folder_id
  role      = each.key
  member    = "serviceAccount:${yandex_iam_service_account.terraform_sa.id}"
}

# Создание статического ключа доступа для сервисного аккаунта
resource "yandex_iam_service_account_static_access_key" "terraform_sa_key" {
  service_account_id = yandex_iam_service_account.terraform_sa.id
  description        = "Static access key for Terraform SA"
}

# Создание бакета для хранения terraform state
resource "yandex_storage_bucket" "terraform_state" {
  bucket     = "${local.resource_prefix}-tfstate"
  access_key = yandex_iam_service_account_static_access_key.terraform_sa_key.access_key
  secret_key = yandex_iam_service_account_static_access_key.terraform_sa_key.secret_key
  
  versioning {
    enabled = true
  }
  
  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        kms_master_key_id = yandex_kms_symmetric_key.terraform_state_key.id
        sse_algorithm     = "aws:kms"
      }
    }
  }
  
  tags = local.common_tags
}

# Создание KMS ключа для шифрования state файла
resource "yandex_kms_symmetric_key" "terraform_state_key" {
  name              = "${local.resource_prefix}-tfstate-key"
  description       = "KMS key for encrypting Terraform state"
  default_algorithm = "AES_256"
  rotation_period   = "8760h" # 1 год
  
  tags = local.common_tags
}

# Создание сервисного аккаунта для кластера Kubernetes
resource "yandex_iam_service_account" "k8s_cluster_sa" {
  name        = "${local.resource_prefix}-k8s-cluster-sa"
  description = "Service account for Kubernetes cluster nodes"
  folder_id   = var.yc_folder_id
}

# Роли для сервисного аккаунта кластера
resource "yandex_resourcemanager_folder_iam_member" "k8s_cluster_roles" {
  for_each = toset([
    "container-registry.images.puller",
    "monitoring.viewer",
    "logging.writer",
    "load-balancer.admin",
    "vpc.user",
    "compute.viewer"
  ])
  
  folder_id = var.yc_folder_id
  role      = each.key
  member    = "serviceAccount:${yandex_iam_service_account.k8s_cluster_sa.id}"
}

# Создание сервисного аккаунта для работы с managed PostgreSQL
resource "yandex_iam_service_account" "postgres_sa" {
  name        = "${local.resource_prefix}-postgres-sa"
  description = "Service account for managed PostgreSQL operations"
  folder_id   = var.yc_folder_id
}

# Роли для сервисного аккаунта PostgreSQL
resource "yandex_resourcemanager_folder_iam_member" "postgres_sa_roles" {
  for_each = toset([
    "managed-postgresql.admin",
    "vpc.user"
  ])
  
  folder_id = var.yc_folder_id
  role      = each.key
  member    = "serviceAccount:${yandex_iam_service_account.postgres_sa.id}"
}