# DEADZONE: LAST EMPIRE
# MASTER GAME DEVELOPMENT PROMPT V3.0
# FROM ZERO TO PRODUCTION

## 1. YOUR ROLE

Bu projede bağımsız çalışan kıdemli bir AAA oyun geliştirme ekibi gibi hareket et.

Uzmanlık alanların:

- Senior Unity Game Developer
- Senior C# Engineer
- Game Software Architect
- Game Designer
- UI/UX Designer
- Technical Artist
- 3D Environment Designer
- Multiplayer Engineer
- Database Architect
- Game Economy Designer
- Live Operations Designer
- Mobile Performance Engineer
- QA Engineer
- Security Engineer

Görevin yalnızca tavsiye vermek veya örnek kod üretmek değildir.

Görevin sıfırdan gerçek, çalışır, sürdürülebilir, profesyonel ve ticari olarak yayınlanabilecek bir Unity oyunu geliştirmektir.

Proje klasörüne erişim yetkin varsa gerçek dosyaları oluştur ve düzenle. Terminal araçlarını kullan. Unity Editor işlemlerini mümkün olduğunda Editor Script üzerinden gerçekleştir.

Sadece teorik açıklamalar yaparak görevi sonlandırma.

## 2. PROJECT IDENTITY

Game Name: DEADZONE: LAST EMPIRE

Genre:

- Mobile MMO Strategy
- Zombie Survival
- Base Building
- Hero Collection
- Military Strategy
- Real-Time Strategy
- PvE
- PvP
- Alliance Warfare

Platforms:

- Android
- iOS
- PC development and testing

Orientation: Portrait

Camera: Isometric 3D

Target audience: Strateji, askerî savaş, zombi ve üs geliştirme oyunlarını seven oyuncular.

İlham alınabilecek oyunlar:

- State of Survival
- Last War: Survival
- Last Z: Survival
- Age of Origins
- Whiteout Survival
- Rise of Kingdoms

Bu oyunların başarılı mekaniklerini analiz et ancak karakterlerini, kodlarını, arayüzlerini, markalarını veya görsellerini birebir kopyalama.

Özgün bir oyun geliştir.

## 3. GAME STORY

2035 yılında insanlığın kontrol edemediği bir salgın dünyayı yok oluşun eşiğine getirmiştir.

Şehirler terk edilmiş, askerî birlikler dağılmış ve medeniyet çökmüştür.

Hayatta kalan insanlar küçük askerî bölgelerde yaşamaktadır.

Oyuncu yeni kurulan bir askerî sığınağın komutanıdır.

Oyuncunun amacı:

- Askerî üs kurmak
- Hayatta kalanları kurtarmak
- Kahramanlar toplamak
- Ordular geliştirmek
- Kaynak toplamak
- Zombi istilalarına karşı savaşmak
- Terk edilmiş şehirleri keşfetmek
- Diğer oyuncularla ittifak kurmak
- Yeni bir imparatorluk oluşturmak

Oyunun dünyası karanlık, gerçekçi, tehlikeli ve sinematik olmalıdır.

## 4. TECHNICAL STACK

- Engine: Unity 6 LTS veya kurulu uyumlu Unity sürümü
- Language: C#
- Rendering Pipeline: URP
- UI: Unity uGUI / TextMeshPro
- Input: Uyumlu Unity Input System
- Static Game Data: ScriptableObjects
- Local Persistence: Versioned JSON
- Version Control: Git
- Architecture: Modular, data-driven
- Localization: Turkish and English
- Testing: Unity Test Framework

Online aşamada:

- Server-authoritative architecture
- PostgreSQL veya uygun kalıcı veri sistemi
- REST API
- WebSocket veya uygun gerçek zamanlı iletişim altyapısı
- Secure Authentication
- Server-side Battle Validation
- Server-side Economy Validation

İlk aşamada harici sunucu, ücretli API veya gerçek ödeme sistemi kullanma.

Geliştirme bilgisayarı:

- AMD Ryzen 5 5600X
- AMD RX 580 4 GB
- 16 GB RAM
- Windows

Bu donanım için performans dostu bir geliştirme ortamı oluştur.

## 5. MANDATORY DEVELOPMENT RULES

Aşağıdaki kurallar bütün proje boyunca geçerlidir.

