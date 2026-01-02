# 📚 Docker Documentation Index

Быстрая навигация по всей Docker документации для ZasNet.

## 🚀 С чего начать?

### Новичок в Docker?
1. **[QUICK_START.md](QUICK_START.md)** ⭐ - Начните отсюда! Минимальные инструкции для запуска.

### Опытный пользователь?
1. **[README_DOCKER.ru.md](README_DOCKER.ru.md)** - Главный README с обзором.
2. **[DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)** - Чеклист для проверки настройки.

## 📖 Документация

### Основные документы

| Документ | Описание | Для кого |
|----------|----------|----------|
| **[QUICK_START.md](QUICK_START.md)** | Быстрый старт за 3 шага | Все |
| **[README_DOCKER.ru.md](README_DOCKER.ru.md)** | Главный README, обзор | Все |
| **[DOCKER_README.md](DOCKER_README.md)** | Подробная документация | Пользователи |
| **[DOCKER_SETUP_SUMMARY.md](DOCKER_SETUP_SUMMARY.md)** | Техническая сводка | Разработчики |
| **[DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)** | Чеклист проверки | DevOps |
| **[CHANGES_SUMMARY.md](CHANGES_SUMMARY.md)** | Сводка изменений | Разработчики |

### Скрипты

| Скрипт | Описание | Использование |
|--------|----------|---------------|
| **start.ps1** | Запуск всех сервисов | `.\start.ps1` |
| **stop.ps1** | Остановка сервисов | `.\stop.ps1` |
| **logs.ps1** | Просмотр логов | `.\logs.ps1` или `.\logs.ps1 -Service api` |
| **status.ps1** | Проверка статуса | `.\status.ps1` |

## 🎯 Быстрые ссылки по задачам

### Первый запуск
1. [QUICK_START.md](QUICK_START.md) - Быстрый старт
2. [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md) - Проверка настройки

### Ежедневное использование
- **Запуск:** `.\start.ps1`
- **Остановка:** `.\stop.ps1`
- **Логи:** `.\logs.ps1`
- **Статус:** `.\status.ps1`

