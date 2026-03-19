# Kiến Trúc Hệ Thống (Architecture)

Tài liệu này mô tả cấu trúc phân tầng và luồng dữ liệu thực tế của dự án Inner Duel.

## 1. Sơ Đồ Kiến Trúc Thực Tế

Hệ thống được chia thành các lớp (Layers) nhưng ranh giới chưa thực sự cứng (còn phụ thuộc chéo).

```text
[ Core Layer ]
   ├── Singleton<T> (Base Class cho Managers)
   ├── GameData (Static Data Container - Blackboard)
   ├── InputManager (Wrapper cho Unity Input System)
   └── AudioManager (Quản lý âm thanh)

[ Logic Layer ]
   ├── GameManager (Central Controller - God Object)
   ├── InnerCharacterController (Character State Machine & Combat Logic)
   ├── BaseCharacterAbility (Abilities mở rộng)
   └── CharacterFactory (Utility tạo nhân vật - Chưa được tích hợp hoàn toàn)

[ Data Layer (ScriptableObjects) ]
   ├── CharacterData (Stats, Prefabs, Visual configs)
   ├── MapData (Map Prefab, Info)
   └── InputActions (Unity Input Asset)

[ Presentation Layer (UI & View) ]
   ├── MainMenuManager, MapSelectManager, CharacterSelectManager (Scene UIs)
   ├── UIManager (In-Game HUD)
   ├── CameraController (Cinemachine Logic)
   └── ResultScreenManager (End Game UI)
```

## 2. Luồng Dữ Liệu (Data Flow)

### Quá trình Khởi tạo Trận đấu
1.  **Selection**: Người chơi chọn Map và Nhân vật. ScriptableObject (`CharacterData`, `MapData`) được gán vào biến static trong `GameData`.
2.  **Scene Load**: `MainGameScene` được tải.
3.  **Initialization**:
    - `GameManager` thức dậy (`Start/InitializeGame`).
    - Đọc `GameData.selectedMap` -> Instantiate Map Prefab.
    - Đọc `GameData.player1Character` -> Instantiate Player Prefab.
    - `GameManager` lấy tham chiếu của `InputManager`, `UIManager`, `CameraController` và liên kết chúng lại với nhau (Dependency Injection thủ công).

### Vòng lặp Update (Frame Loop)
1.  **Input**: `InputManager` đọc tín hiệu từ thiết bị, cập nhật trạng thái các biến bool/vector.
2.  **Logic**: `InnerCharacterController` đọc `InputManager`.
    - Xử lý Physics (`Rigidbody2D`).
    - Xử lý Cooldowns.
    - Kích hoạt Animation (`Animator`).
3.  **Visuals**:
    - `CameraController` bám theo vị trí nhân vật.
    - `UIManager` đọc máu hiện tại từ Controller để cập nhật thanh máu.

## 3. Phân Tích Component Chính

### InnerCharacterController
Đây là script quan trọng nhất, xử lý mọi hành vi của nhân vật.
- **Dependencies**: `Rigidbody2D`, `Animator`, `InputManager`, `CharacterData`.
- **Cơ chế**: Sử dụng một máy trạng thái đơn giản (bool flags: `isAttacking`, `isGrounded`, `isBlocking`) để quản lý hành vi.

### GameManager
Script quản lý dòng thời gian của trận đấu.
- **Dependencies**: Hầu hết các manager khác.
- **State Machine**: Intro -> Gameplay -> Ending -> Menu.

## 4. Các Vấn Đề Kiến Trúc Cần Lưu Ý
- **Couple chặt chẽ (Tight Coupling)**: `InnerCharacterController` đang tham chiếu trực tiếp tới `InputManager.Instance`. Điều này làm khó việc test tách biệt.
- **Logic Phân Tán**: Logic sinh nhân vật (Spawning) đang nằm ở `GameManager` thay vì `CharacterFactory`.
- **Hard Dependency**: `GameManager` tìm kiếm các thành phần UI bằng `FindObjectOfType`, có thể gây chậm khi khởi tạo scene.