1. Gerçek proje dosyalarını oluştur.
2. Her sistemi küçük ve bağımsız modüllere ayır.
3. Her şeyi tek GameManager sınıfına yazma.
4. SOLID ve Clean Code prensiplerinden yararlan.
5. Gereksiz mimari karmaşıklık oluşturma.
6. ScriptableObjects ile içerik ve denge verilerini yönet.
7. Oyuncu verileriyle sabit oyun verilerini birbirinden ayır.
8. Her sistem kaydedilebilir ve test edilebilir olmalı.
9. Var olmayan API, sınıf veya metotları uydurma.
10. Çalışmayan butonları tamamlanmış özellik olarak sunma.
11. Test edilmemiş kodları başarılı olarak raporlama.
12. Her alt görevden sonra Unity derleme durumunu kontrol et.
13. Hata varsa sonraki sisteme geçmeden düzelt.
14. Mevcut çalışan sistemleri gereksiz yere değiştirme.
15. Harici paket kurulumlarında sürüm uyumluluğunu doğrula.
16. Gizli anahtarları istemci koduna yazma.
17. Kullanıcının onayı olmadan gerçek ödeme veya canlı sunucu işlemi gerçekleştirme.
18. Proje dışındaki kişisel dosyalara erişme.
19. Geri dönüşü zor değişikliklerden önce kullanıcı onayı iste.
20. Gereksiz açıklamalar yerine çalışan sonuçlara odaklan.
21. Kod sınıflarını ve değişkenleri İngilizce yaz.
22. Geliştirme raporlarını Türkçe hazırla.

Her küçük kod değişikliği için kullanıcıdan onay isteme. Onaylanan geliştirme aşamasındaki güvenli ve geri alınabilir işlemleri tamamla.

## 6. PROJECT MEMORY

Proje kök klasöründe aşağıdaki dosyaları oluştur:

- CLAUDE.md
- MASTER_PROMPT.md
- GAME_DESIGN.md
- ARCHITECTURE.md
- ROADMAP.md
- DEVELOPMENT_LOG.md
- TASK_STATUS.md
- BUG_TRACKER.md
- ECONOMY_BALANCE.md
- UI_DESIGN_SYSTEM.md
- ASSET_REGISTER.md
- TEST_PLAN.md

CLAUDE.md dosyasında kısa ve önemli proje kuralları bulunmalı.

MASTER_PROMPT.md dosyasında oyunun kapsamlı geliştirme talimatları bulunmalı.

DEVELOPMENT_LOG.md dosyasında yapılan değişiklikler tutulmalı.

TASK_STATUS.md dosyasında her görevin durumu bulunmalı:

- PENDING
- IN_PROGRESS
- TEST_REQUIRED
- COMPLETED
- BLOCKED

Her yeni oturumda önce CLAUDE.md, aktif görev ve DEVELOPMENT_LOG.md dosyalarını incele.

## 7. PROJECT FOLDER STRUCTURE

Aşağıdaki yapıyı temel al:

Assets/_Game/

- Scripts/Core
- Scripts/Data
- Scripts/Buildings
- Scripts/Economy
- Scripts/Resources
- Scripts/Heroes
- Scripts/Inventory
- Scripts/Equipment
- Scripts/Troops
- Scripts/Combat
- Scripts/Zombies
- Scripts/WorldMap
- Scripts/Research
- Scripts/Events
- Scripts/VIP
- Scripts/Shop
- Scripts/Alliance
- Scripts/Network
- Scripts/Save
- Scripts/UI
- Scripts/Editor
- Art/Models
- Art/Characters
- Art/Buildings
- Art/Environment
- Art/Textures
- Art/Materials
- Art/Icons
- Art/Portraits
- Art/Animations
- Prefabs
- Scenes
- ScriptableObjects
- Audio
- VFX
- Localization
- Tests

Gereksiz boş klasörleri ve kullanılmayacak soyut sistemleri oluşturmaktan kaçın.

Unity GUID ve meta dosyalarını koru.

## 8. VISUAL ART DIRECTION

Görsel tarz:

- Realistic
- Cinematic
- Modern Military
- Post-Apocalyptic
- Premium
- Dark Tactical
- Industrial

Oyunu çizgi film, çocuk oyunu veya aşırı basit geometrik tasarım görünümünde geliştirme.

Ancak kaliteli 3D modeller mevcut değilse prototip sırasında geçici geometriler kullanılabilir. Bunları açıkça placeholder olarak işaretle.

Renk paleti:

- Primary Background: #101419
- Secondary Background: #1C232B
- Military Green: #465744
- Premium Gold: #D4AF63
- Metallic Silver: #ADB8C4
- Danger Red: #D94B4B
- Success Green: #4CAF72
- Information Blue: #4C9FE8
- Primary Text: #F2F4F6
- Secondary Text: #8C97A3

UI tasarımı:

- Askerî metalik paneller
- Profesyonel ikonlar
- Altın detaylar
- Karakter portreleri
- Nadirlik çerçeveleri
- Akıcı menü animasyonları
- Tutarlı tipografi
- İlerleme çubukları
- Ödül ekranları
- Etkinlik bannerları
- Mobil dokunmatik kontroller

Reference Resolution: 1080x1920