### Troubleshooting
1. [DOCKER_README.md - Troubleshooting](DOCKER_README.md#troubleshooting)
2. [DOCKER_CHECKLIST.md - Troubleshooting Checklist](DOCKER_CHECKLIST.md#-troubleshooting-checklist)

### Backup и Restore
- [DOCKER_README.md - Работа с базой данных](DOCKER_README.md#работа-с-базой-данных)

### Продакшн
- [DOCKER_README.md - Продакшн рекомендации](DOCKER_README.md#продакшн-рекомендации)
- [DOCKER_SETUP_SUMMARY.md - Безопасность](DOCKER_SETUP_SUMMARY.md#-безопасность)

### Архитектура и технические детали
- [DOCKER_SETUP_SUMMARY.md](DOCKER_SETUP_SUMMARY.md)
- [CHANGES_SUMMARY.md](CHANGES_SUMMARY.md)

## 📁 Структура файлов

### В ZasNet

```
ZasNet/
├── docker-compose.yml          # Главная конфигурация
├── .dockerignore               # Исключения для Docker
├── .gitignore                  # Обновлен для Docker
│
├── start.ps1                   # Скрипт запуска
├── stop.ps1                    # Скрипт остановки
├── logs.ps1                    # Скрипт логов
├── status.ps1                  # Скрипт статуса
│
├── README_DOCKER.ru.md         # Главный README
├── QUICK_START.md              # Быстрый старт
├── DOCKER_README.md            # Подробная документация
├── DOCKER_SETUP_SUMMARY.md     # Техническая сводка
├── DOCKER_CHECKLIST.md         # Чеклист
├── CHANGES_SUMMARY.md          # Сводка изменений
├── DOCKER_INDEX.md             # Этот файл
│
└── ZasNet.WebApi/
    ├── Dockerfile              # Dockerfile для API
    └── Program.cs              # Обновлен (health check)
```

### В ZasNetWebClient

```
ZasNetWebClient/ZasNetWebClient/
├── Dockerfile                  # Dockerfile для веб клиента
├── nginx.conf                  # Конфигурация nginx
├── .dockerignore               # Исключения для Docker
├── Program.cs                  # Обновлен (динамический API URL)
│
└── wwwroot/
    ├── appsettings.json        # Конфигурация для development
    └── appsettings.Production.json  # Конфигурация для production
```

## 🔍 Поиск информации

### Как запустить?
→ [QUICK_START.md](QUICK_START.md)

### Как остановить?
→ [QUICK_START.md - Остановка](QUICK_START.md#остановка)

### Как посмотреть логи?
→ [QUICK_START.md - Просмотр логов](QUICK_START.md#просмотр-логов)

### Как настроить переменные окружения?
→ [DOCKER_README.md - Настройка переменных окружения](DOCKER_README.md#1-настройка-переменных-окружения-опционально)

### Порт уже занят, что делать?
→ [DOCKER_README.md - Troubleshooting - Порт занят](DOCKER_README.md#проблема-порт-занят)

### Как сделать backup БД?
→ [DOCKER_README.md - Резервное копирование](DOCKER_README.md#резервное-копирование)

### Как подключиться к PostgreSQL?
→ [DOCKER_README.md - Работа с базой данных](DOCKER_README.md#работа-с-базой-данных)

### Какие порты используются?
→ [DOCKER_SETUP_SUMMARY.md - Порты](DOCKER_SETUP_SUMMARY.md#порты)

### Какая архитектура системы?
→ [DOCKER_SETUP_SUMMARY.md - Архитектура](DOCKER_SETUP_SUMMARY.md#архитектура)

### Что было изменено в коде?
→ [CHANGES_SUMMARY.md](CHANGES_SUMMARY.md)

### Как проверить что все работает?
→ [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)

## 💡 Полезные команды

### Основные

```powershell
# Запуск
.\start.ps1

# Остановка
.\stop.ps1

# Статус
.\status.ps1

# Логи
.\logs.ps1
.\logs.ps1 -Service api
```

### Docker Compose

```powershell
# Статус контейнеров
docker-compose ps

# Логи
docker-compose logs -f

# Перезапуск сервиса
docker-compose restart api

# Пересборка
docker-compose up -d --build

# Остановка
docker-compose down

# Остановка + удаление volumes
docker-compose down -v
```

### PostgreSQL

```powershell
# Подключение
docker-compose exec postgres psql -U zasnet -d ZasNet

# Backup
docker-compose exec -T postgres pg_dump -U zasnet ZasNet > backup.sql

# Restore
Get-Content backup.sql | docker-compose exec -T postgres psql -U zasnet -d ZasNet
```

## 🎓 Обучающие материалы

### Для новичков в Docker

1. Прочитайте [QUICK_START.md](QUICK_START.md)
2. Запустите систему: `.\start.ps1`
3. Изучите [DOCKER_README.md](DOCKER_README.md)
4. Попробуйте команды из раздела "Управление сервисами"

### Для разработчиков

1. Изучите [DOCKER_SETUP_SUMMARY.md](DOCKER_SETUP_SUMMARY.md)
2. Прочитайте [CHANGES_SUMMARY.md](CHANGES_SUMMARY.md)
3. Изучите Dockerfile для API и Web Client
4. Изучите docker-compose.yml

### Для DevOps

1. Изучите [DOCKER_SETUP_SUMMARY.md](DOCKER_SETUP_SUMMARY.md)
2. Проверьте [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)
3. Изучите health checks в docker-compose.yml
4. Изучите раздел "Продакшн рекомендации"

## 🔗 Внешние ресурсы

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [nginx Docker Image](https://hub.docker.com/_/nginx)

## 📊 Диаграмма принятия решений

```
Нужно запустить систему?
│
├─ Первый раз?
│  └─ Да → QUICK_START.md
│
├─ Что-то не работает?
│  └─ Да → DOCKER_README.md (Troubleshooting)
│
├─ Нужна подробная информация?
│  └─ Да → DOCKER_README.md
│
├─ Нужны технические детали?
│  └─ Да → DOCKER_SETUP_SUMMARY.md
│
├─ Нужно проверить настройку?
│  └─ Да → DOCKER_CHECKLIST.md
│
└─ Нужно узнать что изменилось?
   └─ Да → CHANGES_SUMMARY.md
```

## 🆘 Получить помощь

1. **Проверьте логи:** `.\logs.ps1`
2. **Проверьте статус:** `.\status.ps1`
3. **Прочитайте Troubleshooting:**
   - [DOCKER_README.md - Troubleshooting](DOCKER_README.md#troubleshooting)
   - [DOCKER_CHECKLIST.md - Troubleshooting](DOCKER_CHECKLIST.md#-troubleshooting-checklist)
4. **Проверьте чеклист:** [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)

## ✅ Быстрая проверка

Все работает если:
- ✅ `.\start.ps1` выполнился без ошибок
- ✅ `.\status.ps1` показывает все контейнеры Up
- ✅ http://localhost:8080 открывается
- ✅ http://localhost:5142/health возвращает JSON

## 📝 Обратная связь

Если нашли ошибку в документации или есть предложения:
1. Создайте Issue на GitHub
2. Опишите проблему или предложение
3. Приложите логи если это ошибка

---

**Последнее обновление:** 2 января 2026  
**Версия:** 1.0  
**Статус:** ✅ Актуально

