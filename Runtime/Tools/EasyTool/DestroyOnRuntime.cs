using UnityEngine;

/// <summary>
/// 挂在仅编辑期使用的对象上（如 Timeline 制作相机），进入运行时（Play / 正式包）立即销毁，避免干扰正式逻辑。
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-10000)]
public class DestroyOnRuntime : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}
