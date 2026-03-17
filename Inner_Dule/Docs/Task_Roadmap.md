# Lộ Trình Phát Triển (Task Roadmap)

Danh sách các nhiệm vụ cần thực hiện để hoàn thiện dự án Inner Dule.

## 🚨 Critical (Ưu tiên cao - Cần fix ngay)
1. **Tích hợp CharacterFactory vào GameManager**
   - **Mô tả**: Thay thế code `Instantiate` trực tiếp trong `GameManager` bằng `CharacterFactory.Instance.CreateCharacter()`.
   - **Lý do**: Đảm bảo các kỹ năng đặc thù (`UniqueAbilities`) được gán đúng cách cho từng nhân vật.
   - **Hướng fix**: Sửa hàm `InitializeGame()` trong `GameManager.cs`.

2. **Fix lỗi Ground Check trên các Map mới**
   - **Mô tả**: Đảm bảo tất cả các prefab bản đồ đều có Layer "Ground" cho các vật thể nền đất.
   - **Lý do**: Ngăn chặn tình trạng nhân vật không thể nhảy khi đổi bản đồ.
   - **Hướng fix**: Kiểm tra và cập nhật các Prefab trong `Assets/_Project/Prefabs/Maps`.

3. **Cập nhật Input Action cho 2 người chơi**
   - **Mô tả**: Thiết lập hoàn chỉnh `Input Action Asset` để tách biệt input cho P1 (Keyboard) và P2 (Gamepad hoặc Keyboard khác).
   - **Lý do**: Tránh xung đột điều khiển giữa hai người chơi.

## ✨ Important (Quan trọng - Cải thiện trải nghiệm)
1. **Hoàn thiện Unique Abilities cho tất cả nhân vật**
   - **Mô tả**: Triển khai các script kỹ năng cho Logic, Creativity, Stillness, Rage.
   - **Lý do**: Tạo sự khác biệt thực sự trong lối chơi giữa các nhân vật.
   - **Hướng fix**: Tạo các class kế thừa từ `BaseCharacterAbility`.

2. **Hệ thống Audio (SFX & BGM)**
   - **Mô tả**: Gắn âm thanh cho các hành động: Tấn công, Nhảy, Trúng đòn, Dash.
   - **Lý do**: Tăng tính phản hồi (feedback) và cảm giác lực (impact) khi chiến đấu.
   - **Hướng fix**: Thêm AudioSource vào Prefab nhân vật và gọi thông qua `AudioManager`.

3. **Refactor GameManager**
   - **Mô tả**: Tách `GameManager` ra các module nhỏ hơn.
   - **Lý do**: Giảm sự phức tạp, dễ bảo trì và mở rộng sau này.

## 🎯 Nice-to-have (Mở rộng - Thêm tính năng)
1. **Hệ thống AI đơn giản (CPU Opponent)**
   - **Mô tả**: Tạo một `AIController` cơ bản có thể tự di chuyển và tấn công.
   - **Lý do**: Cho phép người chơi luyện tập khi không có bạn chơi cùng.
2. **Hiệu ứng hình ảnh (VFX) nâng cao**
   - **Mô tả**: Thêm các hạt (Particles) khi dash, khi va chạm hoặc khi sử dụng kỹ năng đặc biệt.
   - **Lý do**: Làm cho trò chơi trông chuyên nghiệp và bắt mắt hơn.
3. **Chế độ Story (Cốt truyện)**
   - **Mô tả**: Chuỗi các trận đấu với các đoạn thoại dẫn dắt về sự hòa hợp nội tâm.
   - **Lý do**: Tăng chiều sâu nội dung cho trò chơi.
