# A Piece of Memory 🌻

**Zip Tie Team**
- Alfarel Sandriano Subektiansyah (5053241048) - Game Programmer
- Alief Rif'an Madani (5053241013) - Assets and Level Designer

*Departemen Teknik Informatika*  
*Fakultas Teknologi Informasi dan Komunikasi*  
*Institut Teknologi Sepuluh Nopember*  
*2025*

## 📖 Tentang Game

**A Piece of Memory** adalah game edukasi berbasis desktop yang memadukan elemen petualangan dan simulasi. Game ini menceritakan perjalanan seorang anak dalam menumbuhkan dan merawat sebuah bunga matahari, sambil menghadapi berbagai tantangan yang merepresentasikan kesulitan dalam menjaga lingkungan.

Dalam dunia di mana krisis lingkungan semakin parah, game ini hadir sebagai media pembelajaran interaktif yang mengajarkan pentingnya menjaga keseimbangan antara kemajuan teknologi dan pelestarian lingkungan. Game ini menggambarkan bahwa di balik setiap hal dan tindakan, terdapat memori berharga yang dapat mengubah hidup penjaganya.

## 🎯 User Stories

1. **Sebagai pemain**, saya ingin belajar cara menanam dan merawat bunga agar tumbuh dengan baik
2. **Sebagai pemain**, saya ingin mengalami tantangan dalam merawat lingkungan agar memahami kesulitan yang sebenarnya dalam pelestarian alam
3. **Sebagai pemain**, saya ingin mengetahui manfaat tanaman bagi lingkungan dan manusia agar lebih menghargai alam
4. **Sebagai pemain**, saya ingin melihat hasil dari usaha merawat bunga saya agar merasa puas dan bangga
5. **Sebagai pemain**, saya ingin mendapat pengalaman yang menyenangkan sambil belajar tentang lingkungan agar pendidikan lingkungan terasa menarik


## 🏗️ Arsitektur Sistem

Game ini dibangun menggunakan prinsip pemrograman berorientasi objek dengan struktur kelas sebagai berikut:

### Core Classes
- **Program**: Entry point aplikasi
- **StartScreen**: Layar pembuka dengan animasi dan navigasi
- **GameForm**: Form utama game yang mengelola state dan gameplay

### Game Objects
- **Player**: Karakter pemain dengan sistem pergerakan dan interaksi
- **Flower**: Objek bunga dengan sistem pertumbuhan dan status kesehatan
- **Enemy**: Musuh (robot mini) dengan AI dan sistem pergerakan
- **Boss**: Musuh besar dengan mekanik khusus
- **Projectile**: Sistem proyektil untuk pertahanan
- **Collectible**: Item yang dapat dikumpulkan (air, pupuk)

### Management Systems
- **SpriteManager**: Mengelola aset visual dan animasi
- **AnimatedSpriteManager**: Sistem animasi sprite

### Enumerations
- **FlowerState**: Status pertumbuhan bunga (Healthy, Damaged, Broken, Dead)
- **EnemyType**: Jenis musuh (Slow, Fast, Faster)
- **ProjectileType**: Jenis proyektil (Player, Enemy)
- **CollectibleType**: Jenis item (Water, Fertilizer)

## 🎯 Target Semester

1. Implementasi 6 level utama game
2. Mekanik pertahanan dasar terhadap musuh
3. UI/UX dasar dengan menu utama dan antarmuka level

## 🚀 Target Kompetisi

1. Enhanced graphics dengan animasi pertumbuhan tanaman yang lebih detail
2. Sistem cuaca yang mempengaruhi pertumbuhan tanaman
3. Mode tantangan tambahan dengan variasi musuh yang lebih kompleks
4. Implementasi sistem suara dan musik yang menenangkan
5. Fitur berbagi pencapaian ke media sosial
6. Optimasi performa untuk berbagai spesifikasi sistem

## 🏆 Kompetisi Target

**GELATIK 2025**  
*Penyelenggara*: Kementerian Pendidikan, Kebudayaan, Riset dan Teknologi Republik Indonesia  
*Tanggal*: Menunggu pengumuman resmi (TBA)

## 👥 Tim Pengembang

### 🔧 Game Programmer
**Alfarel Sandriano Subektiansyah**
- Tanggung jawab: Mengembangkan core gameplay, mekanik pertahanan, dan integrasi komponen game
- Skill spesifik: Pemrograman berorientasi objek, algoritma AI untuk perilaku musuh, sistem fisika game

### 🎨 Assets and Level Designer  
**Alief Rif'an Madani**
- Tanggung jawab: Merancang antarmuka pengguna, aset visual, animasi tanaman, dan pengalaman pengguna secara keseluruhan
- Skill spesifik: Desain grafis, animasi 2D, user experience, pengembangan UI responsif

---

*"Di balik setiap hal dan tindakan, terdapat memori berharga yang dapat mengubah hidup penjaganya."*