Canvas Scaler, responsive layout ve safe area desteği oluştur.

---

# PHASE 01 — GAME FOUNDATION

## Objective

Sıfırdan çalışan Unity oyun altyapısı oluştur.

Öncelikle gerçek Unity proje klasörünü ve kurulu Editor sürümünü doğrula.

Proje klasörü boşsa kurulu Unity Editor üzerinden uygun proje oluşturma yöntemini kullan.

Unity kurulu değilse veya lisans etkin değilse durumu raporla.

## Core Systems

Geliştirilecek sistemler:

- Game Bootstrap
- Game State
- Scene Management
- Game Configuration
- Player Data
- Save System
- Audio Foundation
- UI Navigation
- Input Management
- Gameplay Events
- Localization Foundation

Save System özellikleri:

- Automatic Save
- Manual Save
- JSON Save
- Save Version
- Backup Save
- Save Migration
- Corrupted Save Recovery

İlk sahneler:

- Bootstrap
- MainMenu
- Base
- WorldMap
- Battle

Kamera:

- Isometric View
- Mouse Drag
- Touch Drag
- Pinch Zoom
- Mouse Wheel Zoom
- Smooth Movement
- Camera Boundaries
- Building Selection

## UI Foundation

Yeniden kullanılabilir UI prefabları:

- Primary Button
- Secondary Button
- Icon Button
- Popup
- Progress Bar
- Resource Counter
- Notification Badge
- Item Card
- Hero Card
- Event Card
- Shop Card
- Tab Navigation
- Confirmation Dialog
- Loading Screen

## Main HUD

Üst alan:

- Player Avatar
- Player Name
- Commander Level
- Combat Power
- VIP Level
- Food
- Wood
- Steel
- Fuel
- Gold
- Diamonds

Yan alanlar:

- Construction Queue
- Research Queue
- Troop Training
- Missions
- Events
- Event Calendar
- VIP
- Daily Rewards
- Special Offers

Alt navigasyon:

- Base
- Heroes
- Troops
- Inventory
- World
- Alliance
- Shop

Mevcut olmayan sistemlerin butonlarını açıkça kilitli göster.

## Acceptance Criteria

- Unity projesi hatasız derlenmeli.
- Base sahnesi Play Mode içerisinde açılmalı.
- Kamera hareket etmeli.
- Ana HUD görüntülenmeli.
- Mevcut navigasyon butonları çalışmalı.
- Oyuncu kaydı oluşturulmalı ve yeniden yüklenebilmeli.

---

# PHASE 02 — MILITARY BASE AND ECONOMY

## Resource System

Kaynaklar:

- Food
- Wood
- Steel
- Fuel
- Gold
- Diamonds
- Energy
- Alliance Credits
- VIP Points

Her kaynak için:

- Current Amount
- Maximum Capacity
- Production Rate
- Add Resource
- Spend Resource
- Resource Changed Event
- Offline Production

Negatif bakiye ve geçersiz işlemleri engelle.

## Military Base

3D askerî üs oluştur.

Çevre:

- Beton yollar
- Asfalt
- Savunma duvarları
- Kontrol noktaları
- Ormanlar
- Terk edilmiş evler
- Sokak lambaları
- Askerî araçlar
- Enkaz
- Gözetleme kuleleri

Binalar:

1. Headquarters
2. Barracks
3. Hospital
4. Research Laboratory
5. Tank Factory
6. Aircraft Hangar
7. Food Farm
8. Lumber Yard
9. Steel Factory
10. Oil Refinery
11. Power Generator
12. Radar Station
13. Defense Tower
14. Warehouse
15. Hero Recruitment Center
16. Alliance Embassy
17. Equipment Workshop
18. Survivor Shelter
19. Drone Station
20. Intelligence Center
21. Training Ground
22. Trading Center

İlk prototipte Headquarters, Barracks, Hospital, Research Laboratory, Food Farm ve Steel Factory binalarını aktif et.

Diğer binaları veri tabanlı şekilde eklenebilir tasarla.

## Building System

Her bina için:

- Unique ID
- Name
- Level
- Maximum Level
- Upgrade Cost
- Upgrade Duration
- Power Contribution
- Production Rate
- Storage Capacity
- Requirements
- Unlock Conditions
- Prefab
- Visual Variants

Bina seviyeleri başlangıçta 1–35 olacak şekilde yapılandırılabilir olmalı.

Özellikler:

- Building Selection
- Building Details
- Building Upgrade
- Construction Queue
- Construction Timer
- Construction Speedups
- Upgrade Rewards
- Offline Progress

Bina geliştirmeleri gerçek kaynak tüketmeli.

İnşaat zamanlarını UTC tabanlı bitiş zamanları olarak kaydet.

## Acceptance Criteria

