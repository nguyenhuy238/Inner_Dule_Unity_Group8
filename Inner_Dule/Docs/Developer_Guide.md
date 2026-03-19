# Hướng Dẫn Cho Nhà Phát Triển (Developer Guide)

Tài liệu hướng dẫn quy trình làm việc với mã nguồn hiện tại của Inner Duel.

## 1. Quy Trình Thêm Nhân Vật Mới (Character)

Để thêm một nhân vật mới hoàn chỉnh, bạn cần thực hiện các bước sau:

### Bước 1: Tạo ScriptableObject Data
1.  Trong Project, chuột phải chọn **Create > InnerDuel > Character > CharacterData**.
2.  Đặt tên file (ví dụ: `CD_NewHero.asset`).
3.  Điền các thông số:
    - **Stats**: HP, Speed, Jump Force.
    - **Combat**: Damage, Cooldowns cho Normal, Attack 1/2/3.
    - **Visuals**: Gán màu sắc đại diện.
    - **Cờ**: Tích chọn `Can Dash`, `Can Block` tùy theo thiết kế.

### Bước 2: Tạo Prefab Nhân Vật
1.  Tạo một GameObject mới hoặc Duplicate từ `BaseCharacter`.
2.  Gắn script `InnerCharacterController`.
3.  Gắn `Animator` và `Rigidbody2D`.
4.  **Quan Trọng**: Thiết lập các Transform con cho điểm tấn công:
    - `NormalAttackPoint`
    - `Attack1Point`, `Attack2Point`, `Attack3Point`
    - `GroundCheck` (nằm dưới chân)
5.  Gán `Ground Layer` trong script là "Ground".

### Bước 3: Đăng Ký Nhân Vật
1.  Mở scene `CharacterSelectScene`.
2.  Tìm object `CharacterSelectManager`.
3.  Thêm `CharacterData` vừa tạo vào danh sách `availableCharacters`.

---

## 2. Quy Trình Thêm Bản Đồ Mới (Map)

### Bước 1: Thiết Kế Map Prefab
1.  Tạo map trong Scene.
2.  Đảm bảo tất cả nền đất (sàn, bục nhảy) có **Collider2D**.
3.  **BẮT BUỘC**: Gán Layer của các nền đất là **"Ground"**. Nếu không, nhân vật sẽ không nhảy được.
4.  Kéo toàn bộ map vào folder Prefabs để tạo Prefab.

### Bước 2: Tạo Data
1.  Chuột phải chọn **Create > InnerDuel > Game > MapData**.
2.  Kéo Prefab bản đồ vào trường `Map Prefab`.
3.  Thêm ảnh Preview và Tên.

### Bước 3: Đăng Ký Map
1.  Mở scene `MapSelectScene`.
2.  Tìm `MapSelectManager`.
3.  Thêm `MapData` mới vào danh sách.

---

## 3. Hệ Thống Input (Input System)

Dự án sử dụng **Unity Input System Package**.
- File cấu hình: `Assets/_Project/Settings/InputActions.inputactions`.
- Script quản lý: `InputManager.cs`.

### Cách thêm nút bấm mới:
1.  Mở file `InputActions`.
2.  Thêm Action mới vào Maps `Player1` và `Player2`.
3.  Generate C# Class (nếu chưa tự động).
4.  Vào `InputManager.cs`:
    - Khai báo biến `InputAction` mới.
    - Gán trong hàm `InitializeInput()`.
    - Thêm property `get` để các class khác truy cập.

---

## 4. Debug & Xử Lý Lỗi Thường Gặp

### Nhân vật rơi xuyên đất?
- Kiểm tra xem `Rigidbody2D` có để `Collision Detection` là `Continuous` không.
- Kiểm tra `BoxCollider2D` của đất có tắt `Is Trigger` không.

### Không đánh được?
- Kiểm tra `Cooldown` trong `CharacterData`.
- Bật Gizmos để xem phạm vi đánh (`OnDrawGizmosSelected` trong Controller).

### Game báo lỗi Null khi Start?
- Thường do `GameData` chưa có dữ liệu (khi bạn chạy trực tiếp Scene `MainGameScene` mà không qua Menu).
- `GameManager` có cơ chế fallback, nhưng tốt nhất hãy chạy từ `MainMenuScene`.
