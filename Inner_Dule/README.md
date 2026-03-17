# Inner Dule - Cuộc Chiến Nội Tâm

**Inner Dule** là một trò chơi đối kháng 2D được phát triển bằng Unity, tập trung vào sự cân bằng giữa các khía cạnh đối lập trong tâm trí con người. Người chơi điều khiển các nhân vật đại diện cho các trạng thái tâm lý khác nhau (Kỷ luật vs Ngẫu hứng, Lý trí vs Cảm xúc) để tìm kiếm sự hòa hợp (Harmony).

## 🚀 Công Nghệ Sử Dụng
- **Unity Engine**: 2022.3.x (hoặc mới hơn)
- **Input System Package**: Xử lý input đa nền tảng và local multiplayer.
- **Cinemachine**: Hệ thống camera động cho các trận đấu.
- **TextMesh Pro**: Hiển thị UI chất lượng cao.
- **Scriptable Objects**: Quản lý dữ liệu nhân vật và bản đồ.

## 🕹️ Gameplay Loop (Vòng lặp trò chơi)
1. **Khởi động**: `Bootstrap` khởi tạo các hệ thống cốt lõi.
2. **Menu chính**: Người chơi chọn chế độ chơi.
3. **Chọn nhân vật & Bản đồ**: Lưu lựa chọn vào `GameData`.
4. **Trận đấu**: `GameManager` khởi tạo bản đồ và nhân vật.
   - **Intro**: Giới thiệu các câu nói triết lý về nội tâm.
   - **Gameplay**: Chiến đấu thời gian thực.
   - **Ending**: Xác định người thắng và hiển thị thông điệp hòa hợp.
5. **Kết quả**: Hiển thị màn hình kết quả và quay lại Menu.

## 📂 Cấu Trúc Thư Mục Chính
- `Assets/_Project/Scripts/Core`: Các hệ thống nền tảng (GameData, Singleton, Bootstrap).
- `Assets/_Project/Scripts/Game`: Quản lý logic trận đấu (GameManager).
- `Assets/_Project/Scripts/Character`: Điều khiển nhân vật và hệ thống kỹ năng.
- `Assets/_Project/Scripts/UI`: Quản lý giao diện người dùng.
- `Assets/_Project/Scenes`: Các màn chơi và bản đồ đấu trường.
- `Assets/_Project/Data`: Các ScriptableObject chứa dữ liệu cấu hình.

## 🛠️ Cách Chạy Project
1. Mở project bằng Unity Hub.
2. Đảm bảo các Scene đã được thêm vào **Build Settings**.
3. Mở scene `MainMenuScene` hoặc `Bootstrap` để bắt đầu từ đầu.
4. Sử dụng phím `WASD` cho Player 1 và các phím mũi tên cho Player 2 (cấu hình trong `InputManager`).
