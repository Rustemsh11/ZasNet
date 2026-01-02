# 🚀 Инструкция по развертыванию ZasNet на production сервере

## 📋 Предварительные требования

### На сервере должно быть установлено:
- Ubuntu 22.04 LTS или новее (или другой Linux дистрибутив)
- Docker (версия 24.0+)
- Docker Compose (версия 2.0+)
- Доменное имя, указывающее на IP сервера
- Открыты порты: 80 (HTTP), 443 (HTTPS), 22 (SSH)

---

## 🔧 Шаг 1: Подготовка сервера

### 1.1. Подключитесь к серверу

```bash
ssh root@your-server-ip
# или
ssh your-user@your-server-ip
```

### 1.2. Обновите систему

```bash
sudo apt update && sudo apt upgrade -y
```

### 1.3. Установите Docker

```bash
# Установка Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Добавьте пользователя в группу docker
sudo usermod -aG docker $USER

# Перезагрузите сессию или выполните
newgrp docker

# Проверьте установку
docker --version
docker compose version
```

### 1.4. Настройте Firewall (UFW)

```bash
# Установка UFW (если еще не установлен)
sudo apt install ufw -y

# Разрешите SSH (ВАЖНО! Сделайте это перед включением UFW)
sudo ufw allow 22/tcp

# Разрешите HTTP и HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Включите firewall
sudo ufw enable

# Проверьте статус
sudo ufw status
```

---

## 📂 Шаг 2: Загрузка проекта на сервер

### Вариант 1: Через Git (рекомендуется)

```bash
# Создайте директорию для проектов
sudo mkdir -p /opt/apps
cd /opt/apps

# Клонируйте репозитории
sudo git clone https://github.com/your-username/ZasNet.git
sudo git clone https://github.com/your-username/ZasNetWebClient.git

# Установите права
sudo chown -R $USER:$USER /opt/apps
```

### Вариант 2: Через SCP/SFTP

```powershell
# На вашем локальном компьютере (Windows)
# Используйте WinSCP, FileZilla или scp команду

# Пример с scp:
scp -r C:\Users\rsh\source\repos\ZasNet your-user@your-server:/opt/apps/
scp -r C:\Users\rsh\source\repos\ZasNetWebClient your-user@your-server:/opt/apps/
```

---

## 🔐 Шаг 3: Настройка переменных окружения

```bash
cd /opt/apps/ZasNet

# Создайте .env файл из шаблона
cp .env.production .env

# Отредактируйте .env файл
nano .env
```

### Установите следующие значения:

```env
# Database
POSTGRES_PASSWORD=ваш_сильный_пароль_для_бд

# JWT (сгенерируйте случайную строку минимум 32 символа)
JWT_SECRET_KEY=ваш_случайный_jwt_секрет_минимум_32_символа

# Telegram (если используете)
TELEGRAM_BOT_TOKEN=ваш_токен_от_BotFather
TELEGRAM_CHANNEL_ID=ваш_id_канала
TELEGRAM_WEBHOOK_SECRET=ваш_webhook_секрет
```

### Генерация случайных паролей:

```bash
# Генерация пароля для БД (32 символа)
openssl rand -base64 32

# Генерация JWT секрета (64 символа)
openssl rand -base64 64
```

---

## 🌐 Шаг 4: Настройка домена

### 4.1. Укажите ваш домен в nginx конфигурации

```bash
# Отредактируйте nginx-prod.conf
nano nginx/nginx-prod.conf
```

Замените `yourdomain.com` на ваш реальный домен:

```nginx
server_name yourdomain.com www.yourdomain.com;

ssl_certificate /etc/nginx/ssl/live/yourdomain.com/fullchain.pem;
ssl_certificate_key /etc/nginx/ssl/live/yourdomain.com/privkey.pem;
```

### 4.2. Убедитесь что DNS настроен

Проверьте что ваш домен указывает на IP сервера:

```bash
# Проверка DNS
dig yourdomain.com +short
# или
nslookup yourdomain.com
```

---

## 🔒 Шаг 5: Получение SSL сертификата

### 5.1. Запустите контейнеры без SSL (временно)

```bash
cd /opt/apps/ZasNet

# Создайте директории
mkdir -p nginx/ssl nginx/logs

# Временно запустите только nginx для получения сертификата
# Создайте временный docker-compose для certbot
nano docker-compose.certbot.yml
```

Содержимое `docker-compose.certbot.yml`:

