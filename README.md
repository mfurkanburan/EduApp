# 🎓 EduApp

EduApp, üniversite ortamında öğretim elemanlarının ders materyallerini düzenli bir yapıda sunabileceği ve öğrencilerin ilerlemelerini etkileşimli biçimde takip edebileceği bir **ASP.NET Core MVC** eğitim platformudur.

---

## 📌 Proje Amacı

- C# ve nesne yönelimli programlama temellerini gerçek bir web mimarisinde uygulamak
- ASP.NET Core MVC, Entity Framework Core ve ASP.NET Core Identity yapılarını temiz ve sürdürülebilir bir servis mimarisiyle geliştirmek
- Git / GitHub üzerinden düzenli sürüm kontrolü ve yazılım geliştirme pratikleri kazanmak

---

## 🚀 Özellikler

### 👥 3 Kademeli Rol Mimarisi
- **Öğrenci:** Kurs arama ve filtreleme, kurslara kayıt olma, ders içeriklerini ve materyallerini görüntüleme, ders tamamlama, ilerleme yüzdesi takibi ve otomatik rozet kazanımı.
- **Eğitmen:** Eğitmen panelinden kendi kurslarını oluşturma/yönetme, kapak görseli ve gereksinimler tanımlama, sıralı bölüm/ders ekleme, Quill.js zengin metin editörü ile içerik hazırlama ve ders materyali yükleme.
- **Yönetici:** Yönetici panelinden platform istatistiklerini ve açılan tüm kursları denetleme/yönetme, kategori sistemini yönetme.

### 📚 Kurs, Müfredat ve İçerik Yönetimi
- Başlık, zorluk seviyesi (`Başlangıç`, `Orta`, `İleri`), kapak görseli, açıklama ve gereksinimler listesi ile detaylı kurs tanımlama
- Sıralı bölüm (`CourseSection`) ve ders (`Lesson`) yönetimi
- **Quill.js** zengin metin editörü, **Tailwind Typography** ile okunaklı içerik gösterimi ve **HtmlSanitizer** ile sunucu tarafı XSS koruması
- PDF, DOC, DOCX, PPT, PPTX ve ZIP ders materyali yükleme ve silindiğinde otomatik dosya temizliği
- Kurs adı, kategori ve eğitmen adına göre Türkçe uyumlu büyük-küçük harf duyarsız arama, kategori/seviye filtreleme ve filtre korumalı sayfalama

### 👤 Profil ve Rozet Sistemi
- Profil fotoğrafı yükleme, ad-soyad düzenleme ve parola güncelleme
- Kayıtlı kurslar, tamamlanan kurs/ders sayıları ve genel tamamlanma oranı istatistikleri
- Tamamlanan ders ve kurslara göre otomatik profil rozetleri (`İlk Ders`, `10 Ders`, `İlk Kurs`, `5 Kurs`)

---

## ⚒️ Kullanılan Teknolojiler

| Katman | Teknoloji |
| :--- | :--- |
| **Back-end** | C#, ASP.NET Core MVC (.NET 10) |
| **Veritabanı** | SQLite |
| **Front-end** | Tailwind CSS, Preline UI |
| **Editör & Güvenlik** | Quill.js, HtmlSanitizer |
| **Sürüm Kontrolü** | Git & GitHub |

---

## 🤖 Yapay Zekâ Desteği Alınan Alanlar

Projenin geliştirme sürecinde öğrenme ve kod kalitesini artırma amacıyla aşağıdaki spesifik konularda yapay zekâ araçlarından destek alınmış; tüm kodlar incelenerek projeye kontrollü şekilde entegre edilmiştir:

- **Zengin Metin Editörü Entegrasyonu:** Ders içeriklerinde kullanılan Quill.js editörünün yapılandırılması, form gönderimi ve `HtmlSanitizer` ile XSS güvenliğinin sağlanması.
- **Veritabanı Bağlantıları ve İlişkiler:** Entity Framework Core üzerinde tablo ilişkilerinin (Fluent API), unique index'lerin ve silme davranışlarının yapılandırılması.
- **JavaScript Gerektiren Alanlar:** Dosya boyutu ön kontrolleri, Quill.js başlatma/doğrulama betikleri ve istemci tarafı etkileşimler.
- **Kod Sadeleştirme (Refactoring):** Tekrar eden kodların ortak servis/yardımcılarda toplanması ve mimarinin daha okunabilir hale getirilmesi.

---

## ⚙️ Kurulum ve Çalıştırma

1. Depoyu klonlayın ve proje dizinine geçin:
   ```bash
   git clone https://github.com/mfurkanburan/EduApp.git
   cd EduApp/EduApp.Web
   ```

2. Yönetici (Admin) hesabı için User Secrets bilgilerini tanımlayın:
   ```bash
   dotnet user-secrets set "Admin:Email" "eduapp@teknomosfer.com"
   dotnet user-secrets set "Admin:Password" "Admin123!"
   dotnet user-secrets set "Admin:FirstName" "EduApp"
   dotnet user-secrets set "Admin:LastName" "Yönetici"
   ```

3. NuGet paketlerini geri yükleyin ve veritabanını oluşturun:
   ```bash
   dotnet restore
   dotnet ef database update
   ```

4. Front-end bağımlılıklarını kurup Tailwind CSS'i derleyin:
   ```bash
   npm install
   npm run css:build
   ```

5. Uygulamayı başlatın:
   ```bash
   dotnet run
   ```