# Tổng Quan Hệ Thống (System Overview)

Tài liệu này cung cấp cái nhìn tổng quan về tình trạng hiện tại của dự án Inner Dule.

## 1. Các Hệ Thống Đang Hoạt Động (Functional)
- **Cốt lõi (Core)**:
  - Khởi tạo hệ thống (Bootstrap).
  - Quản lý trạng thái trận đấu (GameManager).
  - Lưu trữ và chuyển đổi dữ liệu giữa các Scene (GameData).
- **Nhân vật (Character)**:
  - Di chuyển (Move, Jump, Dash, Air Control).
  - Chiến đấu (Normal Attack, 3 Skills khác nhau).
  - Hệ thống HP, sát thương và cái chết.
  - Tấn công tầm xa (Projectile) cho các nhân vật tầm xa.
  - Tấn công nhảy tới (Leap) cho các nhân vật cận chiến.
- **Môi trường (Map)**:
  - Load bản đồ động từ ScriptableObject.
  - Hệ thống va chạm nền đất (Ground Layer).
- **UI**:
  - Thanh máu đồng bộ với nhân vật.
  - Màn hình Intro với hiệu ứng Typewriter.
  - Màn hình Ending và Harmony.

## 2. Các Hệ Thống Chưa Hoàn Thiện (In-Progress)
- **CharacterFactory**: Chưa được tích hợp hoàn toàn vào `GameManager`. Hiện tại `GameManager` đang tự tạo nhân vật thay vì sử dụng Factory.
- **Kỹ năng đặc biệt (Unique Abilities)**: Chỉ mới triển khai mẫu một số kỹ năng (Parry, Counter). Nhiều nhân vật chưa có kỹ năng đặc thù.
- **Hệ thống trạng thái (Status Effects)**: Mới chỉ có khung sườn, chưa áp dụng rộng rãi (ví dụ: Stun, Poison).
- **AI**: Hiện tại dự án chỉ hỗ trợ Local Multiplayer (PvP), chưa có AI cho chế độ PvE.
- **Âm thanh (Audio)**: Đã có `AudioManager` cơ bản nhưng chưa gắn đầy đủ SFX cho từng đòn đánh.

## 3. Rủi Ro Kỹ Thuật (Technical Risks)
- **Sự phụ thuộc vào Layer/Tag**: Hệ thống va chạm và nhảy (Ground Check) phụ thuộc vào Layer "Ground". Nếu không thiết lập đúng Layer cho bản đồ, nhân vật sẽ không thể nhảy.
- **Tham chiếu Null**: `GameManager` có thể bị lỗi nếu `GameData` không chứa thông tin nhân vật hợp lệ (mặc dù đã có code fallback).
- **GameManager quá lớn (God Object)**: `GameManager` đang đảm nhiệm quá nhiều việc (spawn map, spawn player, link UI, link camera, quản lý state). Cần tách nhỏ ra các module.
- **Hardcoded Strings**: Tên scene và tên layer hiện đang được dùng dưới dạng chuỗi (strings), dễ gây lỗi khi đổi tên trong Editor.
- **Xung đột Animation**: Các trạng thái tấn công và di chuyển có thể xung đột nếu Logic không đồng bộ chặt chẽ với Animator.