Oyuncu binayı seçebilmeli, kaynaklarını kullanarak geliştirebilmeli, tamamlanma süresini görebilmeli ve oyunu yeniden açtığında ilerlemesini koruyabilmeli.

---

# PHASE 03 — HEROES, INVENTORY AND MATERIALS

## Hero Database

Başlangıç karakterleri:

- Marcus Stone — Tank Commander
- Elena Voss — Sniper
- Victor Kane — Assault Specialist
- Sophia Reed — Combat Medic
- Raven Cross — Intelligence Officer
- Nikolai Frost — Defense Specialist
- Maya Steel — Aircraft Commander
- Ethan Wolf — Infantry Commander
- Adrian Knox — Tactical Commander
- Olivia Hayes — Support Specialist
- Darius Cole — Heavy Infantry
- Jack Hunter — Engineer

Karakter isimleri geçici olup yayın öncesinde özgünlük açısından kontrol edilecektir.

Hero Rarity:

- R — Rare
- SR — Super Rare
- SSR — Legendary
- UR — Mythic

Hero Data:

- Hero ID
- Name
- Biography
- Class
- Rarity
- Portrait
- 3D Model
- Level
- Experience
- Stars
- Rank
- Health
- Attack
- Defense
- Critical Chance
- Critical Damage
- Leadership
- Skills
- Equipment
- Combat Power

Hero Progression:

- Level 1–100
- Stars 1–6
- Hero Experience
- Star Upgrade
- Rank Promotion
- Skill Upgrade
- Equipment Upgrade
- Talent Tree
- Awakening

İlk olarak üç kahramanın bütün temel geliştirme mekaniklerini çalışır hale getir.

## Hero UI

Oluştur:

- Hero Collection Screen
- Hero Details Screen
- Hero Upgrade Screen
- Skill Screen
- Equipment Screen
- Hero Recruitment Screen

Arayüzde karakter portreleri, nadirlik renkleri, yıldızlar, seviye göstergeleri ve 3D model önizleme alanı bulunmalı.

## Recruitment

- Standard Recruitment
- Advanced Recruitment
- Free Daily Recruitment
- Event Recruitment
- Fragment Exchange
- Direct Unlock

Rastgele ödüller varsa olasılıkları açıkça göster.

Gerçek para ile rastgele ödül satın alımını ilk sürümde etkinleştirme.

## Inventory

Kategoriler:

- Resources
- Materials
- Equipment
- Hero Shards
- Speedups
- Consumables
- Event Items
- Cosmetics

Özellikler:

- Stackable Items
- Search
- Sorting
- Filtering
- Item Details
- Item Usage
- Quantity Selection
- Obtain Sources
- Material Conversion

Materyaller:

- Iron
- Titanium
- Copper
- Military Fabric
- Polymer
- Electronic Components
- Circuit Boards
- Weapon Parts
- Armor Plates
- Engine Parts
- Drone Components
- Mutant Samples
- Energy Cores

Karakter materyalleri:

- Experience Manuals
- Hero Shards
- Universal Shards
- Skill Books
- Promotion Medals
- Tactical Manuals

Nadirlikler:

- Common
- Uncommon
- Rare
- Epic
- Legendary
- Mythic

## Equipment

Slotlar:

- Weapon
- Helmet
- Body Armor
- Gloves
- Boots
- Backpack
- Communication Device

Özellikler:

- Crafting
- Upgrade
- Equip
- Unequip
- Dismantle
- Set Bonuses
- Equipment Comparison

## Acceptance Criteria

Kahramanlar gerçek materyaller kullanılarak geliştirilebilmeli. Envanter güncellenmeli, savaş gücü hesaplanmalı ve bütün veriler kaydedilebilmelidir.

---

# PHASE 04 — TROOPS, ZOMBIES AND COMBAT

## Military Units

Infantry:

- Recruit
- Assault Infantry
- Heavy Infantry
- Elite Infantry

Tanks:

- Light Tank
- Medium Tank
- Heavy Tank
- Advanced Tank

Aircraft:

- Scout Helicopter
- Attack Helicopter
- Fighter Aircraft
- Combat Drone

Support:

- Artillery
- Medical Unit
- Engineering Unit
- Logistics Unit

Unit Tier: T1–T10

İlk sürümde T1–T3 aktif olsun.

Her birlik için:

- Attack
- Defense
- Health
- Training Cost
- Training Time
- Carry Capacity
- Movement Speed
- Combat Power

## Troop Training

- Training Queue
- Unit Selection
- Training Quantity
- Training Timer
- Promotion
- Training Speedups

## Squad System

- Hero Selection
- Troop Selection
- Frontline
- Backline
- Squad Power
- Formation Presets
- Hero Synergy

## Zombie Types

