# Сводка по настройке Docker для ZasNet

## Созданные файлы

### В проекте ZasNet (API)

1. **docker-compose.yml** - главный файл конфигурации Docker Compose
   - Определяет 3 сервиса: PostgreSQL, API, Web Client
   - Настроены health checks для всех сервисов
   - Настроены volumes для сохранения данных БД
   - Настроена сеть для взаимодействия контейнеров

2. **.dockerignore** - файлы, игнорируемые при сборке Docker образа

3. **DOCKER_README.md** - подробная документация по использованию Docker

4. **QUICK_START.md** - краткое руководство по быстрому старту

5. **start.ps1** - PowerShell скрипт для запуска всех сервисов
   - Проверяет наличие Docker
   - Проверяет структуру проектов
   - Запускает все контейнеры
   - Показывает статус

6. **stop.ps1** - PowerShell скрипт для остановки сервисов

7. **logs.ps1** - PowerShell скрипт для просмотра логов
   - Поддерживает просмотр логов всех сервисов или конкретного

8. **status.ps1** - PowerShell скрипт для проверки статуса
   - Показывает статус контейнеров
   - Показывает использование ресурсов
   - Показывает volumes и networks

9. **ZasNet.WebApi/Program.cs** - добавлен health check endpoint
   - Endpoint: GET /health
   - Возвращает статус и timestamp

### В проекте ZasNetWebClient

1. **Dockerfile** - конфигурация для сборки Docker образа веб клиента
   - Multi-stage build
   - Использует .NET SDK для сборки
   - Использует nginx для раздачи статических файлов

2. **nginx.conf** - конфигурация nginx для Blazor WebAssembly
   - Правильная настройка MIME types для .wasm файлов
   - Настройка кэширования
   - SPA routing (fallback на index.html)

3. **.dockerignore** - файлы, игнорируемые при сборке

4. **wwwroot/appsettings.json** - конфигурация для development
   - API URL: http://localhost:5142

5. **wwwroot/appsettings.Production.json** - конфигурация для production
   - API URL: http://localhost:5142

6. **Program.cs** - обновлен для чтения API URL из конфигурации
   - Динамическое определение API URL
   - Fallback на localhost для разработки

## Архитектура

```
┌─────────────────────────────────────────────────────────────┐
│                         Docker Host                          │
│                                                              │
│  ┌────────────────┐  ┌────────────────┐  ┌───────────────┐ │
│  │   PostgreSQL   │  │   ZasNet API   │  │  Web Client   │ │
│  │   (postgres)   │  │     (api)      │  │  (webclient)  │ │
│  │                │  │                │  │               │ │
│  │   Port: 5432   │  │   Port: 5142   │  │   Port: 80    │ │
│  │                │◄─┤                │◄─┤   (nginx)     │ │
│  └────────────────┘  └────────────────┘  └───────────────┘ │
│         │                                        │           │
│         │                                        │           │
│  ┌──────▼──────┐                          ┌─────▼──────┐   │
│  │ postgres-   │                          │  Host:     │   │
│  │   data      │                          │   8080     │   │
│  │  (volume)   │                          └────────────┘   │
│  └─────────────┘                                            │
│                                                              │
│         zasnet-network (bridge)                             │
└─────────────────────────────────────────────────────────────┘
```

## Порты

- **8080** - Web Client (доступен с хоста)
- **5142** - API (доступен с хоста)
- **5432** - PostgreSQL (доступен с хоста)

## Volumes

- **postgres-data** - данные PostgreSQL (сохраняются между перезапусками)

## Networks

- **zasnet-network** - bridge сеть для взаимодействия контейнеров

## Переменные окружения

### PostgreSQL
- `POSTGRES_DB` - имя базы данных
- `POSTGRES_USER` - пользователь БД
- `POSTGRES_PASSWORD` - пароль БД

### API
- `ASPNETCORE_ENVIRONMENT` - окружение (Development/Production)
- `ASPNETCORE_URLS` - URL для прослушивания
- `ConnectionStrings__sqlConnection` - строка подключения к БД
- `AuthSettings__secretKey` - секретный ключ для JWT
- `TelegramSettings__BotToken` - токен Telegram бота
- `TelegramSettings__ChannelId` - ID Telegram канала
- `TelegramSettings__WebhookSecret` - секрет для webhook

## Health Checks

### PostgreSQL
- Команда: `pg_isready -U zasnet -d ZasNet`
- Интервал: 10 секунд
- Timeout: 5 секунд
- Retries: 5

