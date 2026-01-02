#!/bin/bash
# Скрипт развертывания ZasNet на production сервере

set -e

echo "🚀 ZasNet Production Deployment Script"
echo "======================================="

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Проверка что скрипт запущен в правильной директории
if [ ! -f "docker-compose.prod.yml" ]; then
    echo -e "${RED}❌ Ошибка: docker-compose.prod.yml не найден${NC}"
    echo "Запустите скрипт из директории /opt/apps/ZasNet"
    exit 1
fi

# Проверка наличия .env файла
if [ ! -f ".env" ]; then
    echo -e "${RED}❌ Ошибка: .env файл не найден${NC}"
    echo "Создайте .env файл из .env.production.example"
    echo "cp .env.production.example .env"
    echo "nano .env"
    exit 1
fi

# Проверка Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker не установлен${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Проверки пройдены${NC}"
echo ""

# Спросить пользователя о действии
echo "Выберите действие:"
echo "1) Первичное развертывание (build + up)"
echo "2) Обновление (pull + rebuild + restart)"
echo "3) Перезапуск контейнеров"
echo "4) Остановка"
echo "5) Просмотр логов"
echo "6) Backup базы данных"
read -p "Введите номер действия (1-6): " ACTION

case $ACTION in
    1)
        echo -e "${YELLOW}📦 Выполняется первичное развертывание...${NC}"
        
        # Создание директорий
        mkdir -p nginx/ssl nginx/logs backups
        
        # Остановка старых контейнеров (если есть)
        docker compose -f docker-compose.prod.yml down 2>/dev/null || true
        
        # Сборка и запуск
        docker compose -f docker-compose.prod.yml up -d --build
        
        echo -e "${GREEN}✅ Развертывание завершено${NC}"
        echo ""
        echo "Проверьте статус:"
        docker compose -f docker-compose.prod.yml ps
        ;;
        
    2)
        echo -e "${YELLOW}🔄 Выполняется обновление...${NC}"
        
        # Git pull
        if [ -d ".git" ]; then
            echo "Получение последних изменений из Git..."
            git pull
        fi
        
        # Остановка контейнеров
        docker compose -f docker-compose.prod.yml down
        
        # Пересборка и запуск
        docker compose -f docker-compose.prod.yml up -d --build
        
        echo -e "${GREEN}✅ Обновление завершено${NC}"
        echo ""
        docker compose -f docker-compose.prod.yml ps
        ;;
        
    3)
        echo -e "${YELLOW}🔄 Перезапуск контейнеров...${NC}"
        docker compose -f docker-compose.prod.yml restart
        
        echo -e "${GREEN}✅ Перезапуск завершен${NC}"
        docker compose -f docker-compose.prod.yml ps
        ;;
        
    4)
        echo -e "${YELLOW}🛑 Остановка контейнеров...${NC}"
        docker compose -f docker-compose.prod.yml down
        
        echo -e "${GREEN}✅ Контейнеры остановлены${NC}"
        ;;
        
    5)
        echo -e "${YELLOW}📋 Просмотр логов (Ctrl+C для выхода)${NC}"
        docker compose -f docker-compose.prod.yml logs -f
        ;;
        
    6)
        echo -e "${YELLOW}💾 Создание backup базы данных...${NC}"
        
        BACKUP_DIR="./backups"
        DATE=$(date +%Y%m%d_%H%M%S)
        mkdir -p $BACKUP_DIR
        
        docker compose -f docker-compose.prod.yml exec -T postgres \
          pg_dump -U zasnet ZasNet > "$BACKUP_DIR/backup_$DATE.sql"
        
        echo -e "${GREEN}✅ Backup создан: $BACKUP_DIR/backup_$DATE.sql${NC}"
        
        # Показать размер backup
        ls -lh "$BACKUP_DIR/backup_$DATE.sql"
        ;;
        
    *)
        echo -e "${RED}❌ Неверный выбор${NC}"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}✨ Готово!${NC}"

