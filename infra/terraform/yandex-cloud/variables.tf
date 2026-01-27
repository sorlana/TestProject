# Основные переменные Yandex Cloud
variable "yc_cloud_id" {
  type        = string
  description = "Yandex Cloud Cloud ID"
  sensitive   = true
}

variable "yc_folder_id" {
  type        = string
  description = "Yandex Cloud Folder ID"
  sensitive   = true
}

variable "yc_zone" {
  type        = string
  description = "Yandex Cloud default zone"
  default     = "ru-central1-a"
}

variable "yc_token" {
  type        = string
  description = "Yandex Cloud OAuth token (alternative to service account key)"
  sensitive   = true
  default     = ""
}

variable "yc_service_account_key_file" {
  type        = string
  description = "Path to Yandex Cloud service account key file"
  sensitive   = true
  default     = ""
}

# Переменные для бэкенда S3
variable "YC_STORAGE_ACCESS_KEY" {
  type        = string
  description = "Yandex Cloud Storage access key for Terraform backend"
  sensitive   = true
}

variable "YC_STORAGE_SECRET_KEY" {
  type        = string
  description = "Yandex Cloud Storage secret key for Terraform backend"
  sensitive   = true
}

# Окружение
variable "environment" {
  type        = string
  description = "Deployment environment (prod, staging, dev)"
  default     = "prod"
  
  validation {
    condition     = contains(["prod", "staging", "dev"], var.environment)
    error_message = "Environment must be one of: prod, staging, dev"
  }
}

# Настройки сети
variable "vpc_cidr" {
  type        = string
  description = "CIDR block for VPC"
  default     = "10.0.0.0/16"
}

variable "public_subnet_cidrs" {
  type        = list(string)
  description = "CIDR blocks for public subnets"
  default     = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
}

variable "private_subnet_cidrs" {
  type        = list(string)
  description = "CIDR blocks for private subnets"
  default     = ["10.0.10.0/24", "10.0.11.0/24", "10.0.12.0/24"]
}

variable "database_subnet_cidrs" {
  type        = list(string)
  description = "CIDR blocks for database subnets"
  default     = ["10.0.20.0/24", "10.0.21.0/24", "10.0.22.0/24"]
}

# Настройки Kubernetes
variable "k8s_version" {
  type        = string
  description = "Kubernetes version for managed cluster"
  default     = "1.28"
  
  validation {
    condition     = can(regex("^1\\.(2[4-9]|3[0-1])$", var.k8s_version))
    error_message = "Kubernetes version must be between 1.24 and 1.31"
  }
}

variable "k8s_node_groups" {
  type = map(object({
    name         = string
    node_count   = number
    min_size     = number
    max_size     = number
    cpu          = number
    memory       = number
    disk_size    = number
    disk_type    = string
    preemptible  = bool
    node_labels  = map(string)
    node_taints  = list(string)
  }))
  
  description = "Configuration for Kubernetes node groups"
  
  default = {
    system = {
      name         = "system"
      node_count   = 3
      min_size     = 3
      max_size     = 5
      cpu          = 4
      memory       = 8
      disk_size    = 100
      disk_type    = "network-ssd"
      preemptible  = false
      node_labels = {
        "node-type" = "system"
        "critical"  = "true"
      }
      node_taints = [
        "CriticalAddonsOnly=true:NoSchedule"
      ]
    }
    
    application = {
      name         = "application"
      node_count   = 2
      min_size     = 2
      max_size     = 10
      cpu          = 8
      memory       = 16
      disk_size    = 100
      disk_type    = "network-ssd"
      preemptible  = true
      node_labels = {
        "node-type" = "application"
        "pool"      = "application"
      }
      node_taints = []
    }
    
    monitoring = {
      name         = "monitoring"
      node_count   = 2
      min_size     = 2
      max_size     = 4
      cpu          = 4
      memory       = 8
      disk_size    = 200
      disk_type    = "network-ssd"
      preemptible  = true
      node_labels = {
        "node-type" = "monitoring"
        "pool"      = "monitoring"
      }
      node_taints = []
    }
  }
}

# Настройки PostgreSQL
variable "postgres_version" {
  type        = string
  description = "PostgreSQL version"
  default     = "15"
  
  validation {
    condition     = contains(["13", "14", "15", "16"], var.postgres_version)
    error_message = "PostgreSQL version must be one of: 13, 14, 15, 16"
  }
}

variable "postgres_resources" {
  type = object({
    disk_size          = number
    disk_type          = string
    resource_preset_id = string
  })
  
  description = "Resources for managed PostgreSQL cluster"
  
  default = {
    disk_size          = 100
    disk_type          = "network-ssd"
    resource_preset_id = "s2.micro" # 2 vCPU, 8GB RAM
  }
}

