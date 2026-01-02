# ZasNet Docker Compose Setup

Этот файл содержит инструкции по запуску всей системы ZasNet с использованием Docker Compose.

## Компоненты

Система состоит из трех контейнеров:

1. **PostgreSQL** - База данных (порт 5432)
2. **ZasNet API** - ASP.NET Core Web API (порт 5142)
3. **ZasNet Web Client** - Blazor WebAssembly клиент (порт 8080)

## Предварительные требования

- Docker Desktop установлен и запущен
- Docker Compose установлен (обычно входит в Docker Desktop)
- Git (для клонирования репозиториев)

## Структура проектов

Убедитесь, что у вас следующая структура директорий:

```
C:\Users\rsh\source\repos\
├── ZasNet\                         # API проект
│   ├── docker-compose.yml          # Главный файл Docker Compose
│   ├── ZasNet.WebApi\
│   │   └── Dockerfile
│   └── ...
└── ZasNetWebClient\                # Web Client проект
    └── ZasNetWebClient\
        ├── Dockerfile
        └── nginx.conf
```

## Быстрый старт

### 1. Настройка переменных окружения (опционально)

Создайте файл `.env` в корне проекта ZasNet (рядом с docker-compose.yml):

```env
# Database Configuration
POSTGRES_PASSWORD=ваш_надежный_пароль

# JWT Configuration
JWT_SECRET_KEY=ваш_секретный_ключ_минимум_32_символа

# Telegram Bot Configuration (если используете)
TELEGRAM_BOT_TOKEN=ваш_токен_бота
TELEGRAM_CHANNEL_ID=ваш_id_канала
TELEGRAM_WEBHOOK_SECRET=ваш_webhook_секрет
```

Если файл `.env` не создан, будут использованы значения по умолчанию.

### 2. Запуск всех сервисов

Откройте PowerShell в директории `C:\Users\rsh\source\repos\ZasNet\` и выполните:

```powershell
# Сборка и запуск всех контейнеров
docker-compose up -d --build
```

Параметры:
- `-d` - запуск в фоновом режиме
- `--build` - пересборка образов

### 3. Проверка статуса

```powershell
# Проверить статус контейнеров
docker-compose ps

# Просмотр логов
docker-compose logs -f

# Логи конкретного сервиса
docker-compose logs -f api
docker-compose logs -f webclient
docker-compose logs -f postgres
```

### 4. Доступ к приложению

После успешного запуска:

- **Web Client**: http://localhost:8080
- **API**: http://localhost:5142
- **PostgreSQL**: localhost:5432

## Управление сервисами

### Остановка сервисов

```powershell
# Остановить все контейнеры
docker-compose stop

# Остановить и удалить контейнеры (данные БД сохранятся)
docker-compose down

# Остановить и удалить контейнеры и volumes (УДАЛИТ ДАННЫЕ БД!)
docker-compose down -v
```

### Перезапуск сервиса

```powershell
# Перезапустить API
docker-compose restart api

# Перезапустить Web Client
docker-compose restart webclient
```

### Обновление кода

После изменения кода нужно пересобрать образ:

```powershell
# Пересобрать и перезапустить API
docker-compose up -d --build api

# Пересобрать и перезапустить Web Client
docker-compose up -d --build webclient
```

### Просмотр логов

```powershell
# Все логи
docker-compose logs -f

# Логи API
docker-compose logs -f api

# Последние 100 строк логов
docker-compose logs --tail=100 api
```

## Работа с базой данных

### Подключение к PostgreSQL

```powershell
# Подключиться к PostgreSQL через psql
docker-compose exec postgres psql -U zasnet -d ZasNet
```

### Резервное копирование

```powershell
# Создать backup
docker-compose exec -T postgres pg_dump -U zasnet ZasNet > backup.sql

# Восстановить из backup
docker-compose exec -T postgres psql -U zasnet -d ZasNet < backup.sql
```

## Отладка

### Проверка здоровья контейнеров

```powershell
# Проверить health check
docker-compose ps

# Подробная информация о контейнере
docker inspect zasnet-api
```

### Вход в контейнер

```powershell
# Войти в контейнер API
docker-compose exec api /bin/bash

# Войти в контейнер PostgreSQL
docker-compose exec postgres /bin/bash

# Войти в контейнер Web Client
docker-compose exec webclient /bin/sh
```

### Очистка

```powershell
# Удалить все остановленные контейнеры
docker container prune

# Удалить неиспользуемые образы
docker image prune

# Полная очистка (ОСТОРОЖНО!)
docker system prune -a --volumes
```

## Troubleshooting

### Проблема: Контейнер API не запускается

1. Проверьте логи: `docker-compose logs api`
2. Убедитесь что PostgreSQL запущен: `docker-compose ps postgres`
3. Проверьте подключение к БД в логах

### Проблема: Web Client не может подключиться к API

1. Убедитесь что API запущен: `docker-compose ps api`
2. Проверьте логи API: `docker-compose logs api`
3. Проверьте настройки в `appsettings.json` веб клиента

### Проблема: Порт уже используется

Если порт 5432, 5142 или 8080 уже используется:

1. Остановите другие сервисы использующие эти порты
2. Или измените порты в `docker-compose.yml`:

```yaml
ports:
  - "8080:80"  # Измените 8080 на другой порт
```

### Проблема: Недостаточно места на диске

```powershell
# Проверить использование места
docker system df

# Очистить
docker system prune -a
```

## Дополнительная информация

### Переменные окружения

Полный список переменных окружения для API:

- `ASPNETCORE_ENVIRONMENT` - окружение (Development/Production)
- `ConnectionStrings__sqlConnection` - строка подключения к БД
- `AuthSettings__secretKey` - секретный ключ для JWT
- `TelegramSettings__BotToken` - токен Telegram бота
- `TelegramSettings__ChannelId` - ID Telegram канала
- `TelegramSettings__WebhookSecret` - секрет для webhook

### Volumes

- `postgres-data` - данные PostgreSQL (сохраняются между перезапусками)

## Продакшн рекомендации

1. **Безопасность**:
   - Используйте сильные пароли в `.env`
   - Не коммитьте `.env` в Git
   - Измените секретные ключи на уникальные

2. **Производительность**:
   - Настройте ресурсы Docker Desktop
   - Используйте отдельный сервер для PostgreSQL в продакшн

3. **Мониторинг**:
   - Настройте логирование в внешнюю систему
   - Используйте health checks

4. **Backup**:
   - Регулярно делайте backup БД
   - Храните backup в безопасном месте

## Поддержка

Если возникли проблемы:

1. Проверьте логи всех сервисов
2. Убедитесь что все порты доступны
3. Проверьте версии Docker и Docker Compose
4. Убедитесь что достаточно ресурсов (CPU, RAM, Disk)