```yaml
services:
  nginx-temp:
    image: nginx:alpine
    container_name: nginx-temp
    ports:
      - "80:80"
    volumes:
      - ./nginx/nginx-http.conf:/etc/nginx/nginx.conf:ro
      - certbot-webroot:/var/www/certbot

  certbot:
    image: certbot/certbot:latest
    volumes:
      - ./nginx/ssl:/etc/letsencrypt
      - certbot-webroot:/var/www/certbot
    command: certonly --webroot --webroot-path=/var/www/certbot --email your-email@example.com --agree-tos --no-eff-email -d yourdomain.com -d www.yourdomain.com

volumes:
  certbot-webroot:
```

### 5.2. Создайте временную nginx конфигурацию

```bash
nano nginx/nginx-http.conf
```

Содержимое:

```nginx
events {
    worker_connections 1024;
}

http {
    server {
        listen 80;
        server_name zasnet.ru zasnet.ru;

        location /.well-known/acme-challenge/ {
            root /var/www/certbot;
        }

        location / {
            return 200 "Server is being configured";
            add_header Content-Type text/plain;
        }
    }
}
```

### 5.3. Получите сертификат

```bash
# Запустите certbot
docker compose -f docker-compose.certbot.yml up certbot

# Остановите временный nginx
docker compose -f docker-compose.certbot.yml down

# Проверьте что сертификат получен
ls -la nginx/ssl/live/yourdomain.com/
```

---

## 🔧 Шаг 6: Настройка API URL в веб клиенте

```bash
# Отредактируйте appsettings.Production.json
nano /opt/apps/ZasNetWebClient/ZasNetWebClient/wwwroot/appsettings.Production.json
```

Установите правильный API URL:

```json
{
  "ApiBaseUrl": "https://yourdomain.com/api"
}
```

---

## 🚀 Шаг 7: Запуск приложения

### 7.1. Соберите и запустите контейнеры

```bash
cd /opt/apps/ZasNet

# Сборка и запуск
docker compose -f docker-compose.prod.yml up -d --build

# Проверка статуса
docker compose -f docker-compose.prod.yml ps
```

### 7.2. Проверьте логи

```bash
# Все логи
docker compose -f docker-compose.prod.yml logs -f

# Логи конкретного сервиса
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml logs -f nginx
```

### 7.3. Проверьте health checks

```bash
# Проверка API health
curl https://yourdomain.com/health

# Должен вернуть: {"status":"healthy","timestamp":"..."}
```

---

## 🔄 Шаг 8: Настройка автообновления SSL

### 8.1. Создайте скрипт обновления

```bash
nano /opt/apps/ZasNet/renew-ssl.sh
```

Содержимое:

```bash
#!/bin/bash
cd /opt/apps/ZasNet
docker compose -f docker-compose.prod.yml exec certbot certbot renew
docker compose -f docker-compose.prod.yml exec nginx nginx -s reload
```

### 8.2. Сделайте скрипт исполняемым

```bash
chmod +x /opt/apps/ZasNet/renew-ssl.sh
```

### 8.3. Добавьте в cron

```bash
# Откройте crontab
crontab -e

# Добавьте строку (проверка каждый день в 3:00)
0 3 * * * /opt/apps/ZasNet/renew-ssl.sh >> /var/log/certbot-renew.log 2>&1
```

---

## 📊 Шаг 9: Мониторинг

### 9.1. Проверка статуса контейнеров

```bash
docker compose -f docker-compose.prod.yml ps
```

### 9.2. Просмотр логов

```bash
# Real-time логи
docker compose -f docker-compose.prod.yml logs -f

# Последние 100 строк
docker compose -f docker-compose.prod.yml logs --tail 100

# Логи nginx
tail -f /opt/apps/ZasNet/nginx/logs/access.log
tail -f /opt/apps/ZasNet/nginx/logs/error.log
```

### 9.3. Использование ресурсов

```bash
docker stats
```

---

## 🔄 Шаг 10: Обновление приложения

```bash
cd /opt/apps/ZasNet

# Получите последние изменения
git pull

cd /opt/apps/ZasNetWebClient
git pull

# Вернитесь в ZasNet
cd /opt/apps/ZasNet

# Пересоберите и перезапустите
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d --build

# Проверьте статус
docker compose -f docker-compose.prod.yml ps
```

---

## 💾 Шаг 11: Backup базы данных

### 11.1. Создайте скрипт backup

```bash
nano /opt/apps/ZasNet/backup-db.sh
```

