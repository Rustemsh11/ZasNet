# 🚀 Развертывание ZasNet на zasnet.ru

## ✅ Домен и IP

- **Домен:** zasnet.ru
- **IP сервера:** 80.93.60.244

---

## 📋 Быстрый чеклист

### 1. Настройка DNS (сделайте ДО развертывания)

В панели управления вашего регистратора домена создайте A-записи:

```
Тип   Имя   Значение          TTL
A     @     80.93.60.244      3600
A     www   80.93.60.244      3600
```

Проверка DNS (подождите 5-30 минут после настройки):

```bash
# На вашем локальном компьютере
nslookup zasnet.ru
# Должен вернуть: 80.93.60.244

ping zasnet.ru
# Должен пинговать 80.93.60.244
```

### 2. Подключение к серверу

```bash
ssh root@80.93.60.244
# или
ssh your-user@80.93.60.244
```

### 3. Установка Docker

```bash
# Обновление системы
sudo apt update && sudo apt upgrade -y

# Установка Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Добавление пользователя в группу docker
sudo usermod -aG docker $USER
newgrp docker

# Проверка
docker --version
docker compose version
```

### 4. Настройка Firewall

```bash
# Установка UFW
sudo apt install ufw -y

# ВАЖНО: Сначала разрешите SSH!
sudo ufw allow 22/tcp

# Разрешите HTTP и HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Включите firewall
sudo ufw enable

# Проверьте статус
sudo ufw status
```

Должно показать:

```
Status: active

To                         Action      From
--                         ------      ----
22/tcp                     ALLOW       Anywhere
80/tcp                     ALLOW       Anywhere
443/tcp                    ALLOW       Anywhere
```

### 5. Загрузка проектов на сервер

```bash
# Создайте директорию
sudo mkdir -p /opt/apps
cd /opt/apps

# Клонируйте репозитории (или загрузите через SCP)
sudo git clone https://github.com/your-username/ZasNet.git
sudo git clone https://github.com/your-username/ZasNetWebClient.git

# Установите права
sudo chown -R $USER:$USER /opt/apps

cd /opt/apps/ZasNet
```

### 6. Настройка переменных окружения

```bash
# Создайте .env файл
cp env.production.template .env

# Сгенерируйте безопасные пароли
echo "POSTGRES_PASSWORD=$(openssl rand -base64 32)" > .env
echo "JWT_SECRET_KEY=$(openssl rand -base64 64)" >> .env

# Отредактируйте .env и добавьте Telegram настройки (если нужно)
nano .env
```

Пример `.env` файла:

```env
POSTGRES_PASSWORD=ваш_сгенерированный_пароль
JWT_SECRET_KEY=ваш_сгенерированный_jwt_секрет
TELEGRAM_BOT_TOKEN=ваш_токен_от_BotFather
TELEGRAM_CHANNEL_ID=ваш_id_канала
TELEGRAM_WEBHOOK_SECRET=ваш_webhook_секрет
```

### 7. Получение SSL сертификата

```bash
cd /opt/apps/ZasNet

# Создайте директории
mkdir -p nginx/ssl nginx/logs

# Создайте временную nginx конфигурацию для certbot
cat > nginx/nginx-http.conf << 'EOF'
events {
    worker_connections 1024;
}

http {
    server {
        listen 80;
        server_name zasnet.ru www.zasnet.ru;

        location /.well-known/acme-challenge/ {
            root /var/www/certbot;
        }

        location / {
            return 200 "Server is being configured";
            add_header Content-Type text/plain;
        }
    }
}
EOF

# Запустите временный nginx
docker run -d --name nginx-temp -p 80:80 \
  -v $(pwd)/nginx/nginx-http.conf:/etc/nginx/nginx.conf:ro \
  -v certbot-webroot:/var/www/certbot \
  nginx:alpine

# Получите SSL сертификат
docker run --rm \
  -v $(pwd)/nginx/ssl:/etc/letsencrypt \
  -v certbot-webroot:/var/www/certbot \
  certbot/certbot certonly --webroot \
  --webroot-path=/var/www/certbot \
  --email your-email@example.com \
  --agree-tos --no-eff-email \
  -d zasnet.ru -d www.zasnet.ru

# Остановите временный nginx
docker stop nginx-temp && docker rm nginx-temp

# Проверьте что сертификат получен
sudo ls -la nginx/ssl/live/zasnet.ru/
```

Должны быть файлы:
- `fullchain.pem`
- `privkey.pem`

### 8. Запуск приложения

```bash
cd /opt/apps/ZasNet

# Запустите все контейнеры
docker compose -f docker-compose.prod.yml up -d --build

# Проверьте статус
docker compose -f docker-compose.prod.yml ps
```

Все контейнеры должны быть в состоянии `Up` или `healthy`.

### 9. Проверка работоспособности

```bash
# Проверка Health endpoint
curl https://zasnet.ru/health

# Должен вернуть:
# {"status":"healthy","timestamp":"..."}

# Проверка SSL
curl -I https://zasnet.ru

# Проверка редиректа HTTP -> HTTPS
curl -I http://zasnet.ru

# Просмотр логов
docker compose -f docker-compose.prod.yml logs -f
```

### 10. Настройка Telegram Webhook