### API
- Endpoint: http://localhost:5142/health
- Интервал: 30 секунд
- Timeout: 10 секунд
- Retries: 3
- Start period: 40 секунд

## Зависимости

- **API** зависит от **PostgreSQL** (ждет healthy статуса)
- **Web Client** зависит от **API** (запускается после API)

## Использование

### Первый запуск

```powershell
# Перейти в директорию проекта
cd C:\Users\rsh\source\repos\ZasNet

# Запустить все сервисы
.\start.ps1
```

### Просмотр статуса

```powershell
.\status.ps1
```

### Просмотр логов

```powershell
# Все логи
.\logs.ps1

# Логи конкретного сервиса
.\logs.ps1 -Service api
```

### Остановка

```powershell
.\stop.ps1
```

## Особенности реализации

### Blazor WebAssembly + Docker

Blazor WebAssembly - это клиентское приложение, которое выполняется в браузере. При использовании Docker:

1. Приложение собирается с помощью .NET SDK
2. Результат публикации (статические файлы) копируется в nginx
3. nginx раздает статические файлы
4. Приложение выполняется в браузере пользователя
5. API вызовы идут на http://localhost:5142

### Multi-stage Build

Оба Dockerfile используют multi-stage build:

1. **Build stage** - сборка приложения с помощью SDK
2. **Publish stage** - публикация приложения
3. **Final stage** - минимальный runtime образ

Это позволяет:
- Уменьшить размер финального образа
- Исключить инструменты разработки из продакшн образа
- Ускорить сборку за счет кэширования слоев

### Health Checks

Health checks позволяют:
- Docker автоматически определять готовность сервиса
- Управлять зависимостями между сервисами
- Автоматически перезапускать упавшие сервисы

## Безопасность

### Рекомендации для продакшена

1. **Пароли**
   - Используйте сильные пароли (минимум 16 символов)
   - Не используйте пароли по умолчанию
   - Храните пароли в .env файле
   - Не коммитьте .env в Git

2. **JWT Secret**
   - Используйте случайную строку минимум 32 символа
   - Меняйте секрет при компрометации

3. **HTTPS**
   - В продакшене используйте HTTPS
   - Настройте SSL сертификаты
   - Используйте reverse proxy (nginx/traefik)

4. **Firewall**
   - Ограничьте доступ к портам
   - Используйте только необходимые порты

5. **Updates**
   - Регулярно обновляйте образы
   - Следите за уязвимостями

## Troubleshooting

### Проблема: API не может подключиться к PostgreSQL

**Решение:**
1. Проверьте логи PostgreSQL: `.\logs.ps1 -Service postgres`
2. Убедитесь что PostgreSQL в статусе healthy: `.\status.ps1`
3. Проверьте строку подключения в docker-compose.yml

### Проблема: Web Client показывает ошибки API

**Решение:**
1. Проверьте что API запущен: `.\status.ps1`
2. Проверьте логи API: `.\logs.ps1 -Service api`
3. Проверьте API URL в appsettings.json веб клиента

### Проблема: Порт занят

**Решение:**
1. Найдите процесс: `netstat -ano | findstr :8080`
2. Остановите процесс или измените порт в docker-compose.yml

### Проблема: Недостаточно места на диске

**Решение:**
1. Проверьте использование: `docker system df`
2. Очистите: `docker system prune -a`
3. Удалите старые volumes: `docker volume prune`

## Дополнительные команды

### Пересборка образов

```powershell
docker-compose build --no-cache
```

### Просмотр использования ресурсов

```powershell
docker stats
```

### Подключение к контейнеру

```powershell
# API
docker-compose exec api /bin/bash

# PostgreSQL
docker-compose exec postgres psql -U zasnet -d ZasNet

# Web Client
docker-compose exec webclient /bin/sh
```

### Backup базы данных

```powershell
docker-compose exec -T postgres pg_dump -U zasnet ZasNet > backup_$(Get-Date -Format "yyyy-MM-dd_HH-mm-ss").sql
```

### Restore базы данных

```powershell
Get-Content backup.sql | docker-compose exec -T postgres psql -U zasnet -d ZasNet
```

## Полезные ссылки

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [nginx Docker Image](https://hub.docker.com/_/nginx)

## Поддержка

Для получения помощи:
1. Проверьте логи: `.\logs.ps1`
2. Проверьте статус: `.\status.ps1`
3. Прочитайте DOCKER_README.md
4. Проверьте Issues на GitHub

