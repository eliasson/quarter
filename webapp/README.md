# Quarter web client

The quarter web client consists of single-page applications implemented in
Gleam and using Lustre as the web framework.

## Projects

- `app/` - The time tracking application, served on `/ui`.
- `admin/` - The administration application (e.g. managing users), served
  on `/admin`.
- `shared/` - Gleam code, CSS and fonts shared by both applications. Its
  Gleam modules are namespaced under `quarter_shared/` and it is used as a
  path dependency by `app` and `admin`.

Each project is its own Gleam project. Vite and npm are shared across them all.

## Building

From this directory:

```
npm ci
make build   # gleam build in app and admin, then bundle both with Vite
make test    # gleam test in shared, app and admin
```

The bundles are written to `dist/ui/` and `dist/admin/` in the repository root,
from where the backend serves them.

To bundle a single application use `npm run build:app` or `npm run build:admin`.
To rebundle on changes use`npm run watch` (app) or `npm run watch:admin` (admin).