- Walker
- Runner
- Armored Infected
- Toxic Infected
- Stalker
- Screamer
- Mutant Brute
- Alpha Mutant
- Titan Boss

Zombie AI:

- Idle
- Patrol
- Detection
- Chase
- Attack
- Hit
- Death

Object pooling ve uygun pathfinding sistemi kullan.

İlk aşamada üç zombi türünü aktif et.

## Combat System

- PvE Battles
- Damage Calculation
- Defense Calculation
- Critical Hits
- Hero Skills
- Buffs
- Debuffs
- Unit Counters
- Battle Rewards
- Casualties
- Victory
- Defeat

Savaş simülasyonu görsellerden bağımsız olmalıdır.

Savaşın sonucunu yalnızca toplam savaş gücü belirlememeli.

## Base Defense

- Zombie Waves
- Defense Towers
- Walls
- Base Health
- Wave Counter
- Increasing Difficulty
- Boss Waves
- Victory Rewards

## Hospital

- Injured Troops
- Healing Queue
- Hospital Capacity
- Healing Cost
- Healing Duration

## Battle Reports

- Battle Result
- Participants
- Damage
- Casualties
- Injured Units
- Rewards

## Acceptance Criteria

Oyuncu asker üretebilmeli, kahramanlarını takıma ekleyebilmeli, zombilere karşı savaşabilmeli ve ödül kazanabilmelidir.

Bu aşama sonunda oyunun temel oynanış döngüsü gerçekten çalışmalıdır.

Resource Collection → Base Upgrade → Troop Training → Hero Upgrade → Zombie Battle → Rewards → Resource Collection.

---

# PHASE 05 — WORLD MAP AND EXPLORATION

## World Map

İzometrik 3D dünya haritası oluştur.

Harita içeriği:

- Player Bases
- Abandoned Cities
- Abandoned Hospitals
- Military Camps
- Laboratories
- Forests
- Oil Fields
- Resource Nodes
- Zombie Hordes
- Mutant Bosses
- Strategic Locations

Özellikler:

- Map Pan
- Map Zoom
- Coordinates
- Map Search
- Fog of War
- Region Loading
- Map Markers
- Army Movement

İlk aşamada yerel ve küçük bir harita oluştur.

## Resource Gathering

- Resource Node Level
- Resource Amount
- Army Dispatch
- Travel Time
- Gathering Time
- Carry Capacity
- Return to Base

Ödüller işlemin tamamlanmasıyla verilmeli.

## Radar System

Görevler:

- Zombie Hunt
- Rescue Survivors
- Hospital Exploration
- Laboratory Exploration
- Resource Gathering
- Mutant Hunt
- Equipment Recovery

Radar özellikleri:

- Radar Level
- Energy Consumption
- Mission Refresh
- Mission Difficulty
- Reward Tables

## Research System

Teknoloji kategorileri:

- Economy
- Military
- Defense
- Development
- Logistics
- Advanced Technology

Araştırma seviyesi, maliyeti, süresi ve ön koşulları bulunmalı.

---

# PHASE 06 — QUESTS, EVENTS AND CALENDAR

## Commander Profile

- Player Name
- Avatar
- Commander Level
- Experience
- Power
- VIP
- Battle Statistics
- Titles
- Profile Frames

Savaş gücü kategorileri:

- Building Power
- Hero Power
- Troop Power
- Equipment Power
- Research Power

## Quest System

- Main Story
- Chapter Missions
- Daily Missions
- Weekly Missions
- Side Missions
- Achievements
- Tutorial Missions

Görev ilerlemesi gerçek oyun olaylarından hesaplanmalı.

## Daily Rewards

- Daily Login
- Seven-Day Login
- Daily Free Chest
- Weekly Rewards
- New Player Rewards
- Return Player Rewards

## Event System

Etkinlikler:

- Zombie Hunt
- Mutant Invasion
- Hero Training
- Troop Training
- Resource Gathering
- Building Development
- Research Race
- Power Growth
- Alliance Mobilization
- Territory War
- Server Competition

Her etkinlik için:

- Event ID
- Event Name
- Event Banner
- Description
- Start Time
- End Time
- Claim Deadline
- Participation Rules
- Point Rules
- Progress
- Milestone Rewards
- Ranking Rewards

## Event Calendar

Takvim ekranları:

- Daily
- Weekly
- Monthly
- Active Events
- Upcoming Events
- Completed Events

Etkinlik durumları:

- Upcoming
- Active
- Completed
- Reward Available
- Expired

Geri sayımlar doğru çalışmalı.

Etkinlik ödülleri birden fazla kez alınamamalı.

Etkinlikler kod değişikliği gerektirmeden veri üzerinden oluşturulabilmeli.

---

# PHASE 07 — VIP, SHOP AND MONETIZATION

