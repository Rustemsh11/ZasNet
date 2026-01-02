# ZasNet Docker Compose Status Script
# Этот скрипт показывает статус всех сервисов

Write-Host "=== ZasNet Docker Compose Status ===" -ForegroundColor Cyan
Write-Host ""

# Проверка наличия docker-compose.yml
if (-Not (Test-Path "docker-compose.yml")) {
    Write-Host "✗ Файл docker-compose.yml не найден!" -ForegroundColor Red
    exit 1
}

Write-Host "Статус контейнеров:" -ForegroundColor Yellow
Write-Host ""
docker-compose ps

Write-Host ""
Write-Host "Использование ресурсов:" -ForegroundColor Yellow
Write-Host ""
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}" zasnet-postgres zasnet-api zasnet-webclient 2>$null

Write-Host ""
Write-Host "Volumes:" -ForegroundColor Yellow
docker volume ls --filter name=zasnet

Write-Host ""
Write-Host "Networks:" -ForegroundColor Yellow
docker network ls --filter name=zasnet

