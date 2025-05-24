# Simple POS System - Энгийн Борлуулалтын Систем

## Тойм

Simple POS System нь C# Windows Forms технологи дээр үндэслэн хөгжүүлсэн жижиг, дунд бизнесүүдэд зориулсан борлуулалтын цэгийн систем юм. Энэ систем нь ресторан, кафе, дэлгүүрүүдэд хэрэглэхэд тохиромжтой бөгөөд SQLite өгөгдлийн санг ашигладаг.

## Суулгах Заавар

### 1. Repository-г татах
```bash
git clone https://github.com/Sanjaa46/PosMachine.git
cd PosMachine
```

### 2. Dependency-ууд суулгах
Visual Studio дээр solution-г нээж, NuGet packages-уудыг татна:
```
Tools > NuGet Package Manager > Manage NuGet Packages for Solution
```

Шаардлагатай packages:
- System.Data.SQLite.Core (1.0.119.0)

### 3. Бүтээж ажиллуулах
1. Microsoft Visual Studio дээр `PosMachine.sln` файлыг нээнэ
2. Solution-г Buil хийнэ (Build > Build Solution)
3. Програмыг ажиллуулна уу (F5 эсвэл Debug > Start Debugging)

## Ашиглах Заавар

### Анхны Нэвтрэлт
Систем анх удаа ажиллахад дараах хэрэглэгчид үүсгэгдэнэ:

**Менежер:**
- Хэрэглэгчийн нэр: `Manager`
- Нууц үг: `password`

**Кассчин:**
- Хэрэглэгчийн нэр: `Cashier1` эсвэл `Cashier2`
- Нууц үг: `password`

### Борлуулалт Хийх
1. Системд нэвтрэх
2. Бүтээгдэхүүний код оруулах эсвэл зургийг дарж сонгох
3. Сагсанд нэмэгдсэн бүтээгдэхүүнүүдийг шалгах
4. "PAY" товчийг дарж төлбөр авах
5. Төлсөн дүнг оруулж, "Complete Sale" дарах
6. Баримт хэвлэх эсвэл хадгалах

### Бүтээгдэхүүн Удирдах
**Менежерийн эрх шаардлагатай:**
1. Products > Manage Products цэс сонгох
2. Шинэ бүтээгдэхүүн нэмэх эсвэл сонгож засах
3. Код, нэр, үнэ, ангилал, зураг оруулах
4. Хадгалах эсвэл устгах

### Ангилал Удирдах
**Менежерийн эрх шаардлагатай:**
1. Categories > Manage Categories цэс сонгох
2. Шинэ ангилал нэмэх эсвэл сонгож засах
3. Хадгалах эсвэл устгах

## Файлын Бүтэц

```
PosMachine/
├── Data/
│   └── DatabaseHelper.cs          # Өгөгдлийн сантай ажиллах класс
├── Models/
│   ├── User.cs                    # Хэрэглэгчийн модель
│   ├── Product.cs                 # Бүтээгдэхүүний модель
│   ├── Category.cs                # Ангиллын модель
│   ├── Order.cs                   # Захиалгын модель
│   └── OrderItem.cs               # Захиалгын зүйлийн модель
├── Forms/
│   ├── LoginForm.cs               # Нэвтрэх цонх
│   ├── MainForm.cs                # Үндсэн цонх
│   ├── PaymentForm.cs             # Төлбөрийн цонх
│   ├── ReceiptForm.cs             # Баримтын цонх
│   ├── ProductManagementForm.cs   # Бүтээгдэхүүн удирдах цонх
│   ├── CategoryManagementForm.cs  # Ангилал удирдах цонх
│   └── ProductListForm.cs         # Бүтээгдэхүүний жагсаалт
└── Program.cs                     # Програмын үндсэн цэг
```

## Өгөгдлийн Сан

Систем нь SQLite өгөгдлийн санг ашигладаг бөгөөд дараах хүснэгтүүдтэй:

- **Users** - Хэрэглэгчийн мэдээлэл
- **Categories** - Бүтээгдэхүүний ангилал
- **Products** - Бүтээгдэхүүний мэдээлэл
- **Orders** - Захиалгын мэдээлэл
- **OrderItems** - Захиалгын бараанууд

Өгөгдлийн сан `posdb.sqlite` нэртэй файлд хадгалагдана.

## Default Өгөгдөл

Систем анх удаа ажиллахад дараах жишээ өгөгдлүүд үүсгэгдэнэ:

### Ангиллууд:
- Pizza
- Pasta  
- Sandwich

### Бүтээгдэхүүнүүд:
- Margherita (Code: 1, Price: $100)
- Marinara (Code: 2, Price: $200)
- Vegetarians (Code: 3, Price: $150)
- Alfredo (Code: 4, Price: $200)
- Spaghetti Pasta (Code: 5, Price: $150)
- White Sauce Pasta (Code: 6, Price: $200)
- American Sub (Code: 8, Price: $100)
