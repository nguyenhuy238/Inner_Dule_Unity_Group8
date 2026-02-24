# Hướng dẫn Fix lỗi sau khi Clone – Inner Duel

Khi clone project về lần đầu, có thể gặp một số lỗi do thiếu references trong Unity Editor. File này hướng dẫn cách xử lý.

---

## Lỗi thường gặp & cách fix

### 1. Lỗi "Missing script" hoặc "NullReferenceException" ở Prefab

**Triệu chứng:** Console hiện lỗi đỏ khi nhấn Play, liên quan đến prefab nhân vật.

**Nguyên nhân:** Biến `attackPoint` hoặc `opponentLayer` chưa được gán trên prefab.

**Cách fix:**
1. Mở prefab `Assets/_Project/Prefabs/Prefabs/Characters/Discipline_Character.prefab`
2. Trong Hierarchy, tìm child object `AttackPoint`
3. Chọn object cha `Discipline_Character`
4. Kéo `AttackPoint` vào ô **Attack Point** của `InnerCharacterController` trong Inspector
5. Set **Opponent Layer** = `Player2` (cho P1) hoặc `Player1` (cho P2)
6. Lặp lại cho `Spontaneity_Character.prefab`

---

### 2. CharacterFactory thiếu Prefab references

**Triệu chứng:** Game chạy nhưng không spawn được nhân vật, hoặc dùng fallback.

**Cách fix:**
1. Mở scene `MainGameScene`
2. Chọn object `CharacterFactory` trong Hierarchy
3. Kéo prefab từ `Assets/_Project/Prefabs/Prefabs/Characters/` vào các slot:
   - **Discipline Prefab** → `Discipline_Character.prefab`
   - **Spontaneity Prefab** → `Spontaneity_Character.prefab`

---

### 3. Warning "Using fallback prefab for [Character]"

**Triệu chứng:** Console hiện warning vàng, game vẫn chạy nhưng dùng nhân vật thay thế.

**Nguyên nhân:** Prefab cho nhân vật đó chưa tồn tại.

**Cách fix ngắn hạn:** Bỏ qua warning – hệ thống fallback sẽ dùng Discipline prefab thay thế.

**Cách fix đúng:** Tạo prefab mới cho nhân vật cần thiết:
1. Duplicate `Discipline_Character.prefab` (Ctrl+D)
2. Đổi tên theo format `[TenNhanVat]_Character.prefab`
3. Mở prefab → sửa `CharacterData > Type` thành loại nhân vật tương ứng
4. Gán vào `CharacterFactory` trong scene

---

### 4. Health Bar không hiển thị

**Triệu chứng:** Thanh máu không cập nhật khi bị đánh.

**Cách fix:**
1. Kiểm tra Canvas trong scene có `Health Bar` objects
2. Đảm bảo `UIManager` có reference đến `player1HealthBar` và `player2HealthBar`
3. `UIManager.InitializeWithPlayers()` sẽ tự động kết nối khi game start

---

### 5. Camera không tracking đúng

**Triệu chứng:** Camera không zoom hay theo dõi nhân vật.

**Cách fix:**
1. Chọn object chứa `CameraController`
2. Đảm bảo có `CinemachineVirtualCamera` trong scene
3. Nếu thiếu `CinemachineTargetGroup`, script sẽ tự tạo
4. `GameManager.InitializeGame()` sẽ tự gọi `CameraController.SetTargets()`

---

### 6. Input không hoạt động

**Triệu chứng:** Nhấn phím không có phản hồi.

**Cách fix:**
1. Đảm bảo `InputManager` singleton tồn tại (kiểm tra Hierarchy)
2. Kiểm tra `Edit > Project Settings > Input System Package` đã active
3. Nếu chạy từ scene Bootstrap, input sẽ được khởi tạo tự động

---

> [!TIP]
> **Mẹo nhanh:** Khi gặp lỗi đỏ, nhấp đúp vào lỗi trong Console → Unity sẽ highlight chính xác object/script gây lỗi. Hầu hết lỗi đều do thiếu reference trong Inspector.
