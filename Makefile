.PHONY: help build up down logs shell clean restart dev prod backup restore

# Default target
help:
	@echo "ChoreWars Docker Commands"
	@echo "========================="
	@echo ""
	@echo "Production:"
	@echo "  make build     - Build Docker image"
	@echo "  make up        - Start application (production)"
	@echo "  make down      - Stop application"
	@echo "  make restart   - Restart application"
	@echo "  make logs      - View logs (follow mode)"
	@echo ""
	@echo "Development:"
	@echo "  make dev       - Start application (development mode)"
	@echo "  make dev-down  - Stop development application"
	@echo ""
	@echo "Maintenance:"
	@echo "  make shell     - Open shell in running container"
	@echo "  make clean     - Remove containers, volumes, and images"
	@echo "  make ps        - Show running containers"
	@echo "  make health    - Check container health status"
	@echo ""
	@echo "Database:"
	@echo "  make backup    - Backup database"
	@echo "  make restore   - Restore database from backup"
	@echo ""

# Production commands
build:
	@echo "Building Docker image..."
	docker-compose build

up:
	@echo "Starting ChoreWars (Production)..."
	docker-compose up -d
	@echo "Application running at http://localhost:5287"

down:
	@echo "Stopping ChoreWars..."
	docker-compose down

restart:
	@echo "Restarting ChoreWars..."
	docker-compose restart

logs:
	@echo "Viewing logs (Ctrl+C to exit)..."
	docker-compose logs -f

ps:
	@echo "Container status:"
	docker-compose ps

# Development commands
dev:
	@echo "Starting ChoreWars (Development)..."
	docker-compose -f docker-compose.dev.yml up -d
	@echo "Application running at http://localhost:5287"

dev-down:
	@echo "Stopping ChoreWars (Development)..."
	docker-compose -f docker-compose.dev.yml down

dev-logs:
	@echo "Viewing development logs (Ctrl+C to exit)..."
	docker-compose -f docker-compose.dev.yml logs -f

# Maintenance commands
shell:
	@echo "Opening shell in container..."
	docker exec -it chorewars-web bash

clean:
	@echo "Cleaning up Docker resources..."
	docker-compose down -v --rmi all
	@echo "Cleanup complete!"

health:
	@echo "Checking container health..."
	@docker inspect --format='{{.State.Health.Status}}' chorewars-web 2>/dev/null || echo "Container not running"

# Database commands
backup:
	@echo "Creating database backup..."
	@mkdir -p backups
	@docker exec chorewars-web tar -czf /tmp/backup.tar.gz /app/data
	@docker cp chorewars-web:/tmp/backup.tar.gz ./backups/backup-$$(date +%Y%m%d-%H%M%S).tar.gz
	@echo "Backup created in ./backups/"

restore:
	@echo "Restoring database from backup..."
	@echo "Available backups:"
	@ls -lh backups/ 2>/dev/null || echo "No backups found"
	@echo ""
	@echo "To restore, run: tar -xzf backups/[filename] -C ./"

# Quick rebuild and restart
rebuild:
	@echo "Rebuilding and restarting..."
	docker-compose down
	docker-compose build --no-cache
	docker-compose up -d
	@echo "Application restarted at http://localhost:5287"
