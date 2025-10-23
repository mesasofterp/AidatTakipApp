# StudentApp - .NET 8 MVC Web Application

A complete .NET 8 MVC web application for managing students with Entity Framework Core and SQLite database.

## Features

- ✅ Full CRUD operations for Student entity
- ✅ Entity Framework Core with SQLite database
- ✅ Clean architecture with Service layer
- ✅ Bootstrap 5 responsive UI
- ✅ Form validation with DataAnnotations
- ✅ Dependency injection
- ✅ Auto-migration on startup

## Student Entity Properties

- **Id**: Primary key (int)
- **FirstName**: Student's first name (string, required, max 50 chars)
- **LastName**: Student's last name (string, required, max 50 chars)
- **Email**: Student's email (string, required, unique, max 100 chars)
- **EnrollmentDate**: Date when student enrolled (DateTime, required)

## Project Structure

```
StudentApp/
├── Controllers/
│   └── StudentController.cs
├── Models/
│   └── Student.cs
├── Services/
│   ├── IStudentService.cs
│   └── StudentService.cs
├── Data/
│   └── AppDbContext.cs
├── Views/
│   ├── Student/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Details.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _ValidationScriptsPartial.cshtml
├── Program.cs
├── appsettings.json
└── StudentApp.csproj
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Running the Application

1. **Navigate to the project directory:**
   ```bash
   cd StudentApp
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Open your browser and navigate to:**
   ```
   https://localhost:5001
   ```

### Database

- The application uses SQLite database (`StudentApp.db`)
- Database is automatically created on first run
- No manual migration commands needed

## Available Routes

- `/Student` or `/` - List all students
- `/Student/Create` - Add new student
- `/Student/Details/{id}` - View student details
- `/Student/Edit/{id}` - Edit student
- `/Student/Delete/{id}` - Delete student

## Features Implemented

### ✅ Core Requirements
- [x] .NET 8 MVC application
- [x] Entity Framework Core with SQLite
- [x] Student entity with all required properties
- [x] Clean architecture with proper folder structure
- [x] Service layer with dependency injection
- [x] Full CRUD operations
- [x] Bootstrap 5 styling

### ✅ Bonus Features
- [x] DataAnnotations validation
- [x] Auto-migration on startup
- [x] Success/error messages with TempData
- [x] Responsive design
- [x] Font Awesome icons
- [x] Email validation
- [x] Unique email constraint
- [x] Professional UI/UX

## Usage

1. **View Students**: Navigate to the home page to see all students
2. **Add Student**: Click "Add New Student" button and fill the form
3. **Edit Student**: Click the edit button next to any student
4. **Delete Student**: Click the delete button and confirm
5. **View Details**: Click the view button to see student information

## Technology Stack

- **.NET 8** - Framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core** - ORM
- **SQLite** - Database
- **Bootstrap 5** - CSS framework
- **jQuery** - JavaScript library
- **Font Awesome** - Icons

## Development Notes

- The application follows clean architecture principles
- All database operations are handled through the service layer
- Form validation is implemented both client-side and server-side
- The database is automatically created and migrated on startup
- All CRUD operations include proper error handling