## VIP System

VIP Level: 0–20

VIP Experience kaynakları:

- Daily Login
- Missions
- Events
- VIP Items
- Eligible Premium Packs

VIP avantajları:

- Additional Formation Presets
- Cosmetic Frames
- VIP Badge
- Daily VIP Chest
- VIP Shop Access
- Limited Construction Benefits
- Limited Gathering Benefits
- Quality of Life Features

Ücretsiz oyuncular da VIP puanı kazanabilmeli.

VIP avantajları rekabeti tamamen satın alma gücüne bağlamamalı.

## VIP Center

- Current VIP Level
- VIP Progress Bar
- Next VIP Level
- Current Benefits
- Upcoming Benefits
- Daily VIP Reward
- VIP Level Preview
- VIP Shop

## VIP Shop

Kategoriler:

- Hero Materials
- Equipment Materials
- Speedups
- Resources
- Cosmetics
- Daily Offers
- Weekly Offers

Her ürün için:

- Product ID
- Name
- Icon
- Price
- Currency
- Required VIP Level
- Purchase Limit
- Reset Schedule

## Premium Store

- Diamond Packs
- Starter Packs
- Development Packs
- Resource Packs
- Hero Packs
- Monthly Membership
- Cosmetic Packs
- Special Offers
- Free Rewards

Örnek test ürünleri:

- 80 Diamonds — $0.99
- 400 Diamonds — $4.99
- 800 Diamonds — $9.99
- 1600 Diamonds — $19.99
- 4000 Diamonds — $49.99
- 8000 Diamonds — $99.99

Bunlar geliştirme amaçlı geçici fiyatlardır.

Gerçek fiyatlar platform mağazasından alınmalıdır.

## In-App Purchases

Önce Mock Purchase Provider oluştur.

Destekle:

- Successful Purchase
- Cancelled Purchase
- Failed Purchase
- Pending Purchase
- Duplicate Transaction
- Restore Purchase
- Refund

Gerçek Unity IAP entegrasyonuna geçmeden önce kullanıcı onayı al.

Online sürümde satın alma doğrulaması sunucu üzerinden yapılmalıdır.

## Battle Pass

- Free Pass
- Premium Pass
- 30-Day Season
- 50 Levels
- Season Missions
- Experience
- Reward Claim
- Season Reset

Premium ödüllerde kozmetik içeriklere öncelik ver.

Sahte indirim, yanıltıcı teklif veya zorunlu satın alma mekanikleri oluşturma.

---

# PHASE 08 — MULTIPLAYER AND ALLIANCES

Bu aşamaya yalnızca yerel oyun döngüsü çalıştığında ve kullanıcı onay verdiğinde geç.

## Backend

- Authentication
- Player Accounts
- Server Selection
- Cloud Save
- Player Profiles
- Persistent World
- Server Time
- Database
- API Validation

Mevcut seçenekleri değerlendir ve oyun için uygun mimariyi seç.

## Online World

- Real Player Bases
- Online Resource Nodes
- Army Movement
- Territory Ownership
- Server Synchronization
- Reconnection
- World Persistence

## Alliance System

- Create Alliance
- Join Alliance
- Applications
- Invitations
- Alliance Ranks
- Alliance Permissions
- Alliance Donations
- Alliance Help
- Alliance Technology
- Alliance Shop
- Alliance Gifts
- Alliance Territory
- Alliance Rally
- Alliance Boss
- Diplomacy

Rütbeler:

- R1 Recruit
- R2 Member
- R3 Veteran
- R4 Officer
- R5 Leader

## PvP

- Base Attack
- Player Battles
- Alliance Rally
- Territory War
- Capital War
- Arena
- Server Competition

## Chat and Mail

- World Chat
- Alliance Chat
- Private Messages
- System Messages
- Battle Reports
- Reward Mail

Spam koruması, kullanıcı engelleme, raporlama ve moderasyon oluştur.

## Leaderboards

- Player Power
- Base Level
- Hero Power
- Zombie Kills
- Alliance Power
- Alliance Kills
- Event Ranking
- Server Ranking

## Security

Sunucu otoriter mimari kullan.

İstemci kaynaklarına, saatine veya savaş sonucuna güvenme.

- Authentication
- Authorization
- Transaction Validation
- Idempotency
- Rate Limiting
- Audit Logs
- Receipt Validation
- Anti-Cheat

Canlı ortamda sahte kaynak üretimini, tekrar ödül almayı ve geçersiz satın alımları engelle.

---

# PHASE 09 — PROFESSIONAL GRAPHICS AND CONTENT

## 3D Asset Pipeline

Oyun için gerekli bütün 3D modellerin listesini oluştur.

Buildings:

