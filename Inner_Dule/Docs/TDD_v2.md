# Tài Liệu Thiết Kế Kỹ Thuật (TDD v2)

Tài liệu này mô tả chi tiết các hệ thống kỹ thuật hiện tại của Inner Dule, phản ánh mã nguồn thực tế trong dự án.

## 1. Kiến Trúc Tổng Thể (UPDATED)
Dự án sử dụng mô hình **Data-Driven** với ScriptableObjects cho dữ liệu nhân vật và bản đồ, kết hợp với các Singleton Manager để điều phối logic.

- **Dữ liệu**: `CharacterData`, `MapData` (ScriptableObjects).
- **Luân chuyển dữ liệu**: `GameData` (Static Class).
- **Điều khiển**: `GameManager`, `UIManager`, `InputManager`.

## 2. Luồng Scene (Scene Flow)
1. **Bootstrap**: Khởi tạo Singleton, tải `MainGameScene` (hoặc Menu).
2. **MainMenuScene**: Điều hướng đến Character Select hoặc Map Select.
3. **CharacterSelectScene**: Chọn nhân vật P1 và P2. Dữ liệu lưu vào `GameData.player1Character` và `player2Character`.
4. **MapSelectScene**: Chọn đấu trường. Dữ liệu lưu vào `GameData.selectedMap`.
5. **MainGameScene**: Nơi trận đấu diễn ra. `GameManager` đọc `GameData` để spawn.
6. **ResultScene**: Hiển thị kết quả thắng/thua.

## 3. Vòng đời GameManager (Lifecycle)
`GameManager` quản lý vòng đời của một trận đấu thông qua `GameState`:
- `Awake/Start`: Gọi `InitializeGame()`.
- `InitializeGame()`:
  - Gọi `SpawnMap()` để tải bản đồ từ `GameData`.
  - Thiết lập Player (Spawn từ prefab hoặc tìm object có sẵn).
  - Liên kết Camera với 2 Player.
  - Khởi tạo UI.
- `HandleIntroState()`: Hiển thị quote triết lý, khóa input nhân vật.
- `HandleGameplayState()`: Mở khóa input, kiểm tra điều kiện thắng (HP <= 0).
- `HandleEndingSequence()`: Khóa input, chạy hiệu ứng Harmony, chuyển scene kết quả.

## 4. Hệ Thống Nhân Vật (Real Implementation)
Hệ thống nhân vật hiện tại tập trung vào `InnerCharacterController`:
- **Thành phần**: Rigidbody2D, Animator, SpriteRenderer, StatusEffectManager.
- **Tấn công**:
  - `NormalAttack`: Đòn đánh cơ bản.
  - `Attack 1, 2, 3`: Các kỹ năng đặc biệt. Hỗ trợ cả tấn công cận chiến (Melee) và tầm xa (Projectile).
  - **Leap (Nhảy tới)**: Được hỗ trợ trong `Attack 3` thông qua `attack3LeapForce`.
- **Phòng thủ**: Hệ thống `Block` và `Dash` dựa trên cấu hình trong `CharacterData`.
- **Khả năng mở rộng**: Sử dụng `BaseCharacterAbility` để thêm các hiệu ứng đặc biệt như Parry (Đỡ đòn hoàn hảo), Counter (Phản công).

## 5. Hệ Thống Camera & UI
- **Camera**: Sử dụng Cinemachine (Target Group) để tự động zoom và bao quát cả hai nhân vật trong trận đấu.
- **UI**: `UIManager` quản lý thanh máu (`HealthBar`), text giới thiệu và kết thúc. Nó nhận thông tin từ `GameManager` để cập nhật trạng thái.

## 6. Thay Đổi So Với Version Cũ
- Tích hợp hệ thống Input mới (Input System Package) hỗ trợ Local Multiplayer ổn định hơn.
- Tách biệt hoàn toàn dữ liệu (`CharacterData`) khỏi logic (`InnerCharacterController`).
- Hệ thống `GameData` giúp việc chuyển đổi scene và duy trì lựa chọn của người chơi mượt mà hơn.
- Bổ sung hệ thống `StatusEffects` (đang hoàn thiện) để xử lý các trạng thái như Choáng (Stun).
