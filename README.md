# 🚄 KuwGo - نظام إدارة وحجز القطارات

<div align="center">

![KuwGo Logo](https://img.shields.io/badge/KuwGo-Train%20Management-blue?style=for-the-badge&logo=train)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)](https://docs.microsoft.com/en-us/aspnet/core/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**نظام شامل لإدارة وحجز رحلات القطارات في الكويت**

[المميزات](#-المميزات-الرئيسية) • [التقنيات](#️-التقنيات-المستخدمة) • [التثبيت](#-التثبيت-والتشغيل) • [الهيكلة](#-الهيكلة-المعمارية) • [لقطات الشاشة](#-لقطات-الشاشة)

</div>

---

## 📋 نظرة عامة

**KuwGo** هو نظام متكامل لإدارة وحجز رحلات القطارات، مصمم خصيصاً للسوق الكويتي. يوفر النظام تجربة مستخدم سلسة للركاب ولوحة تحكم قوية للمسؤولين، مع دعم كامل للغة العربية والتتبع الحي للقطارات.

### 🎯 الأهداف الرئيسية

- **تبسيط عملية الحجز**: واجهة سهلة وسريعة لحجز التذاكر
- **إدارة فعالة**: لوحة تحكم شاملة لإدارة الرحلات والقطارات
- **تتبع حي**: متابعة موقع القطارات في الوقت الفعلي
- **تنبيهات ذكية**: إشعارات SMS تلقائية للتأخيرات والتحديثات

---

## ✨ المميزات الرئيسية

### 👥 للمستخدمين

- 🎫 **حجز التذاكر**: نظام حجز سهل وسريع مع خيارات دفع متعددة
- 🗺️ **التتبع الحي**: متابعة موقع القطار على الخريطة في الوقت الفعلي
- 📱 **إشعارات SMS**: تنبيهات فورية عن التأخيرات والتحديثات
- 🎁 **نظام النقاط**: اكسب نقاط مع كل حجز واستبدلها بخصومات
- 📄 **تذاكر PDF**: تحميل التذاكر بصيغة PDF احترافية
- 🔍 **بحث متقدم**: ابحث عن الرحلات حسب المحطة والتاريخ

### 👨‍💼 للمسؤولين

- 📊 **لوحة تحكم شاملة**: إحصائيات وتقارير في الوقت الفعلي
- 🚂 **إدارة القطارات**: إضافة وتعديل وحذف القطارات
- 🗺️ **إدارة المحطات**: إدارة كاملة للمحطات ومواقعها
- 🎫 **إدارة الرحلات**: جدولة الرحلات مع **حساب تلقائي لوقت الوصول**
- 📨 **إرسال الإشعارات**: إشعارات SMS جماعية للركاب
- 🎮 **محاكي الرحلات**: محاكاة حركة القطارات للاختبار

### 🧠 مميزات تقنية متقدمة

- ⏱️ **حساب ذكي للمسافات**: استخدام معادلة Haversine لحساب المسافات الفعلية
- 🚄 **سرعة ثابتة**: 300 كم/ساعة مع احتساب محطات التوقف (10 دقائق لكل محطة)
- 🏗️ **Clean Architecture**: هيكلة معمارية احترافية مع فصل الطبقات
- 🔄 **CQRS Pattern**: استخدام MediatR لفصل القراءة والكتابة
- 🗺️ **AutoMapper**: تحويل تلقائي بين الكيانات والـ DTOs
- 🔐 **ASP.NET Identity**: نظام مصادقة وتفويض آمن

---

## 🛠️ التقنيات المستخدمة

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C# 12
- **Database**: SQLite (قابل للترقية لـ SQL Server)
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity
- **Real-time**: SignalR
- **Patterns**: 
  - Clean Architecture
  - CQRS (MediatR)
  - Repository Pattern
  - Dependency Injection

### Frontend
- **UI Framework**: Bootstrap 5
- **Icons**: Font Awesome 6
- **Maps**: Leaflet.js
- **Charts**: Chart.js
- **Real-time Updates**: SignalR Client
- **Styling**: Custom CSS with RTL support

### External Services
- **SMS**: Twilio API
- **PDF Generation**: QuestPDF
- **Email**: SMTP (Mock implementation)

---

## 🏗️ الهيكلة المعمارية

المشروع يتبع **Clean Architecture** مع فصل واضح للطبقات:

```
TrainTracking/
├── TrainTracking.Domain/          # الكيانات الأساسية والـ Enums
│   └── Entities/
│       ├── Trip.cs
│       ├── Train.cs
│       ├── Station.cs
│       ├── Booking.cs
│       └── Notification.cs
│
├── TrainTracking.Application/     # منطق الأعمال والـ Use Cases
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Features/                  # CQRS Queries & Commands
│   ├── Interfaces/                # Contracts
│   ├── Mappings/                  # AutoMapper Profiles
│   └── Services/                  # Business Services
│
├── TrainTracking.Infrastructure/  # تنفيذ الخدمات الخارجية
│   ├── Persistence/               # Database Context & Migrations
│   ├── Repositories/              # Repository Implementations
│   └── Services/                  # External Services (SMS, Email)
│
└── TrainTracking.Web/             # طبقة العرض (MVC)
    ├── Controllers/
    ├── Views/
    ├── wwwroot/
    └── Hubs/                      # SignalR Hubs
```

### 🔄 تدفق البيانات (CQRS)

```
User Request → Controller → Mediator → Query/Command Handler → Repository → Database
                                ↓
                            AutoMapper
                                ↓
                              DTO → View
```

---

## 🚀 التثبيت والتشغيل

### المتطلبات الأساسية

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) أو [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### خطوات التثبيت

1. **استنساخ المشروع**
```bash
git clone https://github.com/YOUR_USERNAME/KuwGo.git
cd KuwGo
```

2. **استعادة الحزم**
```bash
dotnet restore
```

3. **تحديث قاعدة البيانات**
```bash
cd TrainTracking.Web
dotnet ef database update --project ../TrainTracking.Infrastructure
```

4. **تشغيل المشروع**
```bash
dotnet run --project TrainTracking.Web
```

5. **فتح المتصفح**
```
https://localhost:5244
```

### 🔑 بيانات الدخول الافتراضية

**حساب الأدمن:**
- البريد الإلكتروني: `admin@train.com`
- كلمة المرور: `Admin123!`

---

## ⚙️ الإعدادات

### إعداد Twilio للـ SMS (اختياري)

في ملف `appsettings.json`:

```json
{
  "TwilioSettings": {
    "AccountSid": "YOUR_ACCOUNT_SID",
    "AuthToken": "YOUR_AUTH_TOKEN",
    "PhoneNumber": "YOUR_TWILIO_PHONE"
  }
}
```

### إعداد قاعدة البيانات

لتغيير قاعدة البيانات من SQLite إلى SQL Server، عدّل `Program.cs`:

```csharp
builder.Services.AddDbContext<TrainTrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## 📸 لقطات الشاشة

### الصفحة الرئيسية
![Home Page](screenshots/home.png)

### لوحة التحكم
![Admin Dashboard](screenshots/admin-dashboard.png)

### التتبع الحي
![Live Tracking](screenshots/live-tracking.png)

### حجز التذاكر
![Booking](screenshots/booking.png)

---

## 🗺️ خريطة الطريق

- [x] نظام الحجز الأساسي
- [x] لوحة تحكم الأدمن
- [x] التتبع الحي
- [x] إشعارات SMS
- [x] نظام النقاط
- [x] Clean Architecture
- [x] حساب تلقائي لوقت الوصول
- [ ] دعم الدفع الإلكتروني (K-Net)
- [ ] تطبيق موبايل (Flutter)
- [ ] API Documentation (Swagger)
- [ ] Unit Tests
- [ ] Docker Support

---

## 🤝 المساهمة

المساهمات مرحب بها! إذا كنت تريد المساهمة:

1. Fork المشروع
2. أنشئ فرع للميزة (`git checkout -b feature/AmazingFeature`)
3. Commit التغييرات (`git commit -m 'Add some AmazingFeature'`)
4. Push للفرع (`git push origin feature/AmazingFeature`)
5. افتح Pull Request

---

## 📝 الترخيص

هذا المشروع مرخص تحت رخصة MIT - انظر ملف [LICENSE](LICENSE) للتفاصيل.

---

## 👨‍💻 المطور

**Mahmoud**
- GitHub: [@YOUR_USERNAME](https://github.com/YOUR_USERNAME)
- LinkedIn: [Your LinkedIn](https://linkedin.com/in/YOUR_PROFILE)

---

## 🙏 شكر وتقدير

- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Bootstrap](https://getbootstrap.com/)
- [Font Awesome](https://fontawesome.com/)
- [Leaflet.js](https://leafletjs.com/)
- [Chart.js](https://www.chartjs.org/)
- [QuestPDF](https://www.questpdf.com/)
- [Twilio](https://www.twilio.com/)

---

<div align="center">

**صُنع بـ ❤️ في الكويت**

⭐ إذا أعجبك المشروع، لا تنسى إعطائه نجمة!

</div>