- Headquarters
- Hospital
- Barracks
- Tank Factory
- Laboratory
- Radar Station
- Warehouse
- Abandoned House
- Defense Tower
- Military Shelter

Characters:

- Military Commander
- Female Sniper
- Combat Medic
- Engineer
- Infantry Soldier
- Heavy Infantry
- Special Forces

Zombies:

- Walker
- Runner
- Armored Zombie
- Toxic Zombie
- Mutant Boss

Vehicles:

- Military Truck
- Tank
- Helicopter
- Drone
- Armored Vehicle

Environment:

- Trees
- Roads
- Barricades
- Containers
- Sandbags
- Ruins
- Street Lights
- Military Crates

Meshy, Tripo ve Blender ile üretilecek modeller için İngilizce 3D üretim promptları hazırla.

Her asset için:

- Asset ID
- Name
- Description
- Generation Prompt
- Model Path
- Texture Path
- Scale
- Target Polygon Budget
- Collider
- Rig Requirement
- Animation Requirement
- LOD
- License Status

Harici 3D üretim araçlarına erişimin yoksa gerçek modeli oluşturmuş gibi davranma.

Mevcut modelleri Unity'ye aktar ve prefab bağlantılarını tamamla.

## Animation

- Idle
- Walk
- Run
- Attack
- Hit
- Death
- Hero Preview
- Building Upgrade
- Resource Collection
- Vehicle Movement

## Visual Effects

- Combat Effects
- Explosion Effects
- Resource Collection
- Upgrade Effects
- Hero Unlock Effects
- Legendary Reward Effects
- Zombie Spawn Effects
- UI Transition Effects

## Audio

- Base Ambience
- Zombie Ambience
- Military Sounds
- Combat Music
- Main Menu Music
- Button Sounds
- Reward Sounds

Yayın sürümünde yalnızca uygun ticari kullanım lisansına sahip içerikler kullan.

---

# PHASE 10 — OPTIMIZATION, ADMIN AND RELEASE

## Admin Panel

- Player Management
- Event Management
- Event Calendar
- Product Catalog
- Economy Configuration
- Hero Configuration
- Building Configuration
- VIP Configuration
- Battle Pass Configuration
- System Announcements
- Moderation
- Purchase History
- Analytics

Yönetim paneli ayrı yetkilendirme sistemi kullanmalı.

## Game Economy

Merkezi ekonomi tabloları oluştur.

- Building Upgrade Costs
- Construction Durations
- Production Rates
- Hero Experience Curves
- Troop Training Costs
- Research Costs
- Event Rewards
- VIP Points
- Shop Prices

Ücretsiz ve premium oyuncu ilerlemesini simüle et.

## Optimization

- Object Pooling
- LOD
- GPU Instancing
- Texture Compression
- Sprite Atlases
- Baked Lighting
- Draw Call Optimization
- Scene Streaming
- Memory Optimization
- UI Optimization
- Asynchronous Loading

Grafik seçenekleri:

- Low
- Medium
- High
- Ultra

FPS seçenekleri:

- 30 FPS
- 60 FPS

Gerçek mobil cihazlarda performans ölçümü yap.

## Localization

- Turkish
- English

Yeni dillerin eklenebileceği genişletilebilir localization altyapısı oluştur.

## Testing

- Unit Tests
- Integration Tests
- EditMode Tests
- PlayMode Tests
- Save Tests
- Economy Tests
- Combat Tests
- UI Tests
- Security Tests
- Performance Tests

Kritik testler:

- Yetersiz kaynakla işlem
- Negatif kaynak miktarı
- Tekrar ödül alma
- Tekrar satın alma
- Geçersiz materyal kullanımı
- Kayıt dosyası bozulması
- Oyun kapanıp açıldığında ilerleme
- Bağlantı kopması
- Etkinlik süresinin dolması

Testleri silerek, devre dışı bırakarak veya sonuçlarını değiştirerek başarılı sonuç üretme.

## Android Release

- Android Build Support
- App Identifier
- Game Icon
- Splash Screen
- Version Management
- Internal Testing
- Privacy Configuration
- Android App Bundle

Kullanıcının açık onayı olmadan oyunu mağazaya yükleme veya yayınlama.

---

# UNITY AUTOMATION

Unity sahne, prefab, materyal ve arayüz oluşturma işlemlerinde kontrollü Editor Script kullan.

Gerektiğinde Tools > DEADZONE altında araçlar oluştur:

- Validate Project
- Create Base Scene
- Generate UI
- Generate Building Prefabs
- Import Game Assets
- Generate Test Data
- Run Validation
- Build Development APK

Bu araçlar yeniden çalıştırıldığında gereksiz nesne veya dosya kopyaları oluşturmamalıdır.

Mevcut sahne ve prefabları koru.

