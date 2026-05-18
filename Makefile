.PHONY: up up-infra down down-clean build test logs

up:
	docker compose up --build -d

up-infra:
	docker compose up -d postgres-transactions postgres-consolidation rabbitmq redis keycloak

down:
	docker compose down

down-clean:
	docker compose down -v

build:
	dotnet build CashFlow.slnx -c Release

test:
	dotnet test CashFlow.slnx --no-build -c Release

logs:
	docker compose logs -f
