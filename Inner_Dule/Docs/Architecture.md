# Kiến Trúc Hệ Thống (Architecture)

Tài liệu này mô tả cấu trúc phân tầng và sự tương tác giữa các thành phần trong dự án.

## 1. Sơ đồ Kiến trúc (Mô tả bằng Text)

```text
[ Core Layer ]
   ├── Singleton<T> (Base)
   ├── Bootstrap (Khởi tạo)
   ├── GameData (Static - Truyền dữ liệu giữa các Scene)
   └── InputManager (Xử lý đầu vào)

[ Logic Layer ]
   ├── GameManager (Quản lý trạng thái trận đấu)
   ├── InnerCharacterController (Điều khiển nhân vật)
   └── BaseCharacterAbility (Hệ thống kỹ năng mở rộng)

[ Data Layer (ScriptableObjects) ]
   ├── CharacterData (Chỉ số, Prefab, Visuals)
   └── MapData (Prefab đấu trường, Preview)

[ Presentation Layer (UI/Camera) ]
   ├── UIManager (Thanh máu, Text, Panels)
   └── CameraController (Cinemachine)
```

## 2. Luồng Dữ Liệu (GameData Flow)
`GameData` đóng vai trò là cầu nối giữa các giai đoạn của trò chơi:

1. **Character Select**: `player1Character` và `player2Character` được gán vào `GameData`.
2. **Map Select**: `selectedMap` được gán vào `GameData`.
3. **Loading**: Scene `LoadingScene` có thể hiển thị thông tin từ `GameData`.
4. **Gameplay**: `GameManager` đọc `GameData` để Instantiate nhân vật và bản đồ.
5. **Result**: `winnerName` được ghi vào `GameData` sau khi trận đấu kết thúc.

## 3. Phân Tích Vấn Đề Hiện Tại

### GameManager (God Object)
- **Vấn đề**: `GameManager` hiện tại đang thực hiện quá nhiều nhiệm vụ (từ spawn nhân vật, link camera cho đến quản lý intro/ending).
- **Gợi ý Refactor**: Chia nhỏ `GameManager` thành các thành phần:
  - `MatchInitializer`: Chỉ phụ trách spawn nhân vật và map.
  - `MatchFlowManager`: Chỉ phụ trách chuyển đổi giữa Intro -> Play -> Ending.
  - `MatchConditionChecker`: Phụ trách kiểm tra điều kiện thắng/thua.

### CharacterFactory Không Được Sử Dụng
- **Vấn đề**: `GameManager` đang tự tay instantiate prefabs, làm mất đi lợi ích của `CharacterFactory` (như tự động gán abilities đặc thù).
- **Gợi ý Fix**: Cần cập nhật `GameManager` để sử dụng `CharacterFactory.Instance.CreateCharacter()` thay vì gọi `Instantiate` trực tiếp.

### Hệ Thống Bản Đồ (Hybrid)
- **Vấn đề**: Việc quản lý bản đồ đang nửa vời giữa việc chọn scene và spawn prefab.
- **Gợi ý Fix**: Thống nhất hệ thống bản đồ dưới dạng các Prefab độc lập. Mỗi `MapData` chỉ chứa một Prefab, và `GameManager` sẽ spawn prefab đó vào một scene "Arena" trống duy nhất. Điều này giúp giảm số lượng scene cần bảo trì.

## 4. Sự Phụ Thuộc (Dependencies)
- **InnerCharacterController** phụ thuộc vào: `CharacterData`, `InputManager`, `StatusEffectManager`.
- **GameManager** phụ thuộc vào: `GameData`, `UIManager`, `CameraController`.
- **UIManager** phụ thuộc vào: `InnerCharacterController` (để lấy HP).