```bash
# Установите webhook URL
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
  -d "url=https://zasnet.ru/telegram/update" \
  -d "secret_token=<YOUR_WEBHOOK_SECRET>"

# Проверьте webhook
curl "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getWebhookInfo"
```

### 11. Настройка автоматического Backup

```bash
# Создайте скрипт backup
cat > /opt/apps/ZasNet/backup-db.sh << 'EOF'
#!/bin/bash
BACKUP_DIR="/opt/apps/ZasNet/backups"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

docker compose -f /opt/apps/ZasNet/docker-compose.prod.yml exec -T postgres \
  pg_dump -U zasnet ZasNet > "$BACKUP_DIR/backup_$DATE.sql"

# Удаление backup старше 7 дней
find $BACKUP_DIR -name "backup_*.sql" -mtime +7 -delete

echo "Backup completed: backup_$DATE.sql"
EOF

# Сделайте исполняемым
chmod +x /opt/apps/ZasNet/backup-db.sh

# Добавьте в cron (backup каждый день в 2:00)
crontab -e

# Добавьте строку:
# 0 2 * * * /opt/apps/ZasNet/backup-db.sh >> /var/log/db-backup.log 2>&1
```

### 12. Настройка автообновления SSL

```bash
# Скрипт для обновления SSL
cat > /opt/apps/ZasNet/renew-ssl.sh << 'EOF'
#!/bin/bash
cd /opt/apps/ZasNet
docker compose -f docker-compose.prod.yml exec certbot certbot renew
docker compose -f docker-compose.prod.yml exec nginx nginx -s reload
EOF

chmod +x /opt/apps/ZasNet/renew-ssl.sh

# Добавьте в cron (проверка каждый день в 3:00)
crontab -e

# Добавьте строку:
# 0 3 * * * /opt/apps/ZasNet/renew-ssl.sh >> /var/log/certbot-renew.log 2>&1
```

---

## 🌐 URLs после развертывания

| Сервис | URL | Описание |
|--------|-----|----------|
| Web Client | https://zasnet.ru | Главная страница |
| API | https://zasnet.ru/api | REST API |
| Health Check | https://zasnet.ru/health | Проверка работоспособности |
| Telegram Webhook | https://zasnet.ru/telegram/update | Webhook для бота |

---

## 🔄 Управление

### Просмотр логов

```bash
cd /opt/apps/ZasNet

# Все логи
docker compose -f docker-compose.prod.yml logs -f

# Логи конкретного сервиса
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml logs -f nginx
```

### Перезапуск

```bash
# Перезапуск всех контейнеров
docker compose -f docker-compose.prod.yml restart

# Перезапуск конкретного сервиса
docker compose -f docker-compose.prod.yml restart api
```

### Остановка

```bash
docker compose -f docker-compose.prod.yml down
```

### Обновление

```bash
cd /opt/apps/ZasNet
git pull

cd /opt/apps/ZasNetWebClient
git pull

cd /opt/apps/ZasNet
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d --build
```

### Backup базы данных

```bash
# Ручной backup
docker compose -f docker-compose.prod.yml exec -T postgres \
  pg_dump -U zasnet ZasNet > backup_$(date +%Y%m%d).sql
```

### Восстановление из backup

```bash
cat backup_20260102.sql | \
  docker compose -f docker-compose.prod.yml exec -T postgres \
  psql -U zasnet -d ZasNet
```

---

## 🆘 Troubleshooting

### Проблема: DNS не резолвится

```bash
# Проверьте DNS
nslookup zasnet.ru
dig zasnet.ru +short

# Подождите 5-30 минут после настройки DNS
```

### Проблема: SSL сертификат не получается

```bash
# Убедитесь что DNS работает
ping zasnet.ru

# Убедитесь что порт 80 открыт
sudo ufw status | grep 80

# Проверьте логи временного nginx
docker logs nginx-temp
```

### Проблема: 502 Bad Gateway

```bash
# Проверьте логи API
docker compose -f docker-compose.prod.yml logs api

# Проверьте health
curl https://zasnet.ru/health

# Перезапустите API
docker compose -f docker-compose.prod.yml restart api
```

### Проблема: Контейнер постоянно перезапускается

```bash
# Проверьте логи
docker compose -f docker-compose.prod.yml logs [service-name]

# Проверьте использование ресурсов
docker stats
```

---

## ✅ Финальная проверка

- [ ] DNS настроен (zasnet.ru → 80.93.60.244)
- [ ] Docker установлен на сервере
- [ ] Firewall настроен (порты 22, 80, 443)
- [ ] Проекты загружены на сервер
- [ ] .env файл создан и заполнен
- [ ] SSL сертификат получен
- [ ] Все контейнеры запущены и healthy
- [ ] https://zasnet.ru открывается в браузере
- [ ] https://zasnet.ru/health возвращает "healthy"
- [ ] SSL сертификат валидный (зеленый замок)
- [ ] Telegram webhook настроен (если используется)
- [ ] Backup настроен
- [ ] Автообновление SSL настроено

---

## 🎉 Готово!

Ваше приложение ZasNet работает на:

**🌐 https://zasnet.ru**

---

**IP сервера:** 80.93.60.244  
**Домен:** zasnet.ru  
**Дата развертывания:** _______________  
**Версия:** 1.0

