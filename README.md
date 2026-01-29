# 🚀 WordSprint Backend

**WordSprint**, mobil odaklı bir kelime öğrenme uygulaması için geliştirilmiş, ölçeklenebilir ve güvenli bir backend API projesidir.  
Proje, **ASP.NET Core**, **PostgreSQL** ve **JWT tabanlı kimlik doğrulama** kullanılarak geliştirilmiştir.

İngilizce–Türkçe kelime öğrenimi, tekrar sistemi ve quiz tabanlı öğrenme akışını destekleyen tam özellikli bir altyapı sunar.

---

## 🧠 Core Features

### 🔐 Authentication & User Management
- Kullanıcı kaydı (Email doğrulamalı)
- JWT ile giriş (Login)
- Şifremi unuttum & email ile şifre sıfırlama
- Giriş yapmış kullanıcılar için şifre değiştirme
- Token bazlı güvenli yetkilendirme

---

### 📚 Vocabulary Learning
- Kullanıcılara rastgele kelime atama
- Learning list (öğrenilen kelimeler)
- Learned list (öğrenilmiş kelimeler)
- Quiz başarısına göre otomatik:
  - Learning → Learned geçişi

---

### 📝 Quiz System
Desteklenen 4 farklı soru tipi:

- Turkish → English (Yazmalı)
- English → Turkish (Yazmalı)
- Turkish → English (Çoktan Seçmeli)
- English → Turkish (Çoktan Seçmeli)

**Quiz Kuralları:**
- %70+ başarı → kelimeler *learned* olarak işaretlenir
- Learned kelimeler tekrar quizlerine girer
- Tekrar quizlerinde yanlış → kelime tekrar learning’e düşer

---

### 🔁 Repetition System
- Learned kelimeler her zaman tekrar edilebilir
- Yanlış cevap → otomatik relearning
- Kelime bazlı:
  - Doğru / yanlış istatistikleri
  - Başarı oranı takibi

---

### 👤 User Profile
- Profil bilgilerini görüntüleme & güncelleme
- Günlük kelime hedefi
- Tahmini dil seviyesi
- Profil fotoğrafı yükleme
  - Unique hash filename
  - Default avatar desteği
- Profil istatistikleri:
  - Toplam learned / learning
  - Doğru / yanlış sayıları
  - Başarı oranı
  - Bugün öğrenilen kelimeler

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
- **Clean Architecture**
  - Core
  - Application
  - Infrastructure
  - API

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

---

## 👨‍💻 Author

Developed as a learning-focused, production-ready backend for a real-world mobile application.

---
