## Proje Hakkında
**Medinova DbFirst**, sağlık kurumları için geliştirilmiş; randevu yönetimi, doktor arama, içerik yönetimi ve yapay zeka destekli özellikleri tek bir sistemde birleştiren bir web uygulamasıdır.

Hasta, doktor ve yönetici rolleri için ayrı paneller sunar. Amaç; randevu süreçlerini dijitalleştirmek, doktor ve hizmetlere erişimi kolaylaştırmak ve kullanıcı deneyimini iyileştirmektir.

---

## Öne Çıkan Özellikler

### Randevu Yönetimi
- Bölüm, doktor, tarih ve saat seçimine dayalı randevu oluşturma  
- Aynı doktor ve saat için çakışma kontrolü  
- Randevu sonrası bildirim ve e-posta gönderimi  

### Doktor Arama ve Keşif
- Doktor adı ve bölüm bilgisine göre arama  
- Yazım hatalarına toleranslı arama deneyimi  
- En uygun doktorların öne çıkarılması  

### Yapay Zeka Destekli Özellikler
- **OpenAI entegrasyonu** ile genel sağlık danışmanlığı  
- Kullanıcının semptomlarına göre **bölüm önerileri**  
- Hasta panelinde AI destekli soru–cevap deneyimi  

### Kullanıcı Rolleri
- **Admin**: İçerik yönetimi ve sistem takibi  
- **Doctor**: Profil ve randevu süreçlerinin yönetimi  
- **Patient**: Randevu alma, doktor arama ve AI destekli danışmanlık  

### İçerik Yönetimi
- Banner, hizmetler ve bilgilendirici içeriklerin yönetimi  
- Yönetici paneli üzerinden dinamik güncelleme  

---

## Arama Altyapısı
Doktor arama işlemlerinde hızlı ve esnek bir deneyim sunmak amacıyla **Elasticsearch** kullanılmıştır.  
Elasticsearch devre dışı olduğunda sistem, veritabanı üzerinden arama yapmaya devam eder.

---

## Loglama ve İzleme
- Uygulama genelinde loglama yapılmaktadır  
- Teknik loglar dosya tabanlı olarak tutulur  
- Ortam yapılandırmasına bağlı olarak merkezi loglama desteği sağlanabilir  
- Kullanıcı aksiyonları ayrıca veritabanında kayıt altına alınır  

---

## Mimari Yapı
- ASP.NET MVC tabanlı geliştirilmiştir  
- Entity Framework 6 (DbFirst) yaklaşımı kullanılmıştır  
- Admin, Doctor ve Patient alanları **Areas** yapısı ile ayrılmıştır  
- Arama, yapay zeka, loglama ve e-posta işlemleri servis katmanında toplanmıştır  

---

## Kullanılan Teknolojiler
- .NET Framework 4.8 / ASP.NET MVC  
- Entity Framework 6 (DbFirst)  
- Elasticsearch  
- Serilog  
- OpenAI API  
- MailKit  
- AWS S3  
- ML.NET  

---

## Genel Amaç
**Medinova DbFirst**, sağlık kurumlarının randevu ve içerik süreçlerini tek bir platformda toplarken,  
yapay zeka destekli özellikler ile kullanıcı deneyimini güçlendirmeyi hedefleyen modern bir web uygulamasıdır.

<img width="1920" height="1080" alt="Ekran görüntüsü 2026-01-28 165847" src="https://github.com/user-attachments/assets/586c9ba3-d310-4bf0-bfe7-650fd116379c" />

<img width="1915" height="753" alt="Ekran görüntüsü 2026-01-28 172751" src="https://github.com/user-attachments/assets/74872cbc-b999-4c01-be08-4096d58b2106" />

<img width="1895" height="865" alt="Ekran görüntüsü 2026-01-28 173929" src="https://github.com/user-attachments/assets/0c291a63-f291-416f-81ae-0f551cff0503" />

<img width="1888" height="857" alt="Ekran görüntüsü 2026-01-28 174242" src="https://github.com/user-attachments/assets/ca2b7c92-2416-423e-9de4-12a679a7dffa" />

<img width="1870" height="865" alt="Ekran görüntüsü 2026-01-28 175030" src="https://github.com/user-attachments/assets/998624df-03f6-409b-a3f6-b768231c0fbe" />

<img width="1250" height="870" alt="Ekran görüntüsü 2026-01-28 183118" src="https://github.com/user-attachments/assets/fcaeb917-afa5-42ec-8261-e6aa0ea6de76" />
