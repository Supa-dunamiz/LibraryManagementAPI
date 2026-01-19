# LibraryManagementAPI

A secure **ASP.NET Core Web API** for managing library books.  
This API allows users to **add, view, update, and delete books**, with JWT-based authentication and EF Core persistence using MSSQL.  
---

## Features

- **CRUD operations** for books
- **JWT Authentication** for protected endpoints
- **Entity Framework Core** with MSSQL
- **Database seeding** for sample data on startup
- **Search and pagination** for listing books
- **Partial updates** supported — update only the fields you need
- **Centralized error handling** and meaningful HTTP responses
- **Clean architecture** using Repository + Service pattern

---

## Tech Stack

- ASP.NET Core 8.0 Web API  
- Entity Framework Core 8.0  
- MSSQL Server  
- JWT Authentication  
- Swagger for API documentation  

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- SQL Server (LocalDB or full instance)  
- Visual Studio 2022, VS Code, or any preferred IDE  

---

### Setup Instructions

1. **Clone the repository**

```bash
git clone https://github.com/Supa-dunamiz/LibraryManagementAPI.git
cd LibraryManagementAPI
````

2. **Configure the database**

* Open `appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=LibraryDB;Trusted_Connection=True;"
}
```

* Enable seeding of sample books:

```json
"RunSeedOperation": true
```

3. **Apply migrations**

```bash
dotnet ef database update
```

4. **Run the application**

```bash
dotnet run
```

* The API will start at `http://localhost:5164`.

---

### API Endpoints

| Method | Endpoint             | Description                             | Auth Required |
| ------ | -------------------- | --------------------------------------- | ------------- |
| POST   | `/api/auth/register` | Register a new user                     | No            |
| POST   | `/api/auth/login`    | Login and receive JWT token             | No            |
| GET    | `/api/books`         | Retrieve all books                      | Yes           |
| GET    | `/api/books/{id}`    | Get a book by ID                        | Yes           |
| POST   | `/api/books`         | Create a new book                       | Yes           |
| PUT    | `/api/books/{id}`    | Update a book (partial updates allowed) | Yes           |
| DELETE | `/api/books/{id}`    | Delete a book                           | Yes           |

> **JWT Authentication**: Include the token in the header as
> `Authorization: Bearer {your-token}`

---

### Testing the API

1. **Swagger UI**

* Navigate to:

```
http://localhost:5164/swagger
```

* Use `/api/auth/login` to obtain a JWT token.
* Click **Authorize** in Swagger and paste the token.
* Test all protected book endpoints.

2. **Postman / REST Client**

* Use the endpoints above.
* Include `Authorization: Bearer {token}` for protected routes.

