# Lộ Trình Phát Triển (Task Roadmap)

Cập nhật ngày 18/03/2026. Phân loại dựa trên audit mã nguồn thực tế.

## 🚨 Nợ Kỹ Thuật (Technical Debt) - Cần Xử Lý Trước

### 1. Tích hợp CharacterFactory (Quan trọng)
- **Tình trạng**: `CharacterFactory` đã có code nhưng `GameManager` chưa dùng.
- **Tác vụ**:
  - Sửa `GameManager.InitializeGame()` để gọi `CharacterFactory.Instance.CreateCharacter()`.
  - Loại bỏ logic `Instantiate` thủ công trong GameManager.
  - Điều này giúp tự động gắn các script `UniqueAbility` (như `Ability_DisciplineParry`) vốn đang được logic Factory xử lý.

### 2. Refactor Hardcoded Combat Logic
- **Tình trạng**: `InnerCharacterController.PerformAttack` đang chứa logic kiểm tra cứng (`if attackIndex == 3...`).
- **Tác vụ**: Chuyển logic Leap và Projectile sang các class `BaseCharacterAbility` riêng biệt để Controller gọn nhẹ hơn.

### 3. Input System Script Execution Order
- **Tình trạng**: Rủi ro `LateUpdate` của InputManager chạy trước logic của nhân vật, làm mất input.
- **Tác vụ**: Cấu hình `Script Execution Order` để `InputManager` luôn chạy trước các script khác.

---

## 🛠 Tính Năng Cần Hoàn Thiện (To-Do)

### 1. Hệ thống Âm Thanh (Audio Feedback)
- Thêm SFX cho: Chém trúng, Đỡ đòn thành công, Bước chân.
- Tích hợp vào Animation Events.

### 2. Visual Effects (VFX)
- Thêm hiệu ứng hạt khi Dash.
- Hiệu ứng tóe lửa khi vũ khí va chạm.

### 3. Cân bằng Game (Balancing)
- Tinh chỉnh thông số trong các file `CharacterData` (hiện tại các số liệu damage/speed đang là placeholder).

---

## ✅ Đã Hoàn Thành (Done)

- [x] Cơ chế di chuyển cơ bản (Move, Jump, Dash).
- [x] Hệ thống Combat cơ bản (Attack, Block, HP).
- [x] Luồng game (Menu -> Map Select -> Character Select -> Game -> Result).
- [x] Hệ thống Data (GameData, MapData, CharacterData).
- [x] Hỗ trợ 2 người chơi cục bộ (Local Multiplayer).
