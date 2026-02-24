# Quy trình Làm việc Team – Inner Duel

Tài liệu hướng dẫn quy trình phát triển cho team.

---

## Git Workflow

### Branch Strategy

```
main                    ← Bản ổn định, luôn chạy được
├── feature/ten-tinh-nang    ← Phát triển tính năng mới
├── fix/ten-loi              ← Sửa bug
└── art/ten-asset            ← Thêm art/audio assets
```

### Quy tắc đặt tên Branch

| Loại | Format | Ví dụ |
|:---|:---|:---|
| Tính năng mới | `feature/ten-tinh-nang` | `feature/berserker-mode` |
| Sửa bug | `fix/ten-loi` | `fix/healthbar-null-ref` |
| Art/Audio | `art/ten-asset` | `art/warden-animations` |
| UI | `ui/ten-man-hinh` | `ui/character-selection` |

### Quy trình thao tác

```bash
# 1. Luôn pull main mới nhất trước
git checkout main
git pull origin main

# 2. Tạo branch mới từ main
git checkout -b feature/ten-tinh-nang

# 3. Làm việc, commit thường xuyên
git add .
git commit -m "feat: mô tả ngắn gọn"

# 4. Push lên remote
git push origin feature/ten-tinh-nang

# 5. Tạo Pull Request trên GitHub → chờ review → merge vào main
```

### Commit Message Format

```
<type>: <mô tả ngắn gọn>

Ví dụ:
feat: thêm berserker mode cho Rage character
fix: sửa lỗi null reference khi attack
art: thêm animation idle cho Warden
ui: tạo màn hình chọn nhân vật
refactor: tách logic combat ra file riêng
docs: cập nhật architecture document
```

| Type | Khi nào dùng |
|:---|:---|
| `feat` | Thêm tính năng mới |
| `fix` | Sửa bug |
| `art` | Thêm/sửa art, animation, audio |
| `ui` | Thêm/sửa giao diện |
| `refactor` | Tái cấu trúc code |
| `docs` | Cập nhật tài liệu |

---

## Quy trình Phát triển trong Unity

### Trước khi bắt đầu code

1. Đọc [ARCHITECTURE.md](ARCHITECTURE.md) để hiểu hệ thống
2. Đọc [CODING_CONVENTIONS.md](CODING_CONVENTIONS.md) để biết chuẩn code
3. Xác định module bạn sẽ làm → đọc code file liên quan

### Thêm Script mới

1. Đặt vào **đúng thư mục** trong `Assets/_Project/Scripts/<Module>/`
2. Dùng **đúng namespace**: `InnerDuel.<Module>`
3. Nếu là Manager dùng chung → kế thừa `Singleton<T>`
4. Thêm XML documentation `///` cho public methods

### Test trong Unity

1. Nhấn **Play** → kiểm tra Console
2. Nếu có lỗi đỏ → nhấp đúp vào lỗi để xem file gây lỗi
3. Nếu có warning vàng → xem `FIX_GUIDE.md` để biết cách fix

---

## Phân công & Phối hợp

### Tránh Merge Conflict

> [!WARNING]
> **Unity Scene files** rất dễ bị conflict. Quy tắc:
> - **Chỉ 1 người** sửa 1 scene tại cùng một thời điểm
> - Tránh sửa trực tiếp `MainGameScene.unity` – dùng Prefab thay thế
> - Khi sửa Prefab → chỉ sửa Prefab riêng, không sửa instance trong scene

### Asset Workflow

| Loại file | Thư mục | LFS |
|:---|:---|:---|
| Sprites/Textures | `_Project/Art/` | ✅ (.png, .psd, .jpg) |
| Animations | `_Project/Art/Animations/` | ❌ (.anim, .controller) |
| Audio | `_Project/Audio/` | ✅ (.mp3, .wav, .ogg) |
| Prefabs | `_Project/Prefabs/` | ❌ (.prefab) |

---

## Checklist trước khi tạo Pull Request

- [ ] Code biên dịch thành công (không có lỗi đỏ trong Console)
- [ ] Nhấn Play chạy được, không crash
- [ ] Không sửa file scene ngoài phạm vi task
- [ ] Commit message đúng format
- [ ] Đã test cả Player 1 và Player 2
- [ ] Không để code debug/test (Debug.Log thừa, hardcoded values)
