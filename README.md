# 🚀 WordSprint Backend

**WordSprint** is a scalable and secure backend API developed for a mobile-focused vocabulary learning application.

Built with **ASP.NET Core 8**, **PostgreSQL**, and **JWT-based authentication**, it provides a full-featured infrastructure for quiz-driven language learning and repetition workflows.

---

## 🧠 Core Features

### 🔐 Authentication & User Management
- User registration with email confirmation
- JWT-based login
- Forgot password & email-based password reset
- Authenticated password change
- Token-based secure authorization

---

### 📚 Vocabulary Learning
- Random word assignment per user
- Learning list (words in progress)
- Learned list (mastered words)
- Automatic state transition:
  - Learning → Learned based on quiz success

---

### 📝 Quiz System

Supports 4 different question types:

- Turkish → English (Typing)
- English → Turkish (Typing)
- Turkish → English (Multiple Choice)
- English → Turkish (Multiple Choice)

**Quiz Rules:**
- ≥ 70% success rate → words marked as *learned*
- Learned words enter repetition mode
- Wrong answer in repetition → word returns to learning

---

### 🔁 Repetition System
- Learned words can always be retested
- Wrong answer triggers automatic relearning
- Word-level tracking:
  - Correct / wrong counts
  - Success rate statistics

---

### 👤 User Profile
- View & update profile information
- Daily word goal
- Estimated language level
- Profile photo upload
  - Unique hashed filename
  - Default avatar support
- Profile statistics:
  - Total learned / learning words
  - Correct / wrong counts
  - Success rate
  - Words learned today

---

## 🧪 Testing Layer

The project includes both **Unit Tests** and **Integration Tests**.

### ✅ Unit Tests
- Business logic testing for quiz scoring
- Success rate validation (≥ 70% rule)
- Case-insensitive and trimmed input handling
- Mode-based answer validation
- Edge cases:
  - Missing words
  - Duplicate word IDs
  - Empty/null submissions

Unit tests isolate domain logic and ensure scoring rules remain stable during refactoring.

---

### 🔬 Integration Tests

- SQLite in-memory database provider
- Real EF Core execution
- Verifies:
  - `IsLearned` flag updates
  - Correct/Wrong counters
  - `UserDailyActivity` creation
  - Database persistence via `SaveChanges`

This ensures both business logic and database side effects behave correctly.

---

## 🛠️ Tech Stack

- **ASP.NET Core 8 (Web API)**
- **Entity Framework Core (Code First)**
- **PostgreSQL**
- **ASP.NET Core Identity**
- **JWT Authentication**
- **Gmail SMTP**
  - Email confirmation
  - Password reset
- **SQLite (for Integration Testing)**
- **xUnit + FluentAssertions**
- **Clean Architecture**
  - Core
  - Application
  - Infrastructure
  - API
  - Tests

---

## 🔑 Authentication Flow

1. User registers → Confirmation email sent  
2. User confirms email  
3. Login → JWT token issued  
4. Protected endpoints require JWT token  

---

## 📬 Email Features

- Registration email confirmation
- Password reset via email
- Gmail SMTP (App Password)
- HTML email templates

---

## 🔒 Security Notes

- Passwords handled by ASP.NET Identity
- JWT tokens securely generated & validated
- Email existence is not leaked in reset flow
- Uploaded images use unique hashed filenames
- Secrets are NOT stored in source control

---

## 🚧 Roadmap

- Flutter mobile application
- Deep linking for password reset
- Smart spaced repetition algorithm
- Push notifications & reminders
- Subscription & offline mode support
- CI/CD pipeline integration

---

## 👨‍💻 Author

Developed as a production-ready backend system for a real-world language learning application.

---
