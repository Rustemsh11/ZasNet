# ZasNet - Быстрый старт с Docker

## Минимальные требования

- Docker Desktop для Windows
- 4 GB свободной RAM
- 10 GB свободного места на диске

## Запуск за 3 шага

### 1. Откройте PowerShell в директории проекта

```powershell
cd C:\Users\rsh\source\repos\ZasNet
```

### 2. Запустите скрипт

```powershell
.\start.ps1
```

### 3. Откройте браузер

- **Web Client**: http://localhost:8080
- **API**: http://localhost:5142

## Остановка

```powershell
.\stop.ps1
```

## Просмотр логов

```powershell
.\logs.ps1
```

Или для конкретного сервиса:

```powershell
.\logs.ps1 -Service api
.\logs.ps1 -Service webclient
.\logs.ps1 -Service postgres
```

## Настройка (опционально)

Создайте файл `.env` в корне проекта для изменения паролей и ключей:

```env
POSTGRES_PASSWORD=ваш_пароль
JWT_SECRET_KEY=ваш_секретный_ключ_минимум_32_символа
```

## Проблемы?

### Порты заняты

Если порты 5432, 5142 или 8080 заняты, измените их в `docker-compose.yml`:

```yaml
ports:
  - "8080:80"  # Измените 8080 на свободный порт
```

### Контейнеры не запускаются

1. Проверьте логи:
   ```powershell
   docker-compose logs
   ```

2. Убедитесь что Docker Desktop запущен

3. Проверьте доступное место на диске

### Полная очистка и перезапуск

```powershell
docker-compose down -v
.\start.ps1
```

⚠️ **Внимание**: команда `docker-compose down -v` удалит все данные из базы данных!

## Полная документация

Смотрите [DOCKER_README.md](DOCKER_README.md) для подробной документации.

