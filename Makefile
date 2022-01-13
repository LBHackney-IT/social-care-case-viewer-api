.PHONY: setup
setup:
	docker-compose build

.PHONY: build
build:
	docker-compose build base-api

.PHONY: serve
serve:
	docker-compose build social-care-case-viewer-api && docker-compose up social-care-case-viewer-api

.PHONY: start-local-dev-dbs
start-local-dev-dbs:
	docker-compose up -d sccv-api-postgresql && docker-compose up -d sccv-api-mongo-db

.PHONY: shell
shell:
	docker-compose run base-api bash

.PHONY: test
test:
	docker-compose build social-care-case-viewer-api-test && docker-compose up social-care-case-viewer-api-test

.PHONY: start-test-dbs
start-test-dbs:
	docker-compose up -d sccv-api-test-postgresql && docker-compose up -d sccv-api-test-mongo-db && docker-compose up -d sccv-historical-data-test-postgresql

.PHONY: restart-test-pg-db
restart-test-pg-db:
	docker stop $$(docker ps -q --filter ancestor=sccv-api-test-postgresql -a)
	-docker rm $$(docker ps -q --filter ancestor=sccv-api-test-postgresql -a)
	docker rmi sccv-api-test-postgresql
	docker-compose up -d sccv-api-test-postgresql

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format
