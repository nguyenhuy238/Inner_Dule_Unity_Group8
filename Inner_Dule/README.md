# INNER DUEL: THE PATH TO HARMONY

> *"Cuộc chiến nội tâm không phải để tiêu diệt, mà để tìm thấy sự cân bằng giữa các thái cực của bản thân."*

## Tổng quan

**INNER DUEL** là game đối kháng 2D cho 2 người chơi local, lấy chủ đề về cuộc chiến nội tâm bên trong con người. Người chơi điều khiển các "Bản ngã" đại diện cho những mặt đối lập trong suy nghĩ, với mục tiêu tìm kiếm sự cân bằng và hòa hợp.

| Thông tin | Chi tiết |
|:---|:---|
| **Thể loại** | 2D Fighting – Local Multiplayer (2P) |
| **Art Style** | Silhouette / Shadow Art |
| **Engine** | Unity 2022.3 LTS |
| **Trạng thái** | Prototype / Alpha |

---

## Bắt đầu nhanh (Quick Start)

### Yêu cầu
- **Unity 2022.3 LTS** trở lên
- **Git** + **Git LFS** đã cài đặt

### Cài đặt

```bash
# 1. Clone repository
git clone https://github.com/<org>/Inner_Dule_Unity_Group8.git
cd Inner_Dule_Unity_Group8/Inner_Dule

# 2. Pull LFS files (textures, audio...)
git lfs pull

# 3. Mở project trong Unity Hub → Add → chọn thư mục Inner_Dule
```

### Chạy game

1. Mở scene **`Assets/_Project/Scenes/Scenes/MainGameScene.unity`**
2. Nhấn **Play** ▶️
3. Dùng bàn phím để điều khiển 2 nhân vật (xem phần **Điều khiển** bên dưới)

> [!IMPORTANT]
> **Luồng chạy đúng**: Bắt đầu từ scene **Bootstrap** (nếu có trong Build Settings) để đảm bảo các Manager được khởi tạo đúng. Nếu chỉ test nhanh, mở trực tiếp `MainGameScene` cũng hoạt động nhờ Singleton auto-create.

---

## Hệ thống nhân vật

Game có **8 nhân vật** chia thành **4 cặp thái cực đối lập**:

| Cặp | Nhân vật 1 | Nhân vật 2 |
|:---|:---|:---|
| Kỷ Luật vs Ngẫu Hứng | **The Warden** – Chậm, chắc chắn, block | **The Maverick** – Nhanh, linh hoạt, dash |
| Lý Trí vs Sáng Tạo | **The Architect** – Tầm xa, bẫy | **The Muse** – Tầm đánh thay đổi |
| Kiên Trì vs Từ Bỏ | **The Unbroken** – Càng bị đánh càng mạnh | **The Void** – Hút máu |
| Tĩnh Lặng vs Thịnh Nộ | **The Zen** – Phản đòn | **The Berserker** – Berserk mode |

> [!NOTE]
> Hiện tại chỉ có **Discipline (Warden)** và **Spontaneity (Maverick)** có prefab. 6 nhân vật còn lại cần được tạo thêm.

---

## Điều khiển

| Hành động | Player 1 (Trái) | Player 2 (Phải) |
|:---|:---|:---|
| **Di chuyển** | `W` `A` `S` `D` | `↑` `←` `↓` `→` |
| **Tấn công** | `Space` | `Right Shift` |
| **Phòng thủ** | `Left Shift` | `Right Ctrl` |
| **Lướt (Dash)** | `Left Ctrl` | `Enter` |

---

## Cấu trúc dự án

```
Inner_Dule/
├── Assets/
│   └── _Project/                  ← Root asset của dự án
│       ├── Scripts/
│       │   ├── Core/              ← Singleton, Bootstrap, InputManager, AudioManager
│       │   ├── Game/              ← GameManager (state machine)
│       │   ├── Character/         ← CharacterController, Factory, HealthBar, Data
│       │   ├── Camera/            ← CameraController (Cinemachine)
│       │   ├── UI/                ← UIManager
│       │   └── Effects/           ← ParticleEffectsManager
│       ├── Scenes/Scenes/         ← MainGameScene, SampleScene
│       ├── Prefabs/Prefabs/Characters/ ← Discipline, Spontaneity prefabs
│       ├── Art/Animations/        ← Animation assets
│       ├── Audio/                 ← SFX & Music (chưa có file)
│       └── Settings/              ← Input Actions config
├── ARCHITECTURE.md                ← Kiến trúc chi tiết
├── CONTRIBUTING.md                ← Quy trình làm việc team
├── CODING_CONVENTIONS.md          ← Chuẩn viết code
├── FIX_GUIDE.md                   ← Hướng dẫn fix lỗi Unity Editor
├── GAME_DEVELOPMENT_ROADMAP.md    ← Roadmap & tính năng cần phát triển
├── .gitignore                     ← Git ignore cho Unity
└── .gitattributes                 ← Git LFS & Unity YAML merge
```

> Chi tiết kiến trúc: xem [ARCHITECTURE.md](ARCHITECTURE.md)
> Quy trình phát triển: xem [CONTRIBUTING.md](CONTRIBUTING.md)

---

## Packages sử dụng

| Package | Version | Mục đích |
|:---|:---|:---|
| Input System | 1.7.0+ | Xử lý input 2 người chơi |
| Cinemachine | 2.9.7+ | Camera tracking tự động |
| TextMeshPro | 3.0.7+ | Render text UI |
| 2D Animation | 9.2.0 | Sprite animation |

---

## Tài liệu liên quan

| File | Nội dung |
|:---|:---|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Kiến trúc hệ thống, namespaces, class diagram |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Git workflow, branching, quy trình PR |
| [CODING_CONVENTIONS.md](CODING_CONVENTIONS.md) | Chuẩn đặt tên, tổ chức code |
| [FIX_GUIDE.md](FIX_GUIDE.md) | Hướng dẫn fix lỗi khi clone về |
| [GAME_DEVELOPMENT_ROADMAP.md](GAME_DEVELOPMENT_ROADMAP.md) | Roadmap phát triển tính năng |

---

## Credits

- **Concept & Design**: Group 8
- **Programming**: Group 8
- **Art & Assets**: [Asset Sources]
- **Audio**: [Audio Sources]

## License

This project is for educational purposes only.
