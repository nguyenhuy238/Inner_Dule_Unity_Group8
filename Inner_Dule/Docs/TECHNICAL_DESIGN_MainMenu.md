# TÀI LIỆU KỸ THUẬT: HỆ THỐNG MAIN MENU - INNER DUEL

## 1. Tổng quan (Overview)
Hệ thống Main Menu là điểm khởi đầu của trò chơi "Inner Duel", cho phép người chơi điều hướng giữa các chức năng chính: Bắt đầu chơi (Play), Cài đặt (Options), Xem thông tin đội ngũ (Credits) và Thoát (Quit).

Hệ thống được thiết kế theo kiến trúc **Panel-based**, sử dụng một script quản lý duy nhất (`MainMenuManager`) để điều khiển việc hiển thị các lớp UI.

## 2. Kiến trúc UI (UI Architecture)
Cấu trúc phân cấp (Hierarchy) chuẩn trong Unity:

```text
Canvas
├── Background (Image - Hình nền game)
├── MainMenuPanel (GameObject)
│   ├── Logo (Image)
│   └── ButtonGroup (Vertical Layout Group)
│       ├── PlayButton (Button)
│       ├── OptionsButton (Button)
│       ├── CreditsButton (Button)
│       └── QuitButton (Button)
├── OptionsPanel (GameObject - Ẩn mặc định)
│   ├── Title (TextMeshPro - "SETTINGS")
│   ├── VolumeSettings
│   │   ├── MasterSlider (Slider)
│   │   ├── MusicSlider (Slider)
│   │   └── SFXSlider (Slider)
│   ├── VideoSettings
│   │   ├── ResolutionDropdown (TMP_Dropdown)
│   │   └── FullscreenToggle (Toggle)
│   └── BackButton (Button - Gọi ShowMainMenu)
└── CreditsPanel (GameObject - Ẩn mặc định)
    ├── Title (TextMeshPro - "CREDITS")
    ├── ScrollView (Chứa danh sách thành viên)
    └── BackButton (Button - Gọi ShowMainMenu)
```

## 3. Luồng hoạt động (Workflow)
1. **Khởi tạo (Start)**: 
   - `MainMenuManager` gọi `InitializeOptions()` để tải các cài đặt đã lưu từ `PlayerPrefs`.
   - Gọi `ShowMainMenu()` để đảm bảo chỉ có bảng menu chính hiển thị.
2. **Điều hướng**: 
   - Người chơi click vào các nút để chuyển đổi giữa các Panel thông qua các phương thức `ShowOptions()`, `ShowCredits()`.
   - Mỗi lần click đều phát âm thanh phản hồi (`PlayClickSound`).
3. **Cài đặt**: 
   - Các giá trị Volume, Độ phân giải, Toàn màn hình được lưu trực tiếp vào `PlayerPrefs` mỗi khi thay đổi.
4. **Chuyển cảnh**: 
   - Nút Play sẽ gọi `SceneManager.LoadScene` để chuyển sang màn hình chọn bản đồ (`MapSelectScene`).

## 4. Giải thích Script MainMenuManager
Script được viết trong namespace `InnerDuel.UI`, kế thừa từ `MonoBehaviour`.

- **Các biến Panel**: Lưu tham chiếu đến các GameObject của từng màn hình.
- **InitializeOptions()**: Thiết lập giá trị mặc định cho UI dựa trên cấu hình hệ thống hoặc dữ liệu đã lưu.
- **SetMasterVolume / SetResolution / SetFullscreen**: Các hàm callback được gán vào sự kiện `onValueChanged` của các thành phần UI tương ứng.
- **Audio Feedback**: Cung cấp hàm `PlayHoverSound()` (gán vào EventTrigger) và `PlayClickSound()` để tăng trải nghiệm người dùng (UX).

## 5. Hướng dẫn Setup trong Unity Editor (Cho Junior)

### Bước 1: Chuẩn bị Script
- Đảm bảo script `MainMenuManager.cs` đã được cập nhật code mới nhất.
- Kéo script này vào một GameObject trống tên là `MenuManager` trong Scene.

### Bước 2: Tạo UI Panels
1. Tạo một **Canvas** (nếu chưa có).
2. Tạo 3 GameObject con bên trong Canvas: `MainMenuPanel`, `OptionsPanel`, `CreditsPanel`.
3. Trong `OptionsPanel`, tạo các Slider cho Volume và một Dropdown (TextMeshPro) cho Resolution.

### Bước 3: Gán tham chiếu (Assign References)
1. Chọn GameObject `MenuManager`.
2. Trong cửa sổ **Inspector**, kéo các Panel tương ứng vào các ô:
   - `Main Menu Panel`
   - `Options Panel`
   - `Credits Panel`
3. Kéo các Slider và Dropdown vào các ô tương ứng trong phần **Options UI Elements**.
4. (Tùy chọn) Kéo một `AudioSource` và các file âm thanh vào phần **Audio Settings**.

### Bước 4: Gán sự kiện cho Nút (Button OnClick)
1. Chọn nút **Play**: Click dấu `+` trong `OnClick()`, kéo `MenuManager` vào và chọn `MainMenuManager -> PlayGame`.
2. Chọn nút **Options**: Chọn `MainMenuManager -> ShowOptions`.
3. Chọn nút **Credits**: Chọn `MainMenuManager -> ShowCredits`.
4. Chọn nút **Quit**: Chọn `MainMenuManager -> QuitGame`.
5. Trong `OptionsPanel` và `CreditsPanel`, tìm nút **Back** và gán `MainMenuManager -> ShowMainMenu`.

### Bước 5: Thêm Hiệu ứng Hover (UX)
1. Chọn các nút trong menu chính.
2. Thêm component **Event Trigger**.
3. Click **Add New Event Type** -> **Pointer Enter**.
4. Gán `MenuManager` vào và chọn `MainMenuManager -> PlayHoverSound`.

## 6. Gợi ý cải tiến (Optional)
- **Animation**: Sử dụng `DOTween` để tạo hiệu ứng Panel trượt từ cạnh màn hình vào hoặc hiệu ứng Fade Alpha.
- **Background**: Thêm hiệu ứng hạt (Particle System) hoặc ảnh nền động (Parallax) để menu sinh động hơn.
- **Localization**: Tích hợp hệ thống đa ngôn ngữ nếu game phát triển ra quốc tế.
