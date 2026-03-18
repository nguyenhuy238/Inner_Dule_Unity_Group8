# Inner Duel - Tài Liệu Thiết Kế Kỹ Thuật (TDD)

## 1. TỔNG QUAN DỰ ÁN

**Tên Game:** Inner Duel  
**Thể loại:** 2D Fighting Game  
**Engine:** Unity  
**Ngôn ngữ:** C#  
**Nền tảng:** PC  

### Ý Tưởng
"Inner Duel" là một tựa game đối kháng 2D nơi các nhân vật đại diện cho những trạng thái nội tâm đối lập (ví dụ: Kỷ Luật vs Ngẫu Hứng). Vòng lặp gameplay bao gồm việc chọn đấu trường, chọn nhân vật cho hai người chơi, và tham gia vào một trận đấu tay đôi cho đến khi máu của một bên cạn kiệt.

### Vòng Lặp Gameplay (Gameplay Loop)
1.  **Chuẩn bị:** Người chơi chọn bản đồ và nhân vật của mình.
2.  **Chiến đấu:** Người chơi tham gia chiến đấu thời gian thực sử dụng di chuyển, tấn công thường, kỹ năng, đỡ đòn và lướt (dash).
3.  **Kết thúc:** Người chiến thắng được công bố ở màn hình kết quả, với các tùy chọn đấu lại hoặc quay về menu chọn nhân vật.

---

## 2. KIẾN TRÚC LUỒNG GAME (GAME FLOW)

Trò chơi tuân theo luồng chuyển cảnh tuyến tính được quản lý bởi các Manager cụ thể:

1.  **MainMenuScene:** Điểm bắt đầu. Truy cập cài đặt, thông tin (credits) và bắt đầu game.
2.  **MapSelectScene:** Người chơi chọn đấu trường (Map).
3.  **CharacterSelectScene:** Cả Player 1 và Player 2 chọn nhân vật của mình.
4.  **LoadingScene:** Tải scene gameplay bất đồng bộ trong khi hiển thị mẹo chơi.
5.  **MainGameScene:** Đấu trường chính nơi diễn ra trận đấu.
6.  **ResultScene:** Hiển thị người thắng và thống kê trận đấu.

---

## 3. CẤU TRÚC SCENE

### MainMenuScene
*   **Mục đích:** Màn hình chính.
*   **UI:** Main Menu Panel (Play, Options, Credits, Quit), Options Panel, Credits Panel.
*   **Script Chính:** `MainMenuManager.cs`.
*   **Tương tác:** Click chuột để điều hướng hoặc bắt đầu chọn bản đồ.

### MapSelectScene
*   **Mục đích:** Chọn đấu trường.
*   **UI:** Hình ảnh xem trước Map, Tên Map, Mô tả, Nút điều hướng.
*   **Script Chính:** `MapSelectManager.cs`.
*   **Tương tác:** Sử dụng phím hoặc nút UI để duyệt map; Xác nhận để lưu `selectedMap` vào `GameData`.

### CharacterSelectScene
*   **Mục đích:** Chọn nhân vật cho 2 người chơi.
*   **UI:** Khu vực chọn P1, Khu vực chọn P2, Chân dung nhân vật, Trạng thái (READY/SELECTING).
*   **Script Chính:** `CharacterSelectManager.cs`.
*   **Tương tác:** P1 và P2 chọn độc lập. Dữ liệu được lưu vào `GameData.player1Character` và `GameData.player2Character`.

### LoadingScene
*   **Mục đích:** Chuyển cảnh mượt mà.
*   **UI:** Thanh tiến trình, Text % tải, Mẹo ngẫu nhiên.
*   **Script Chính:** `LoadingSceneManager.cs`.
*   **Tương tác:** Tự động chuyển khi tải xong.

### MainGameScene
*   **Mục đích:** Gameplay chính.
*   **UI:** Thanh máu, Tên nhân vật, Thông báo Intro/Ending.
*   **Script Chính:** `GameManager.cs` (God Object), `UIManager.cs`, `CameraController.cs`.
*   **Tương tác:** Điều khiển nhân vật chiến đấu.

### ResultScene
*   **Mục đích:** Tổng kết trận đấu.
*   **UI:** Tên người thắng, Chân dung, Nút Rematch/Menu.
*   **Script Chính:** `ResultScreenManager.cs`.

---

## 4. HỆ THỐNG UI

Hệ thống UI được thiết kế theo hướng module, mỗi scene có Manager riêng.

*   **Điều hướng:** Sử dụng kết hợp chuột và phím.
*   **Lưu trữ:** Các UI Manager đọc/ghi dữ liệu vào `GameData` (lớp tĩnh) để duy trì trạng thái giữa các scene.
*   **Menu Tạm Dừng:** Được xử lý bởi `PauseMenuManager` trong `MainGameScene`, cho phép tạm dừng thời gian (`Time.timeScale = 0`) và khởi động lại trận đấu.

---