Содержимое:

```bash
#!/bin/bash
BACKUP_DIR="/opt/apps/ZasNet/backups"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

docker compose -f /opt/apps/ZasNet/docker-compose.prod.yml exec -T postgres \
  pg_dump -U zasnet ZasNet > "$BACKUP_DIR/backup_$DATE.sql"

# Удаление backup старше 7 дней
find $BACKUP_DIR -name "backup_*.sql" -mtime +7 -delete

echo "Backup completed: backup_$DATE.sql"
```

### 11.2. Сделайте скрипт исполняемым

```bash
chmod +x /opt/apps/ZasNet/backup-db.sh
```

### 11.3. Настройте автоматический backup

```bash
# Откройте crontab
crontab -e

# Добавьте строку (backup каждый день в 2:00)
0 2 * * * /opt/apps/ZasNet/backup-db.sh >> /var/log/db-backup.log 2>&1
```

---

## 🔐 Шаг 12: Настройка Telegram webhook

```bash
# Установите webhook для вашего бота
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
  -d "url=https://yourdomain.com/telegram/update" \
  -d "secret_token=<YOUR_WEBHOOK_SECRET>"

# Проверьте webhook
curl "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getWebhookInfo"
```

---

## ✅ Проверка работоспособности

### Проверьте что все работает:

1. **Web Client:** https://yourdomain.com
2. **API Health:** https://yourdomain.com/health
3. **SSL:** Зеленый замок в браузере
4. **Telegram:** Отправьте сообщение боту

### Команды для диагностики:

```bash
# Статус контейнеров
docker compose -f docker-compose.prod.yml ps

# Health checks
curl https://yourdomain.com/health

# SSL сертификат
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com

# Логи
docker compose -f docker-compose.prod.yml logs --tail 50
```

---

## 🆘 Troubleshooting

### Проблема: SSL сертификат не получается

**Решение:**
1. Убедитесь что DNS указывает на ваш сервер
2. Проверьте что порт 80 открыт и не занят
3. Попробуйте получить сертификат вручную:

```bash
docker run -it --rm \
  -v /opt/apps/ZasNet/nginx/ssl:/etc/letsencrypt \
  -v /opt/apps/ZasNet/nginx/certbot-webroot:/var/www/certbot \
  certbot/certbot certonly --webroot \
  --webroot-path=/var/www/certbot \
  --email your-email@example.com \
  --agree-tos \
  -d yourdomain.com -d www.yourdomain.com
```

### Проблема: API возвращает 502 Bad Gateway

**Решение:**
```bash
# Проверьте логи API
docker compose -f docker-compose.prod.yml logs api

# Проверьте что API запущен
docker compose -f docker-compose.prod.yml ps api

# Перезапустите API
docker compose -f docker-compose.prod.yml restart api
```

### Проблема: База данных не доступна

**Решение:**
```bash
# Проверьте статус postgres
docker compose -f docker-compose.prod.yml ps postgres

# Проверьте логи
docker compose -f docker-compose.prod.yml logs postgres

# Проверьте подключение
docker compose -f docker-compose.prod.yml exec postgres psql -U zasnet -d ZasNet -c "SELECT version();"
```

---

## 📝 Чеклист развертывания

- [ ] Сервер подготовлен (Docker установлен, firewall настроен)
- [ ] Проект загружен на сервер
- [ ] .env файл создан и заполнен
- [ ] Домен указывает на IP сервера
- [ ] nginx-prod.conf обновлен с правильным доменом
- [ ] SSL сертификат получен
- [ ] appsettings.Production.json обновлен с правильным API URL
- [ ] Контейнеры запущены
- [ ] Health check проходит
- [ ] Web Client открывается в браузере
- [ ] SSL работает (зеленый замок)
- [ ] Telegram webhook настроен (если используется)
- [ ] Backup настроен
- [ ] Автообновление SSL настроено

---

## 🎉 Готово!

Ваше приложение ZasNet теперь работает в production!

- **URL:** https://yourdomain.com
- **API:** https://yourdomain.com/api
- **Health:** https://yourdomain.com/health

---

## 📞 Поддержка

Если возникли проблемы:
1. Проверьте логи: `docker compose -f docker-compose.prod.yml logs`
2. Проверьте статус: `docker compose -f docker-compose.prod.yml ps`
3. Проверьте SSL: `openssl s_client -connect yourdomain.com:443`
4. Проверьте DNS: `dig yourdomain.com +short`

