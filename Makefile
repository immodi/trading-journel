# TradingJournel — common tasks.
#
# The default target publishes a self-contained, single-file Linux binary into
# ./publish. Run `make help` to list everything else.

PROJECT   := backend/Api/Api.csproj
OUTPUT    := ./publish
RID       ?= linux-x64
CONFIG    ?= Release

.DEFAULT_GOAL := publish
.PHONY: help publish build test clean

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-10s\033[0m %s\n", $$1, $$2}'

publish: ## Publish a self-contained single-file build into ./publish
	dotnet publish $(PROJECT) \
	  -c $(CONFIG) -r $(RID) --self-contained true \
	  -p:PublishSingleFile=true \
	  -p:IncludeNativeLibrariesForSelfExtract=true \
	  -o $(OUTPUT)

build: ## Build the solution in $(CONFIG)
	dotnet build $(PROJECT) -c $(CONFIG)

test: ## Run the backend test projects
	dotnet test -c $(CONFIG)

clean: ## Remove build output, including ./publish
	dotnet clean $(PROJECT) -c $(CONFIG) || true
	rm -rf $(OUTPUT)