## 5. HỆ THỐNG NHÂN VẬT (CHARACTER SYSTEM)

Nhân vật được triển khai theo hướng Data-Driven (Dữ liệu điều hướng).

### CharacterData (ScriptableObject)
Nằm tại `Assets/_Project/Scripts/Character/CharacterData.cs` (định nghĩa) và `Assets/_Project/Data/` (dữ liệu). Chứa:
*   **Định danh:** `CharacterType` enum, Tên, Mô tả.
*   **Chỉ số:** Máu tối đa, Tốc độ, Lực nhảy.
*   **Chiến đấu:** Sát thương (Normal, Skill 1-3), Tầm đánh, Thời gian hồi chiêu.
*   **Cờ (Flags):** `canBlock`, `canDash`, `canCounterAttack`...
*   **Visuals:** Sprite mặc định, Animator Controller, Màu sắc.

### InnerCharacterController (Logic)
Script trung tâm điều khiển nhân vật:
*   **Input:** Nhận input từ `InputManager` dựa trên `playerID`.
*   **Movement:** Xử lý di chuyển vật lý, nhảy (Ground Check), và Dash.
*   **Combat:** Xử lý tấn công thường và 3 kỹ năng (Attack 1, 2, 3).
*   **State:** Quản lý máu, trạng thái chết, trạng thái bất tử (i-frames).
*   **Ability:** Tích hợp hệ thống `BaseCharacterAbility` để mở rộng kỹ năng.

### Quy Trình Khởi Tạo (Hiện Tại)
1.  `GameManager` đọc `GameData.player1Character` và `GameData.player2Character`.
2.  `GameManager` trực tiếp gọi `Instantiate` prefab được khai báo trong `CharacterData`.
3.  `GameManager` gán `playerID` và gọi `InitializeFromData()` trên controller mới tạo.
    *   *Lưu ý:* `CharacterFactory` đã được cài đặt nhưng chưa được `GameManager` sử dụng trong phiên bản hiện tại.

---

## 6. HỆ THỐNG BẢN ĐỒ (MAP SYSTEM)

### MapData (ScriptableObject)
*   Chứa: `mapName`, `previewImage`, `mapPrefab`, `description`.

### Map Loading
*   Trong `MainGameScene`, `GameManager` kiểm tra `GameData.selectedMap`.
*   Nếu có, nó sẽ `Instantiate` prefab bản đồ vào scene tại vị trí `Vector3.zero`.
*   Nếu không, nó sử dụng `fallbackMap`.

---

## 7. QUẢN LÝ DỮ LIỆU (DATA MANAGEMENT)

### GameData (Static Class)
Nằm tại `Assets/_Project/Scripts/Core/GameData.cs`.
Đóng vai trò "Blackboard" toàn cục:
*   **Input:** Chứa thông tin nhân vật (`CharacterData`) đã chọn.
*   **Map:** Chứa `MapData` đã chọn.
*   **Kết quả:** Lưu `winnerPlayerID` và `winnerName`.
*   **Constants:** Lưu tên các Scene để tránh hardcode string rải rác.

---

## 8. QUY TRÌNH KHỞI TẠO GAMEPLAY

Trong `MainGameScene`, `GameManager.InitializeGame()` thực hiện:
1.  **Spawn Map:** Tạo prefab bản đồ.
2.  **Spawn Player:**
    *   Tìm các placeholder player có sẵn trong scene (nếu có).
    *   Hoặc Instantiate prefab mới từ `GameData` tại vị trí của placeholder.
3.  **Setup Logic:**
    *   Gán Layer (Player1/Player2).
    *   Gán Opponent Layer mask.
    *   Khởi tạo chỉ số từ `CharacterData`.
4.  **Setup Camera:** Gán 2 transform của player vào `CameraController` (Cinemachine Target Group).
5.  **Setup UI:** Gán references player vào `UIManager` để hiển thị thanh máu.
6.  **Start Intro:** Bắt đầu đếm ngược, khóa di chuyển.

---

## 9. KIẾN TRÚC SCRIPT

| Script | Trách nhiệm chính |
| :--- | :--- |
| `GameData` | Lưu trữ trạng thái toàn cục (Static). |
| `MainMenuManager` | Xử lý UI và điều hướng ở Menu chính. |
| `InputManager` | Wrapper cho Unity Input System, xử lý input cho 2 người chơi. |
| `GameManager` | Quản lý vòng lặp game, Spawn Map/Player, Win/Loss condition. |
| `InnerCharacterController` | Điều khiển vật lý, animation và combat của nhân vật. |
| `CharacterFactory` | (Chưa dùng) Tiện ích để tạo nhân vật và gắn abilities động. |
| `BaseCharacterAbility` | Lớp cơ sở cho các kỹ năng rời rạc (Dash, Parry...). |
| `UIManager` | Hiển thị HUD (Máu, Tên) trong gameplay. |

---

*Tài liệu được cập nhật ngày 18/03/2026.*
*Phản ánh đúng mã nguồn phiên bản hiện tại.*
