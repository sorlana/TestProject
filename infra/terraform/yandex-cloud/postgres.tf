# Создание managed PostgreSQL кластера
resource "yandex_mdb_postgresql_cluster" "main" {
  name        = "${local.resource_prefix}-postgresql-cluster"
  description = "Managed PostgreSQL cluster for TestProject"
  environment = "PRODUCTION"
  network_id  = yandex_vpc_network.main.id
  
  config {
    version = var.postgres_version
    
    resources {
      disk_size          = var.postgres_resources.disk_size
      disk_type_id       = var.postgres_resources.disk_type
      resource_preset_id = var.postgres_resources.resource_preset_id
    }
    
    postgresql_config = local.postgres_settings
    
    pooler_config {
      pooling_mode = "TRANSACTION"
    }
    
    performance_diagnostics {
      enabled                     = true
      sessions_sampling_interval  = 60
      statements_sampling_interval = 600
    }
  }
  
  database {
    name       = "postgres"
    owner      = "admin"
    lc_collate = "C"
    lc_type    = "C"
  }
  
  dynamic "database" {
    for_each = var.postgres_databases
    content {
      name       = database.value.name
      owner      = database.value.owner
      lc_collate = database.value.lc_collate
      lc_type    = database.value.lc_ctype
      
      extension {
        name = "uuid-ossp"
      }
      
      extension {
        name = "pgcrypto"
      }
      
      extension {
        name = "pg_stat_statements"
      }
    }
  }
  
  dynamic "user" {
    for_each = var.postgres_users
    content {
      name       = user.value.name
      password   = user.value.password != "" ? user.value.password : random_password.postgres_users[user.value.name].result
      conn_limit = user.value.conn_limit
      
      dynamic "permission" {
        for_each = user.value.grants
        content {
          database_name = "testdb"
        }
      }
    }
  }
  
  host {
    zone      = local.availability_zones[0]
    subnet_id = yandex_vpc_subnet.database[0].id
    
    assign_public_ip = false
    
    replication_source_name = var.postgres_high_availability.enabled ? null : ""
  }
  
  dynamic "host" {
    for_each = var.postgres_high_availability.enabled ? range(1, var.postgres_high_availability.replicas + 1) : []
    content {
      zone      = local.availability_zones[host.value % length(local.availability_zones)]
      subnet_id = yandex_vpc_subnet.database[host.value % length(yandex_vpc_subnet.database)].id
      
      assign_public_ip    = false
      replication_source  = "${local.resource_prefix}-postgresql-cluster-0"
      priority            = host.value * 100
    }
  }
  
  maintenance_window {
    type = "WEEKLY"
    day  = "SAT"
    hour = 3
  }
  
  security_group_ids = [
    yandex_vpc_security_group.postgres.id,
    yandex_vpc_security_group.internal.id
  ]
  
  deletion_protection = var.environment == "prod"
  
  labels = merge(local.common_tags, {
    "database"  = "postgresql"
    "engine"    = "postgresql"
    "version"   = var.postgres_version
  })
  
  depends_on = [
    yandex_vpc_subnet.database,
    yandex_vpc_security_group.postgres
  ]
}

# Создание пользователей PostgreSQL с случайными паролями
resource "random_password" "postgres_users" {
  for_each = {
    for user in var.postgres_users : user.name => user
    if user.password == ""
  }
  
  length  = 32
  special = true
  
  min_lower   = 1
  min_upper   = 1
  min_numeric = 1
  min_special = 1
}

# Создание базы данных
resource "yandex_mdb_postgresql_database" "databases" {
  for_each = {
    for db in var.postgres_databases : db.name => db
  }
  
  cluster_id = yandex_mdb_postgresql_cluster.main.id
  name       = each.value.name
  owner      = each.value.owner
  
  lc_collate = each.value.lc_collate
  lc_type    = each.value.lc_ctype
  
  dynamic "extension" {
    for_each = each.value.extensions
    content {
      name = extension.value
    }
  }
  
  depends_on = [
    yandex_mdb_postgresql_cluster.main
  ]
}

# Настройка backup для PostgreSQL
resource "yandex_mdb_postgresql_backup" "manual" {
  count = var.backup_retention_days > 0 ? 1 : 0
  
  cluster_id = yandex_mdb_postgresql_cluster.main.id
}

