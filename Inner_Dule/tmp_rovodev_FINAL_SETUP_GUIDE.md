# ✅ HƯỚNG DẪN SETUP HOÀN CHỈNH - THUONG & PHAPSU

## 🎮 KEY MAPPING CUỐI CÙNG

### 🔵 PLAYER 1 (THUONG)
| Phím | Chức năng | Animation Parameter |
|------|-----------|---------------------|
| A/D | Di chuyển | Speed (Float) |
| W | Nhảy | IsJumping (Bool) |
| S | Phòng thủ (hold) | IsDefece (Bool) |
| H | Attack cơ bản | Attack (Trigger) |
| J | Skill 1 | Skill1 (Trigger) |
| K | Skill 2 | Skill2 (Trigger) |
| L | Skill 3 + Leap | Skill3 (Trigger) |

### 🔴 PLAYER 2 (PHAPSU)
| Phím | Chức năng | Animation Parameter |
|------|-----------|---------------------|
| ← → | Di chuyển | Speed (Float) |
| ↑ | Nhảy | IsJumping (Bool) |
| ↓ | Phòng thủ (hold) | IsDefence (Bool) |
| 1 | Attack + Fireball | Attack (Trigger) |
| 2 | Skill 1 | Skill1 (Trigger) |
| 3 | Skill 2 | Skill2 (Trigger) |
| 4 | Skill 3 + Leap + Fireballs | Skill3 (Trigger) |

---

## ✅ PARAMETERS HIỆN CÓ (100% KHỚP!)

### THUONG Controller:
- ✅ **Speed** (Float) - Vận tốc di chuyển
- ✅ **IsJumping** (Bool) - Đang nhảy
- ✅ **IsDefece** (Bool) - Đang phòng thủ
- ✅ **IsDeath** (Bool) - Đã chết
- ✅ **Attack** (Trigger) - Attack cơ bản
- ✅ **Skill1** (Trigger) - Skill 1
- ✅ **Skill2** (Trigger) - Skill 2
- ✅ **Skill3** (Trigger) - Skill 3
- ✅ **Hit** (Trigger) - Bị đánh
- ⚠️ **New Bool** - Không cần nữa, có thể xóa

### PHAPSU Controller:
- ✅ **Speed** (Float) - Vận tốc di chuyển
- ✅ **IsJumping** (Bool) - Đang nhảy
- ✅ **IsDefence** (Bool) - Đang phòng thủ
- ✅ **IsDeath** (Bool) - Đã chết
- ✅ **IsAttacking** (Bool) - State flag
- ✅ **Attack** (Trigger) - Attack cơ bản
- ✅ **Skill1** (Trigger) - Skill 1
- ✅ **Skill2** (Trigger) - Skill 2
- ✅ **Skill3** (Trigger) - Skill 3
- ✅ **Hit** (Trigger) - Bị đánh

---

## 🎯 CÁCH SPEED HOẠT ĐỘNG

Script tự động cập nhật parameter **Speed** mỗi frame:
```
Speed = Mathf.Abs(rb.velocity.x)
```

**Giá trị:**
- Đứng yên: `Speed = 0`
- Di chuyển: `Speed ≈ 6` (tùy moveSpeed setting)
- Trong không khí: `Speed` vẫn được cập nhật

**Dùng trong Animator:**
- Idle ↔ Run transitions dựa vào giá trị Speed
- Có thể dùng Blend Tree 1D để blend mượt mà giữa Idle và Run

---

## 🔧 SETUP TRANSITIONS TRONG ANIMATOR

### Phương pháp 1: Transitions đơn giản

**Idle → Run:**
- Condition: `Speed > 0.1`
- Has Exit Time: `false`
- Transition Duration: `0.1s`

**Run → Idle:**
- Condition: `Speed < 0.1`
- Has Exit Time: `false`
- Transition Duration: `0.1s`

### Phương pháp 2: Blend Tree 1D (Khuyến nghị)

1. Tạo Blend Tree 1D trong Base Layer
2. Parameter: `Speed`
3. Add Motion:
   - Threshold `0`: Idle.anim
   - Threshold `6`: Run.anim
4. Script sẽ tự động blend giữa Idle và Run dựa vào Speed

