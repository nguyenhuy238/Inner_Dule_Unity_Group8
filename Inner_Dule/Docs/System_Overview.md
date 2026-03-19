# Tổng Quan Hệ Thống (System Overview)

Tài liệu này phản ánh chính xác tình trạng thực tế của mã nguồn dự án Inner Duel tính đến ngày 18/03/2026.

## 1. Các Hệ Thống Đang Hoạt Động (Functional)

### Cốt lõi (Core)
- **Singleton Pattern**: Hệ thống `Singleton<T>` hoạt động ổn định cho các Manager.
- **GameManager**: Đang hoạt động như một "God Object", quản lý toàn bộ vòng đời của trận đấu (Intro -> Gameplay -> Ending).
- **GameData**: Truyền tải dữ liệu (Nhân vật, Map) giữa các Scene thành công.
- **InputManager**: Xử lý input cho 2 người chơi cục bộ (Local Multiplayer) thông qua Unity New Input System.

### Nhân vật (Character)
- **Cơ chế vật lý**: Di chuyển, nhảy (có Ground Check), và trọng lực hoạt động tốt.
- **Combat cơ bản**:
  - Tấn công thường (Normal Attack).
  - 3 Kỹ năng (Attack 1, 2, 3) có cooldown và thông số riêng.
  - Cơ chế Leap (nhảy tới) ở Attack 3 (được cấu hình trong `CharacterData`).
  - Cơ chế Projectile (đạn) cho các nhân vật tầm xa (Player 2/Creativity).
- **Phòng thủ**: Đỡ đòn (Block) giảm sát thương, Lướt (Dash) để né tránh.
- **Trạng thái**: Hệ thống máu, chết, và bất tử tạm thời (i-frames) khi dính đòn.

### Môi trường & Bản đồ
- **Map Loading**: Load map dựa trên Prefab được lưu trong `GameData`.
- **Scene Flow**: Chuyển cảnh từ Menu -> Map Select -> Character Select -> Loading -> Game -> Result hoạt động đúng luồng.

### UI
- **HUD**: Thanh máu cập nhật theo thời gian thực.
- **Menu**: Các menu chính, menu chọn map/nhân vật hoạt động đầy đủ chức năng điều hướng.

## 2. Các Hệ Thống Chưa Hoàn Thiện / Đang Phát Triển (In-Progress)

- **CharacterFactory**: Class này đã tồn tại và có logic gắn Abilities động (`AddUniqueAbilities`), nhưng **CHƯA** được `GameManager` sử dụng. Hiện tại `GameManager` vẫn đang `Instantiate` prefab một cách thủ công.
- **Abilities Modun hóa**: Mặc dù đã có `BaseCharacterAbility`, nhưng logic tấn công chính (`PerformAttack`) trong `InnerCharacterController` vẫn còn chứa nhiều logic cứng (hardcoded switch case) thay vì ủy quyền hoàn toàn cho hệ thống Ability.
- **Hệ thống Audio**: `AudioManager` đã có nhưng việc tích hợp âm thanh vào từng hành động (chém, trúng đòn) chưa đồng bộ hoàn toàn trên tất cả nhân vật.

## 3. Rủi Ro Kỹ Thuật (Technical Risks) & Nợ Kỹ Thuật (Technical Debt)

### GameManager quá tải (God Object)
`GameManager` hiện đang làm quá nhiều việc:
- Quản lý State (Intro/Game/End).
- Spawn Map & Player.
- Link UI & Camera.
- Xử lý Win/Loss.
**Rủi ro**: Khó bảo trì, khó unit test, dễ gây lỗi dây chuyền khi sửa một tính năng nhỏ.

### Hardcoded Logic trong Controller
Trong `InnerCharacterController.cs`:
- Logic xử lý Projectile đang kiểm tra cứng: `if (attackIndex == 1 || attackIndex == 3)`.
- Điều này làm giảm tính linh hoạt nếu muốn tạo ra một nhân vật có bộ skill khác biệt (ví dụ: projectile ở Attack 2).

### Sự phụ thuộc vào Layer
- Hệ thống nhảy (`CheckGround`) phụ thuộc tuyệt đối vào Layer "Ground". Nếu Level Designer quên set Layer cho prefab map mới, nhân vật sẽ không thể nhảy.

### Input System Reset
- `InputManager` đang sử dụng `LateUpdate` để reset cờ `JumpPressed`. Cần đảm bảo thứ tự thực thi script (Script Execution Order) để tránh việc Input bị reset trước khi `InnerCharacterController` kịp đọc.