# Создание read-only реплики (если нужно)
resource "yandex_mdb_postgresql_cluster" "read_replica" {
  count = var.postgres_high_availability.enabled && var.environment == "prod" ? 1 : 0
  
  name        = "${local.resource_prefix}-postgresql-read-replica"
  description = "Read replica for PostgreSQL cluster"
  environment = "PRODUCTION"
  network_id  = yandex_vpc_network.main.id
  
  config {
    version = var.postgres_version
    
    resources {
      disk_size          = var.postgres_resources.disk_size
      disk_type_id       = var.postgres_resources.disk_type
      resource_preset_id = var.postgres_resources.resource_preset_id
    }
    
    postgresql_config = local.postgres_settings
  }
  
  host {
    zone      = local.availability_zones[1]
    subnet_id = yandex_vpc_subnet.database[1].id
    
    assign_public_ip = false
    replication_role = "REPLICA"
  }
  
  security_group_ids = [
    yandex_vpc_security_group.postgres.id,
    yandex_vpc_security_group.internal.id
  ]
  
  labels = merge(local.common_tags, {
    "database"    = "postgresql"
    "engine"      = "postgresql"
    "role"        = "read-replica"
    "replication" = "async"
  })
  
  depends_on = [
    yandex_mdb_postgresql_cluster.main
  ]
}

# Создание точки восстановления
resource "yandex_mdb_postgresql_restore" "test_restore" {
  count = var.enable_destructive_tests ? 1 : 0
  
  backup_id   = yandex_mdb_postgresql_backup.manual[0].id
  name        = "${local.resource_prefix}-postgresql-test-restore"
  folder_id   = var.yc_folder_id
  time        = timeadd(timestamp(), "24h")
  
  labels = merge(local.common_tags, {
    "test" = "true"
    "temp" = "true"
  })
  
  lifecycle {
    ignore_changes = [time]
  }
}

# Мониторинг для PostgreSQL
resource "yandex_monitoring_dashboard" "postgres_dashboard" {
  count = var.enable_monitoring ? 1 : 0
  
  dashboard_json = templatefile("${path.module}/templates/postgres-dashboard.json", {
    cluster_id = yandex_mdb_postgresql_cluster.main.id
    title      = "PostgreSQL - ${local.resource_prefix}"
  })
  
  depends_on = [
    yandex_mdb_postgresql_cluster.main
  ]
}

# Алерты для PostgreSQL
resource "yandex_monitoring_alert" "postgres_alerts" {
  count = var.create_alerting ? 1 : 0
  
  name = "${local.resource_prefix}-postgres-alerts"
  
  annotations = {
    summary     = "PostgreSQL cluster issues"
    description = "Alert for PostgreSQL cluster ${local.resource_prefix}-postgresql-cluster"
  }
  
  labels = {
    severity = "critical"
    service  = "postgresql"
  }
  
  alert_rule {
    name = "High CPU usage"
    
    expr = format("avg(avg_over_time(pg_stat_database_xact_commit{cluster_id=\"%s\"}[5m])) > 80", yandex_mdb_postgresql_cluster.main.id)
    
    for  = "5m"
    
    labels = {
      severity = "warning"
    }
    
    annotations = {
      description = "PostgreSQL CPU usage is above 80% for 5 minutes"
      summary     = "High PostgreSQL CPU usage"
    }
  }
  
  alert_rule {
    name = "High connections"
    
    expr = format("avg(avg_over_time(pg_stat_activity_count{cluster_id=\"%s\"}[5m])) > %d", yandex_mdb_postgresql_cluster.main.id, var.postgres_users[0].conn_limit * 0.8)
    
    for  = "5m"
    
    labels = {
      severity = "warning"
    }
    
    annotations = {
      description = "PostgreSQL connections are above 80% of limit"
      summary     = "High PostgreSQL connections"
    }
  }
  
  alert_rule {
    name = "Disk space running low"
    
    expr = format("avg(avg_over_time(pg_database_size_bytes{cluster_id=\"%s\"}[1h])) / %d * 100 > 80", yandex_mdb_postgresql_cluster.main.id, var.postgres_resources.disk_size * 1024 * 1024 * 1024)
    
    for  = "1h"
    
    labels = {
      severity = "critical"
    }
    
    annotations = {
      description = "PostgreSQL disk usage is above 80%"
      summary     = "PostgreSQL disk space running low"
    }
  }
  
  notification_channel_ids = [
    for channel in yandex_monitoring_notification_channel.email : channel.id
  ]
  
  depends_on = [
    yandex_mdb_postgresql_cluster.main,
    yandex_monitoring_notification_channel.email
  ]
}