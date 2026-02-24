# Roadmap Phát triển – Inner Duel

Tài liệu tracking tiến độ và kế hoạch phát triển game.

**Trạng thái hiện tại:** Prototype / Alpha
**Cập nhật lần cuối:** 2026-02-24

---

## ĐÃ HOÀN THÀNH ✅

### Kiến trúc & Cơ sở hạ tầng
- [x] **Singleton Pattern** – Base class dùng chung, thread-safe, DontDestroyOnLoad
- [x] **Bootstrap Scene** – Khởi tạo tuần tự tất cả Manager
- [x] **Namespace Organization** – 7 namespace dưới `InnerDuel.*`
- [x] **Git Setup** – `.gitignore`, `.gitattributes`, Git LFS cho binary files
- [x] **Project Structure** – `Assets/_Project` tách biệt, folder rõ ràng

### Hệ thống Core
- [x] **GameManager** – State machine: Intro → Gameplay → Ending
- [x] **InputManager** – Unity Input System, hỗ trợ 2P local
- [x] **AudioManager** – BGM, SFX, volume control, fade in/out
- [x] **CameraController** – Cinemachine, auto-zoom theo 2 player, ending sequence

### Nhân vật & Combat
- [x] **CharacterType Enum** – Đầy đủ 8 loại nhân vật
- [x] **CharacterData** – Data-driven stats (HP, speed, damage, defense, ability flags)
- [x] **CharacterFactory** – Factory pattern, spawn + fallback
- [x] **InnerCharacterController** – Movement, Attack, Block, Dash, Die, BerserkMode
- [x] **Hitbox/Hurtbox** – OverlapCircleAll-based combat
- [x] **Discipline Prefab** – Prefab hoàn chỉnh cho The Warden
- [x] **Spontaneity Prefab** – Prefab cho The Maverick

### UI & Effects
- [x] **UIManager** – Intro text, Gameplay panel, Ending panel
- [x] **HealthBar** – Thanh máu + hiệu ứng "delayed health"
- [x] **ParticleEffectsManager** – Hit (8 màu), Block, Dash, Harmony effects
- [x] **Typewriter Effect** – Hiệu ứng gõ chữ cho intro quotes

### Tài liệu
- [x] **README.md** – Quick start, cấu trúc project, điều khiển
- [x] **ARCHITECTURE.md** – Kiến trúc chi tiết, diagrams, boot flow
- [x] **CONTRIBUTING.md** – Git workflow, branching, PR checklist
- [x] **CODING_CONVENTIONS.md** – Chuẩn code, naming, patterns
- [x] **FIX_GUIDE.md** – Hướng dẫn fix lỗi sau clone

---

## CẦN PHÁT TRIỂN 🔨

### Giai đoạn 1: Hoàn thiện Nhân vật (Ưu tiên cao)

| Nhân vật | Loại | Kỹ năng đặc biệt | Trạng thái |
|:---|:---|:---|:---|
| The Warden (Kỷ Luật) | Discipline | Parry + Phản công | ⚠️ Cần polish moveset |
| The Maverick (Ngẫu Hứng) | Spontaneity | Dash đa hướng + Combo tốc độ | ⚠️ Cần polish moveset |
| The Architect (Lý Trí) | Logic | Đặt bẫy + Khống chế vùng | ❌ Chưa có prefab |
| The Muse (Sáng Tạo) | Creativity | Tầm đánh ngẫu nhiên | ❌ Chưa có prefab |
| The Unbroken (Kiên Trì) | Persistence | Tăng damage khi HP giảm | ❌ Chưa có prefab |
| The Void (Từ Bỏ) | Surrender | Life-steal | ❌ Chưa có prefab |
| The Zen (Tĩnh Lặng) | Stillness | Counter-attack | ❌ Chưa có prefab |
| The Berserker (Thịnh Nộ) | Rage | Berserk mode (visual + gameplay) | ❌ Chưa có prefab |

**Cách bắt đầu:**
1. Duplicate `Discipline_Character.prefab` → đổi tên
2. Sửa `CharacterData.type` → loại nhân vật mới
3. Implement logic đặc biệt trong `InnerCharacterController.HandleAbilities()`
4. Xem thêm: [ARCHITECTURE.md – Quy tắc mở rộng](ARCHITECTURE.md#quy-tắc-mở-rộng)

### Giai đoạn 2: UI/UX

- [ ] **Main Menu** – Title screen, Settings, Play, Exit
- [ ] **Character Selection** – Chọn nhân vật cho Player 1 & 2 trước trận
- [ ] **Pause Menu** – Tạm dừng, Resume, Restart, Quit
- [ ] **Victory/Defeat Screen** – Kết quả trận đấu, Rematch button
- [ ] **Settings Screen** – Volume, Controls

### Giai đoạn 3: Art & Animation

- [ ] **Character Sprites** – Sprite chính thức cho 8 nhân vật (thay placeholder)
- [ ] **Idle/Walk/Attack Animations** – Animation set cho từng nhân vật
- [ ] **Background Art** – Arena/Background cho trận đấu
- [ ] **UI Art** – Button, panel, health bar skin

### Giai đoạn 4: Audio

- [ ] **Unique SFX** – Âm thanh riêng cho từng nhân vật
- [ ] **Dynamic BGM** – Nhạc nền thay đổi nhịp theo HP
- [ ] **Voice Lines** – Tiếng nói cho events đặc biệt
- [ ] **Menu Music** – Nhạc nền cho menu screens

### Giai đoạn 5: Polish & QA

- [ ] **Balance Testing** – Cân bằng stats giữa các nhân vật
- [ ] **Collision Tuning** – Tinh chỉnh hitbox/hurtbox
- [ ] **Performance Optimization** – Tối ưu particle, pooling
- [ ] **Ending Sequence** – Harmony animation hoàn chỉnh
- [ ] **Bug Fixing** – Tổng hợp & fix lỗi

---

## Phân công gợi ý

| Thành viên | Phạm vi | Files liên quan |
|:---|:---|:---|
| Dev 1 | Nhân vật 3-4 | `InnerCharacterController.cs`, `CharacterFactory.cs` |
| Dev 2 | Nhân vật 5-6 | `InnerCharacterController.cs`, `CharacterFactory.cs` |
| Dev 3 | Nhân vật 7-8 | `InnerCharacterController.cs`, `CharacterFactory.cs` |
| Dev 4 | UI/UX | `UIManager.cs`, thêm scenes mới |
| Dev 5 | Art & Animation | `_Project/Art/`, Prefab sprites |
| Dev 6 | Audio & SFX | `AudioManager.cs`, `_Project/Audio/` |

> [!TIP]
> Mỗi người nên tạo **branch riêng** theo tên tính năng. Xem chi tiết tại [CONTRIBUTING.md](CONTRIBUTING.md).
