# ZasNet Docker Compose Stop Script
# Этот скрипт останавливает все сервисы ZasNet

Write-Host "=== ZasNet Docker Compose Stop ===" -ForegroundColor Yellow
Write-Host ""

# Проверка наличия docker-compose.yml
if (-Not (Test-Path "docker-compose.yml")) {
    Write-Host "✗ Файл docker-compose.yml не найден!" -ForegroundColor Red
    exit 1
}

Write-Host "Остановка сервисов..." -ForegroundColor Cyan
docker-compose stop

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Сервисы остановлены" -ForegroundColor Green
    Write-Host ""
    Write-Host "Для полного удаления контейнеров выполните:" -ForegroundColor Yellow
    Write-Host "  docker-compose down" -ForegroundColor White
    Write-Host ""
    Write-Host "Для удаления контейнеров и данных БД:" -ForegroundColor Yellow
    Write-Host "  docker-compose down -v" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "✗ Ошибка при остановке сервисов!" -ForegroundColor Red
    exit 1
}