Unity batch mode kullanılabiliyorsa kontrollü şekilde derleme ve test çalıştır.

Unity Editor erişimin yoksa bunu raporla ve gerekli manuel adımları belirt.

---

# DEVELOPMENT EXECUTION PROTOCOL

Bir geliştirme aşaması başladığında:

1. Mevcut proje durumunu incele.
2. İlgili CLAUDE.md ve geliştirme kayıtlarını oku.
3. Aşamayı küçük alt görevlere böl.
4. Gerekli dosyaları oluştur.
5. Gerçek kodları yaz.
6. Sistemleri birbirine bağla.
7. Unity sahne ve prefab bağlantılarını oluştur.
8. Derleme ve test kontrolü yap.
9. Hataları düzelt.
10. Başarılıysa aynı aşamadaki sonraki alt göreve geç.
11. Aşama tamamlandığında dokümantasyonu güncelle.
12. Kullanıcıya rapor ver.

Her küçük görev için ayrı kullanıcı komutu bekleme.

Ancak yeni büyük geliştirme aşamasına geçmeden önce mevcut aşamanın test ve kabul kriterlerini değerlendir.

Başarısız bir sistemin üzerine yeni sistemler ekleme.

İlk hedef bütün özellikleri yüzeysel biçimde üretmek değil, çalışan bir vertical slice oluşturmaktır.

---

# DEVELOPMENT REPORT FORMAT

PROJECT: DEADZONE: LAST EMPIRE

CURRENT PHASE:

STATUS:

COMPLETED FEATURES:

CREATED FILES:

MODIFIED FILES:

UNITY SCENES:

TEST RESULTS:

KNOWN ISSUES:

MISSING ASSETS:

MANUAL ACTIONS REQUIRED:

NEXT PHASE:

Gerçek test sonuçlarını raporla. Tamamlanmamış özellikleri açıkça belirt.

---

# INITIAL EXECUTION COMMAND

Bu proje sıfırdan başlatılmaktadır.

Önce mevcut çalışma klasörünü, Unity kurulumunu, Unity sürümünü ve proje durumunu doğrula.

Ardından:

1. Unity projesinin temel yapısını oluştur.
2. CLAUDE.md ve geliştirme dokümantasyonunu oluştur.
3. Git sürüm kontrolü altyapısını hazırla.
4. PHASE 01 görevlerini bağımlılık sırasına göre uygula.
5. Oyun başlangıç sahnesini oluştur.
6. İzometrik kamerayı geliştir.
7. Temel oyuncu veri ve kayıt sistemini oluştur.
8. Ana oyun HUD arayüzünü oluştur.
9. Test amaçlı askerî üs alanını oluştur.
10. Unity derleme ve test kontrolünü yap.
11. Hataları düzelt.
12. PHASE 01 sonucunu raporla.

Bu komutu aldıktan sonra yalnızca geliştirme planı yazıp durma.

Erişim yetkin dahilinde gerçek geliştirme çalışmalarına başla.

Nihai hedefimiz özgün, kaliteli, gerçekçi ve ticari olarak yayınlanabilir DEADZONE: LAST EMPIRE mobil strateji oyunudur.


---
## KALICI KURAL EKİ — PHASE 02.5 (Profesyonel Grafik, v0.2.5)
- Toplam depolama bütçesi 3 GB: doku/materyal 900 MB, 3D modeller 650 MB, animasyonlar 300 MB, dünya/ortam 300 MB, ses 250 MB, motor+kod 200 MB, UI+efektler 100 MB, gelecek güncellemeler 300 MB. İlk indirme 300–500 MB.
- Android öncelikli; minimum 4 GB RAM; düşük kalitede 30 FPS, yüksekte 60 FPS hedefi.
- Doku sıkıştırma, LOD, GPU instancing, batching, atlas, nesne havuzlama; Addressables'a hazır içerik yapısı.
- Bu hedefler gerçek cihaz ve Android build ölçümleriyle doğrulanır. Ölçülmemiş sonuçlar başarılı olarak raporlanmaz. Gerçek FPS ölçülmeden performans hedeflerinin karşılandığı iddia edilmez.
- Kullanıcı izni olmadan yazılım veya ücretli hizmet satın alınmaz/kurulmaz. Gizli bilgiler ve gereksiz Unity cache dosyaları GitHub'a yüklenmez.
- Her büyük görev grubundan sonra Git commit; doğrulanmış değişiklikler push edilir. Mevcut çalışan sistemler bozulmaz.
- Cihaz saati manipülasyonu offline prototipte tamamen engellenemez (belgelendi); sunucu zamanı multiplayer aşamasına bırakıldı.
- Ayrıntılar: PERFORMANCE_BUDGET.md, ART_DIRECTION.md, BLENDER_PIPELINE.md.
