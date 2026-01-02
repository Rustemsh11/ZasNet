# 🚀 ZasNet Production - Быстрый старт

## ⚡ За 15 минут до production

### Шаг 1: Подготовка (5 минут)

```bash
# Подключитесь к серверу
ssh root@your-server-ip

# Установите Docker
curl -fsSL https://get.docker.com | sh

# Настройте firewall
ufw allow 22/tcp && ufw allow 80/tcp && ufw allow 443/tcp
ufw enable
```

### Шаг 2: Загрузка проекта (2 минуты)

```bash
# Создайте директорию
mkdir -p /opt/apps && cd /opt/apps

# Клонируйте проекты (или загрузите через SCP)
git clone https://your-repo/ZasNet.git
git clone https://your-repo/ZasNetWebClient.git

cd ZasNet
```

### Шаг 3: Конфигурация (3 минуты)

```bash
# Создайте .env из шаблона
cp env.production.template .env

# Сгенерируйте пароли
echo "POSTGRES_PASSWORD=$(openssl rand -base64 32)" >> .env
echo "JWT_SECRET_KEY=$(openssl rand -base64 64)" >> .env

# Отредактируйте .env
nano .env
```

### Шаг 4: Настройка домена (2 минуты)

```bash
# Обновите nginx конфигурацию
nano nginx/nginx-prod.conf
# Замените yourdomain.com на ваш домен

# Обновите API URL в веб клиенте
nano /opt/apps/ZasNetWebClient/ZasNetWebClient/wwwroot/appsettings.Production.json
# Установите: "ApiBaseUrl": "https://yourdomain.com/api"
```

### Шаг 5: SSL сертификат (3 минуты)

```bash
# Временная конфигурация для certbot
cat > nginx/nginx-http.conf << 'EOF'
events { worker_connections 1024; }
http {
    server {
        listen 80;
        server_name yourdomain.com www.yourdomain.com;
        location /.well-known/acme-challenge/ {
            root /var/www/certbot;
        }
    }
}
EOF

# Запустите временный nginx и получите сертификат
docker run -d --name nginx-temp -p 80:80 \
  -v $(pwd)/nginx/nginx-http.conf:/etc/nginx/nginx.conf \
  -v certbot-webroot:/var/www/certbot nginx:alpine

docker run --rm \
  -v $(pwd)/nginx/ssl:/etc/letsencrypt \
  -v certbot-webroot:/var/www/certbot \
  certbot/certbot certonly --webroot \
  --webroot-path=/var/www/certbot \
  --email your-email@example.com \
  --agree-tos --no-eff-email \
  -d yourdomain.com -d www.yourdomain.com

# Остановите временный nginx
docker stop nginx-temp && docker rm nginx-temp
```

### Шаг 6: Запуск (1 минута)

```bash
# Запустите приложение
chmod +x deploy.sh
./deploy.sh
# Выберите: 1 (Первичное развертывание)

# Или вручную:
docker compose -f docker-compose.prod.yml up -d --build
```

### Шаг 7: Проверка (1 минута)

```bash
# Проверьте статус
docker compose -f docker-compose.prod.yml ps

# Проверьте health
curl https://yourdomain.com/health

# Откройте в браузере
# https://yourdomain.com
```

---

## ✅ Готово!

Ваше приложение работает на:
- **URL:** https://yourdomain.com
- **API:** https://yourdomain.com/api
- **Health:** https://yourdomain.com/health

---

## 🔄 Команды для управления

```bash
# Просмотр логов
docker compose -f docker-compose.prod.yml logs -f

# Перезапуск
docker compose -f docker-compose.prod.yml restart

# Остановка
docker compose -f docker-compose.prod.yml down

# Обновление
git pull
docker compose -f docker-compose.prod.yml up -d --build

# Backup БД
docker compose -f docker-compose.prod.yml exec -T postgres \
  pg_dump -U zasnet ZasNet > backup_$(date +%Y%m%d).sql
```

---

## 📚 Полная документация

- **[PRODUCTION_DEPLOYMENT.md](PRODUCTION_DEPLOYMENT.md)** - Подробная инструкция
- **[PRODUCTION_CHECKLIST.md](PRODUCTION_CHECKLIST.md)** - Чеклист развертывания

---

## 🆘 Проблемы?

1. **SSL не работает:** Проверьте DNS (`dig yourdomain.com`)
2. **502 Bad Gateway:** Проверьте логи API (`docker compose logs api`)
3. **404 Not Found:** Проверьте nginx конфигурацию

**Полный troubleshooting:** См. PRODUCTION_DEPLOYMENT.md

---

**Дата создания:** 2 января 2026  
**Версия:** 1.0

