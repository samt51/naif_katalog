# Atasay customer frontend

Authorized scope: independent ASP.NET Core MVC customer frontend, Atasay branding and official editorial imagery, same existing API. Preserve the original frontend and all existing database files. No backend or schema changes. No administrative controllers, views or mutations in the new frontend.

Implementation:
1. Create frontends/atasay_katalog with its own project, configuration, namespace and publish profile. Exclude frontends/** from the original project's build and publish.
2. Copy customer catalog, details, cart, customer order functionality and required API queries only. Successful login always leads to the customer homepage. Remove administrative order actions and role-based access to other customers' orders.
3. Replace visible branding, PDF branding, storage keys, logo, favicon and layout. Build responsive editorial homepage and login using local Atasay assets. Keep existing API endpoints and existing order persistence behavior; copy no live data or SMTP credentials.
4. Resolve shared product images via an allowlisted, same-origin image proxy because the independent frontend has no existing catalog image files. Never allow arbitrary proxy URLs.
5. Verify original and new builds/publish isolation, customer login and catalog against a local fixture API, missing admin routes, responsive rendering, image loading and cart interactions. No real customer login, order creation or email sends in production.

Important cases: API unavailable; authenticated admin-role account still only sees customer UI; malformed image path; mobile header overflow; no product/category records. Validate these in smoke tests and browser checks.

Assets: public Atasay website/CDN, recorded in the new project's ASSETS.md. Customer product photos remain supplied by the existing catalog service.
