# Chuẩn Viết Code – Inner Duel

Tài liệu quy ước coding cho team, đảm bảo code thống nhất và dễ bảo trì.

---

## Namespace

Mọi script **phải** nằm trong namespace theo module:

```csharp
// ✅ Đúng
namespace InnerDuel.Characters
{
    public class MyNewScript : MonoBehaviour { }
}

// ❌ Sai – không có namespace
public class MyNewScript : MonoBehaviour { }
```

| Module | Namespace | Thư mục |
|:---|:---|:---|
| Core | `InnerDuel.Core` | `Scripts/Core/` |
| Game | `InnerDuel` | `Scripts/Game/` |
| Characters | `InnerDuel.Characters` | `Scripts/Character/` |
| Input | `InnerDuel.Input` | `Scripts/Core/` |
| Audio | `InnerDuel.Audio` | `Scripts/Core/` |
| Camera | `InnerDuel.Camera` | `Scripts/Camera/` |
| UI | `InnerDuel.UI` | `Scripts/UI/` |
| Effects | `InnerDuel.Effects` | `Scripts/Effects/` |

---

## Đặt tên

### Classes & Methods
```csharp
// PascalCase cho class, method, property
public class CharacterFactory { }
public void CreateCharacter() { }
public float MaxHealth { get; set; }
```

### Variables
```csharp
// camelCase cho local variables và parameters
float moveSpeed = 5f;
int playerID = 1;

// _camelCase cho private fields
private float _currentHealth;
private bool _isAttacking;

// Hoặc không có underscore (theo style hiện tại của project)
private float currentHealth;
private bool isAttacking;
```

### Constants & Enums
```csharp
// PascalCase cho enum values
public enum CharacterType
{
    Discipline,
    Spontaneity,
    Logic
}
```

### Prefabs & Assets
```
Tên_Mô_tả.prefab    → Discipline_Character.prefab
Tên_Mô_tả.cs        → InnerCharacterController.cs
```

---

## Cấu trúc Script

Sắp xếp sections trong một script MonoBehaviour theo thứ tự:

```csharp
using UnityEngine;

namespace InnerDuel.Characters
{
    /// <summary>
    /// Mô tả ngắn gọn class làm gì.
    /// </summary>
    public class ExampleScript : MonoBehaviour
    {
        // 1. Constants
        private const float MAX_SPEED = 10f;
        
        // 2. Serialized Fields (hiện trong Inspector)
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        // 3. Public Fields / Properties
        public float CurrentHealth { get; private set; }
        
        // 4. Private Fields
        private Rigidbody2D rb;
        private bool isGrounded;
        
        // 5. Unity Lifecycle (theo thứ tự gọi)
        private void Awake() { }
        private void Start() { }
        private void Update() { }
        private void FixedUpdate() { }
        private void OnDestroy() { }
        
        // 6. Public Methods
        public void TakeDamage(float amount) { }
        
        // 7. Private Methods
        private void HandleMovement() { }
        
        // 8. Coroutines
        private System.Collections.IEnumerator FadeOut() { yield break; }
    }
}
```

---

## Inspector & Serialization

```csharp
// Dùng [Header] để nhóm fields trong Inspector
[Header("Movement")]
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private float jumpHeight = 3f;

[Header("Combat")]
[SerializeField] private float attackDamage = 10f;
[SerializeField] private Transform attackPoint;

// Dùng [Tooltip] cho field phức tạp
[Tooltip("Thời gian cooldown giữa 2 lần attack (giây)")]
[SerializeField] private float attackCooldown = 0.5f;
```

---

## Null Safety

```csharp
// ✅ Luôn kiểm tra null trước khi dùng reference
if (healthBar != null)
{
    healthBar.SetHealth(currentHealth);
}

// ✅ Dùng ?. cho optional calls
audioManager?.PlayHitSound();

// ❌ Không dùng trực tiếp mà không kiểm tra
healthBar.SetHealth(currentHealth); // Có thể crash
```

---

## Debug Logging

```csharp
// Dùng tag để dễ lọc trong Console
Debug.Log("[GameManager] Game started");
Debug.LogWarning("[CharacterFactory] Prefab not found, using fallback");
Debug.LogError("[InputManager] Failed to initialize input system");

// Format: [ClassName] Message
```

> [!IMPORTANT]
> Trước khi merge vào `main`, hãy **loại bỏ** hoặc **comment out** các `Debug.Log` dùng để test. Chỉ giữ lại log có ý nghĩa cho debug.

---

## Singleton Usage

```csharp
// ✅ Tạo Singleton manager mới
using InnerDuel.Core;

namespace InnerDuel.MyModule
{
    public class MyManager : Singleton<MyManager>
    {
        protected override void Awake()
        {
            base.Awake(); // QUAN TRỌNG: phải gọi base.Awake()
            // Custom initialization
        }
    }
}

// ✅ Sử dụng Singleton
var gm = GameManager.Instance;
AudioManager.Instance.PlayBGM();
```

---

## Tóm tắt nhanh

| Quy tắc | Ví dụ |
|:---|:---|
| Namespace bắt buộc | `InnerDuel.Characters` |
| Class: PascalCase | `CharacterFactory` |
| Method: PascalCase | `CreateCharacter()` |
| Private field: camelCase | `currentHealth` |
| SerializeField dùng `[Header]` | `[Header("Combat")]` |
| Kiểm tra null trước dùng | `if (obj != null)` |
| Debug log có tag | `[ClassName] Message` |
| Singleton gọi `base.Awake()` | Luôn luôn |
