# Unity CUT
- Unity Custom UTilities
- `2023.3.21f1 (LTS)`
- UPM
```
https://github.com/rito15/Unity-CUT.git
```

<br>

# List
- Upscale Sampler
- Fly Camera

<br>

# 참고사항
## Upscale Sampler + Lean Integration

<details>
<summary markdown="span"> 
...
</summary>

### 어셈블리 정의 파일 수정
- 대상
  - `CW/LeanTouch/LeanTouch.asmdef`
  - `CW/Shared/Common/CW.Common.asmdef`

- 참조 추가: `Rito.CUT`

### CW/LeanTouch/Required/Scripts/LeanTouch.cs
```cs
        /// <summary>This will return all the RaycastResults under the specified screen point using the specified layerMask.
        /// NOTE: The first result (0) will be the top UI element that was first hit.</summary>
        public static List<RaycastResult> RaycastGui(Vector2 screenPosition, LayerMask layerMask)
        {
            if (Rito.CUT.UpscaleSampler.IsCreated())                    // ** 추가 **
                screenPosition /= Rito.CUT.UpscaleSampler.CurrentRatio; // ** 추가 **
            tempRaycastResults.Clear();

            var currentEventSystem = GetEventSystem();
```

### CW/Shared/Common/Required/Scripts/CWInput.cs
```cs
        public static void GetTouch(int index, out int id, out UnityEngine.Vector2 position, out float pressure, out bool set)
        {
#if USE_NEW_INPUT_SYSTEM
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[index];

            id       = touch.finger.index;
            position = touch.screenPosition;
            pressure = touch.pressure;
            set      =
                touch.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Moved;
#else
            var touch = UnityEngine.Input.GetTouch(index);

            id       = touch.fingerId;
            position = touch.position;
            pressure = touch.pressure;
            set      =
                touch.phase == UnityEngine.TouchPhase.Began ||
                touch.phase == UnityEngine.TouchPhase.Stationary ||
                touch.phase == UnityEngine.TouchPhase.Moved;
#endif
            if (Rito.CUT.UpscaleSampler.IsCreated())              // ** 추가 **
                position *= Rito.CUT.UpscaleSampler.CurrentRatio; // ** 추가 **
        }

        public static UnityEngine.Vector2 GetMousePosition()
        {
            float upsample = Rito.CUT.UpscaleSampler.IsCreated() ? Rito.CUT.UpscaleSampler.CurrentRatio : 1f; // ** 추가 **
#if USE_NEW_INPUT_SYSTEM
            return 
                upsample * // ** 추가 **
                (UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : default(UnityEngine.Vector2));
#else
            return upsample * UnityEngine.Input.mousePosition; // ** 변경 **
#endif
        }
```

</details>
