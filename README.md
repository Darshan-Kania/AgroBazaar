# AgroBazaar Platform

A farmer-to-consumer marketplace built with ASP.NET Core 8 MVC, EF Core (MySQL), Bootstrap, and the Repository + Unit of Work pattern.

## What’s new
- User profiles for Farmer and Consumer (edit details, change password)
- Navbar/user dropdown with correct routes and anti-forgery protection
- Fully working Consumer flow: browse products, product details, cart, checkout (COD), orders, order details
- Add to cart fixed across pages (Home, Products, Product Details)
- Product ratings & reviews (only after purchase), average rating display
- Order tracking page (search by order number)
- Printable invoice page for each order
- Performance: caching for login/register lookup, lazy-loading proxies, optimized data access

## Tech stack
- ASP.NET Core 8.0 MVC (Startup.cs pattern)
- Entity Framework Core 8 + MySQL
- ASP.NET Identity (roles: Farmer, Consumer)
- Repository + Unit of Work
- Razor Views + Bootstrap 5 + jQuery

## Features (implemented)
- Authentication: Register, Login, Logout, role-based redirects
- Profile: view/update details, change password
- Consumer: products listing, filters, product details, add to cart, cart quantity update/remove, checkout (COD), orders, order details
- Ratings: add/update rating and comment for purchased products; average rating on details
- Order tracking: public page to check status by order number
- Invoice: printable invoice view with order summary and line items
- Farmer: add/edit/delete products, orders view, status updates
- UX: alerts, badges, breadcrumbs, responsive layout
- Security: antiforgery for forms/AJAX, cookie/session hardening

## Quick start
1) Update appsettings for MySQL connection and JWT settings.
2) Run EF Core migrations and start the app.
3) Register a user as Farmer or Consumer and explore respective dashboards.

## Project structure (highlights)
- Controllers: Auth, Consumer, Farmer, Home
- Data: ApplicationDbContext (lazy-loading proxies, relationships, indexes)
- Models: ApplicationUser, Product, Category, Order(+Items), Cart(+Items), ProductRating
- Repositories: Generic + specific (Products, Orders, Carts, Categories, Users, ProductRatings)
- UnitOfWork: aggregates repositories, transactions, SaveChanges
- Views: Razor pages for Auth, Consumer, Farmer, Home, Shared layout

## Notes
- Anti-forgery token is injected once in layout and sent automatically with all AJAX requests.
- Add-to-cart works on Home, Products, and Product Details; cart badge updates live.
- Ratings limited to users who purchased the product.

## Future enhancements
- Email/SMS notifications
- Online payment integration
- Advanced search and recommendations
- Export/analytics dashboards

---