variable "postgres_users" {
  type = list(object({
    name       = string
    password   = string
    conn_limit = number
    grants     = list(string)
  }))
  
  description = "Users for managed PostgreSQL"
  
  default = [
    {
      name       = "admin"
      password   = "" # Will be generated
      conn_limit = 50
      grants     = ["mdb_admin", "mdb_monitoring"]
    },
    {
      name       = "appuser"
      password   = "" # Will be generated
      conn_limit = 100
      grants     = ["mdb_basic_role"]
    },
    {
      name       = "monitoring"
      password   = "" # Will be generated
      conn_limit = 10
      grants     = ["mdb_monitoring"]
    }
  ]
}

variable "postgres_databases" {
  type = list(object({
    name          = string
    owner         = string
    lc_collate    = string
    lc_ctype      = string
    extensions    = list(string)
  }))
  
  description = "Databases for managed PostgreSQL"
  
  default = [
    {
      name          = "testdb"
      owner         = "admin"
      lc_collate    = "C"
      lc_ctype      = "C"
      extensions    = ["uuid-ossp", "pgcrypto", "pg_stat_statements"]
    },
    {
      name          = "testdb_test"
      owner         = "admin"
      lc_collate    = "C"
      lc_ctype      = "C"
      extensions    = ["uuid-ossp", "pgcrypto"]
    }
  ]
}

variable "postgres_backup_settings" {
  type = object({
    backup_start_time = string
    backup_retain_period_days = number
  })
  
  description = "Backup settings for managed PostgreSQL"
  
  default = {
    backup_start_time         = "02:00"
    backup_retain_period_days = 7
  }
}

variable "postgres_high_availability" {
  type = object({
    enabled  = bool
    replicas = number
  })
  
  description = "High availability settings for managed PostgreSQL"
  
  default = {
    enabled  = true
    replicas = 1
  }
}

variable "postgres_networks" {
  type        = list(string)
  description = "Network IDs for PostgreSQL cluster access"
  default     = []
}

# Настройки Container Registry
variable "container_registry_name" {
  type        = string
  description = "Name for Container Registry"
  default     = "testproject-registry"
}

variable "container_registry_scan_on_push" {
  type        = bool
  description = "Enable vulnerability scanning on push"
  default     = true
}

# Настройки мониторинга
variable "enable_monitoring" {
  type        = bool
  description = "Enable Yandex Monitoring"
  default     = true
}

variable "enable_logging" {
  type        = bool
  description = "Enable Yandex Logging"
  default     = true
}

variable "create_alerting" {
  type        = bool
  description = "Create alerting rules in Yandex Monitoring"
  default     = true
}

# Настройки сертификатов
variable "certificate_domains" {
  type        = list(string)
  description = "Domains for SSL certificate"
  default     = ["testproject.com", "*.testproject.com"]
}

variable "letsencrypt_email" {
  type        = string
  description = "Email for Let's Encrypt certificate"
  default     = "admin@testproject.com"
}

# Настройки DNS
variable "dns_zone_id" {
  type        = string
  description = "Yandex Cloud DNS zone ID"
  default     = ""
}

variable "dns_records" {
  type = list(object({
    name    = string
    type    = string
    ttl     = number
    data    = list(string)
  }))
  
  description = "DNS records to create"
  
  default = [
    {
      name = "@"
      type = "A"
      ttl  = 300
      data = [] # Will be populated by load balancer IP
    },
    {
      name = "api"
      type = "A"
      ttl  = 300
      data = [] # Will be populated by load balancer IP
    },
    {
      name = "app"
      type = "A"
      ttl  = 300
      data = [] # Will be populated by load balancer IP
    },
    {
      name = "www"
      type = "CNAME"
      ttl  = 300
      data = ["testproject.com"]
    }
  ]
}

# Настройки бэкапов
variable "backup_retention_days" {
  type        = number
  description = "Number of days to retain backups"
  default     = 30
  
  validation {
    condition     = var.backup_retention_days >= 1 && var.backup_retention_days <= 365
    error_message = "Backup retention must be between 1 and 365 days"
  }
}

# Переменные для тегов
variable "additional_tags" {
  type        = map(string)
  description = "Additional tags for all resources"
  default     = {}
}

# Переменные для стоимости
variable "budget_limit" {
  type        = number
  description = "Monthly budget limit in rubles"
  default     = 10000
  
  validation {
    condition     = var.budget_limit > 0
    error_message = "Budget limit must be greater than 0"
  }
}

# Переменные для оповещений
variable "alert_contacts" {
  type = object({
    email = list(string)
    slack = list(string)
  })
  
  description = "Contacts for alerts"
  
  default = {
    email = ["team@testproject.com"]
    slack = []
  }
}

# Переменные для тестирования
variable "enable_destructive_tests" {
  type        = bool
  description = "Enable destructive tests (creates and destroys resources)"
  default     = false
}

variable "test_prefix" {
  type        = string
  description = "Prefix for test resources"
  default     = "test-"
}

# Переменные для CI/CD
variable "github_repository" {
  type        = string
  description = "GitHub repository for CI/CD integration"
  default     = ""
}

variable "github_branch" {
  type        = string
  description = "GitHub branch for CI/CD"
  default     = "main"
}