# ZasNet Docker Compose Start Script
# Этот скрипт запускает все сервисы ZasNet

Write-Host "=== ZasNet Docker Compose Start ===" -ForegroundColor Green
Write-Host ""

# Проверка наличия Docker
Write-Host "Проверка Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker найден: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker не найден. Установите Docker Desktop." -ForegroundColor Red
    exit 1
}

# Проверка наличия Docker Compose
Write-Host "Проверка Docker Compose..." -ForegroundColor Yellow
try {
    $composeVersion = docker-compose --version
    Write-Host "✓ Docker Compose найден: $composeVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker Compose не найден." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Проверка структуры проектов..." -ForegroundColor Yellow

# Проверка наличия docker-compose.yml
if (-Not (Test-Path "docker-compose.yml")) {
    Write-Host "✗ Файл docker-compose.yml не найден!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ docker-compose.yml найден" -ForegroundColor Green

# Проверка наличия веб клиента
$webClientPath = "..\ZasNetWebClient\ZasNetWebClient"
if (-Not (Test-Path $webClientPath)) {
    Write-Host "✗ Проект веб клиента не найден по пути: $webClientPath" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Проект веб клиента найден" -ForegroundColor Green

Write-Host ""
Write-Host "Запуск сервисов..." -ForegroundColor Yellow
Write-Host ""

# Остановка и удаление старых контейнеров
Write-Host "Остановка старых контейнеров (если есть)..." -ForegroundColor Cyan
docker-compose down

Write-Host ""
Write-Host "Сборка и запуск контейнеров..." -ForegroundColor Cyan
Write-Host "Это может занять несколько минут при первом запуске..." -ForegroundColor Yellow
Write-Host ""

# Запуск с выводом логов
docker-compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Сервисы успешно запущены! ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Доступ к приложениям:" -ForegroundColor Cyan
    Write-Host "  • Web Client: http://localhost:8080" -ForegroundColor White
    Write-Host "  • API:        http://localhost:5142" -ForegroundColor White
    Write-Host "  • PostgreSQL: localhost:5432" -ForegroundColor White
    Write-Host ""
    Write-Host "Полезные команды:" -ForegroundColor Cyan
    Write-Host "  • Просмотр логов:    docker-compose logs -f" -ForegroundColor White
    Write-Host "  • Статус сервисов:   docker-compose ps" -ForegroundColor White
    Write-Host "  • Остановка:         docker-compose stop" -ForegroundColor White
    Write-Host "  • Полная остановка:  docker-compose down" -ForegroundColor White
    Write-Host ""
    
    # Показать статус контейнеров
    Write-Host "Статус контейнеров:" -ForegroundColor Cyan
    docker-compose ps
    
    Write-Host ""
    Write-Host "Для просмотра логов выполните: docker-compose logs -f" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "✗ Ошибка при запуске сервисов!" -ForegroundColor Red
    Write-Host "Проверьте логи: docker-compose logs" -ForegroundColor Yellow
    exit 1
}

