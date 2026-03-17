# Hướng Dẫn Cho Nhà Phát Triển (Developer Guide)

Tài liệu này giúp các nhà phát triển mới làm quen và mở rộng dự án Inner Dule.

## 1. Cách Thêm Nhân Vật Mới
Để thêm một nhân vật mới vào trò chơi:
1. **Tạo ScriptableObject**:
   - Trong cửa sổ Project, chuột phải chọn `Create > InnerDuel > Character > CharacterData`.
   - Thiết lập các thông số (HP, Speed, Damage) và gán Prefab/Animator.
2. **Cập nhật CharacterType**:
   - Mở file `CharacterType.cs`, thêm tên nhân vật mới vào enum `CharacterType`.
3. **Cấu hình Prefab**:
   - Prefab nhân vật phải có component `InnerCharacterController`.
   - Thiết lập các `Attack Points` (vị trí đòn đánh) và `Ground Check`.
4. **Thêm Kỹ năng đặc biệt**:
   - Tạo script mới kế thừa từ `BaseCharacterAbility`.
   - Gắn script này vào Prefab nhân vật.
5. **Đăng ký vào CharacterFactory** (Nếu muốn sử dụng Factory):
   - Mở `CharacterFactory` trong scene, gán Prefab mới vào mảng dữ liệu.

## 2. Cách Thêm Đấu Trường (Map) Mới
1. **Tạo Map Prefab**:
   - Đảm bảo các nền đất (Platforms) có **BoxCollider2D** và được gán Layer là **Ground**.
2. **Tạo ScriptableObject**:
   - Chuột phải chọn `Create > InnerDuel > Game > MapData`.
   - Gán Prefab vừa tạo và đặt tên hiển thị.
3. **Đăng ký**:
   - Thêm MapData mới vào danh sách trong `MapSelectManager` hoặc `GameManager` (nếu cần).

## 3. Cách Debug Các Lỗi Thường Gặp
### Lỗi 1: Player không thể Nhảy (Jump)
- **Nguyên nhân**: Layer của mặt đất chưa được gán là "Ground" hoặc component `InnerCharacterController` chưa được thiết lập Layer này.
- **Cách fix**: Kiểm tra Layer của các Platform trong Prefab bản đồ và field `groundLayer` trong `InnerCharacterController`.

### Lỗi 2: Camera không đi theo nhân vật
- **Nguyên nhân**: `GameManager` không tìm thấy `CameraController` hoặc không gán được Target.
- **Cách fix**: Kiểm tra xem trong scene `MainGameScene` có `CameraController` chưa và nó có được gán vào `GameManager` không.

### Lỗi 3: UI Thanh máu không cập nhật
- **Nguyên nhân**: `UIManager` không tìm thấy Player hoặc `HealthBar` không được gán đúng.
- **Cách fix**: Kiểm tra log console để xem `InitializeWithPlayers` có được gọi thành công không.

## 4. Quy Tắc Coding Trong Project
- **Namespace**: Tất cả script phải nằm trong namespace `InnerDuel` (hoặc các sub-namespace như `InnerDuel.Characters`, `InnerDuel.Core`).
- **Singleton**: Sử dụng `Singleton<T>` cho các Manager toàn cục. Luôn dùng `InstanceSafe()` hoặc kiểm tra null trước khi truy cập.
- **Input**: Luôn sử dụng `InputManager` thay vì gọi trực tiếp `Keyboard.current` hoặc `Input.GetButton`.
- **Dữ liệu**: Không bao giờ hardcode chỉ số nhân vật trong script, hãy dùng `CharacterData`.
- **Log**: Sử dụng tiền tố `[InnerDuel]` trong `Debug.Log` để dễ dàng lọc log trong console.
