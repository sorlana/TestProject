Пояснения:
1. Ingress:
Настроен для работы с ingress-nginx контроллером.

Использует аннотации для настройки CORS, безопасности (заголовки) и прокси.

Поддерживает TLS с использованием Let's Encrypt (cert-manager).

Маршрутизация:

api.testproject.com/api → test-service:8080

api.testproject.com/health → health checks

api.testproject.com/swagger → Swagger UI

api.testproject.com/metrics → метрики приложения

testproject.com → основной сервис (если нужно)

2. Network Policies:
default-deny-all: запрещает весь трафик по умолчанию (zero-trust).

allow-dns: разрешает исходящий DNS трафик.

allow-ingress: разрешает входящий трафик от ingress-nginx контроллера.

allow-monitoring: разрешает входящий трафик от Prometheus (если есть).

allow-namespace-internal: разрешает трафик между микросервисами внутри namespace.

allow-egress-external: разрешает исходящий трафик для доступа к внешним API (SMTP, HTTPS, HTTP).

postgres-allow-test-service: разрешает доступ к PostgreSQL только от test-service.

postgres-egress: разрешает исходящий трафик из PostgreSQL (в основном DNS).

Примечания:
Ingress класс может отличаться в зависимости от используемого ingress контроллера (например, nginx, traefik, haproxy).

TLS сертификаты могут быть предоставлены через cert-manager или вручную.

Network Policies требуют поддержки сетевым плагином Kubernetes (например, Calico, Cilium, Weave Net).

Для продакшена необходимо настроить правильные домены и TLS сертификаты.

В зависимости от облачного провайдера, могут потребоваться дополнительные аннотации для Ingress.

Использование:
bash
# Применить Ingress и Network Policies
kubectl apply -f infra/k8s/base/networking/

# Проверить Ingress
kubectl get ingress -n testproject-namespace

# Проверить Network Policies
kubectl get networkpolicies -n testproject-namespace
Эти конфигурации обеспечат безопасный и контролируемый сетевой доступ к вашему .NET микросервису и базе данных.