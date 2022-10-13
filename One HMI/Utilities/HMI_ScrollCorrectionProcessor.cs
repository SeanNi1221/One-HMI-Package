#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif

public class HMI_ScrollCorrectionProcessor : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static HMI_ScrollCorrectionProcessor() {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize() {
        InputSystem.RegisterProcessor<HMI_ScrollCorrectionProcessor>();
    }

    [Tooltip("ScrollValue replacement for incoming Scroll Vector.")]
    public float scrollValue = 120f;

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        if (value.y > 0){
            value.y = scrollValue;
        }
        if (value.y < 0)
        {
            value.y = -scrollValue;
        }
        return value;
    }
}
#endif