---

## 🎮 TRANSITIONS CHO CÁC STATES KHÁC

### Jump:
- **Any State → Jump:** `IsJumping == true`
- **Jump → Idle/Run:** `IsJumping == false`
- Has Exit Time: `false`

### Defence:
- **Any State → Defence:** `IsDefece == true` (hoặc `IsDefence == true`)
- **Defence → Idle/Run:** `IsDefece == false`
- Has Exit Time: `false`
- **Lưu ý:** Defence phải **GIỮ PHÍM**, script tự set false khi nhả

### Attacks/Skills:
- **Any State → Attack:** `Attack (trigger)`
- **Any State → Skill1:** `Skill1 (trigger)`
- **Any State → Skill2:** `Skill2 (trigger)`
- **Any State → Skill3:** `Skill3 (trigger)`
- **Attack/Skill → Idle:** Has Exit Time = `true`, Exit Time = `0.9`

### Hit:
- **Any State → Hit:** `Hit (trigger)`
- **Hit → Idle:** Has Exit Time = `true`, Exit Time = `0.9`

### Death:
- **Any State → Death:** `IsDeath == true`
- **Không có transition ra khỏi Death**
- Has Exit Time: `false`

---

## 📊 COOLDOWNS

| Attack/Skill | Cooldown |
|--------------|----------|
| Attack (H/1) | 1 giây |
| Skill1 (J/2) | 1 giây |
| Skill2 (K/3) | 5 giây |
| Skill3 (L/4) | 8 giây |

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **Defence (S/↓):** Phải **GIỮ PHÍM**, không phải nhấn 1 lần
2. **Skill3:** Có **leap effect** - nhảy lên + lao về phía trước
3. **Player 2's Attack (1):** Spawn 1 fireball
4. **Player 2's Skill3 (4):** Spawn 5 fireballs liên tiếp + leap
5. **Speed parameter:** Tự động cập nhật, không cần set thủ công
6. **Hit animation:** THUONG dùng `Hit` (uppercase), PHAPSU dùng `Hit` (uppercase) - cả 2 giống nhau

---

## 🎯 CHECKLIST CUỐI CÙNG

### Trong Unity Animator:

#### THUONG:
- [ ] Xóa parameter "New Bool" (không cần)
- [ ] Setup Idle ↔ Run dựa vào `Speed`
- [ ] Setup Jump transitions với `IsJumping`
- [ ] Setup Defence transitions với `IsDefece`
- [ ] Setup Attack/Skill transitions với triggers
- [ ] Setup Hit transition
- [ ] Setup Death transition với `IsDeath`

#### PHAPSU:
- [ ] Setup Idle ↔ Run dựa vào `Speed`
- [ ] Setup Jump transitions với `IsJumping`
- [ ] Setup Defence transitions với `IsDefence`
- [ ] Setup Attack/Skill transitions với triggers
- [ ] Setup Hit transition
- [ ] Setup Death transition với `IsDeath`

### Test trong game:
- [ ] A/D (P1) hoặc ←→ (P2) → Run animation
- [ ] Dừng lại → Idle animation
- [ ] W (P1) hoặc ↑ (P2) → Jump animation
- [ ] S (P1) hoặc ↓ (P2) → Defence animation (giữ phím)
- [ ] H/J/K/L (P1) hoặc 1/2/3/4 (P2) → Attack/Skill animations
- [ ] Bị đánh → Hit animation
- [ ] HP = 0 → Death animation

---

## ✅ KẾT LUẬN

**HOÀN TOÀN KHỚP 100%!**

- ✅ Script đã được cập nhật dùng `Speed` (Float)
- ✅ Cả 2 controllers đã có sẵn parameter `Speed`
- ✅ Tất cả parameters cần thiết đã có đầy đủ
- ✅ Key mapping đã được cập nhật
- ✅ Defence và Attack/Skills đã được thêm vào

**Chỉ cần setup transitions trong Animator và test thôi!**

---

**Ngày cập nhật:** 2026-03-14  
**Script version:** PlayerMovement2D.cs (updated to use Speed Float)  
**Status:** ✅ SẴN SÀNG SỬ DỤNG
