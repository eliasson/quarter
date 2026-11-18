.PHONY: build gleam test

# Gleam projects that are applications (shared is compiled as a dependency of these).
APPS := app admin

build: gleam
	npm run build

gleam:
	for p in $(APPS); do (cd $$p && gleam build) || exit 1; done

test:
	for p in shared $(APPS); do (cd $$p && gleam test) || exit 1; done
