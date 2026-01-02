# ZasNet Docker Compose Logs Script
# Этот скрипт показывает логи всех сервисов

param(
    [string]$Service = ""
)

Write-Host "=== ZasNet Docker Compose Logs ===" -ForegroundColor Cyan
Write-Host ""

if ($Service -eq "") {
    Write-Host "Показываю логи всех сервисов..." -ForegroundColor Yellow
    Write-Host "Нажмите Ctrl+C для выхода" -ForegroundColor Yellow
    Write-Host ""
    docker-compose logs -f
} else {
    Write-Host "Показываю логи сервиса: $Service" -ForegroundColor Yellow
    Write-Host "Нажмите Ctrl+C для выхода" -ForegroundColor Yellow
    Write-Host ""
    docker-compose logs -f $Service
}

