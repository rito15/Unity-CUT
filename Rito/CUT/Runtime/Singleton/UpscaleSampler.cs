using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.CUT
{
    [DisallowMultipleComponent]
    public class UpscaleSampler : MonoBehaviour
    {
        #region Singleton 

        public static UpscaleSampler Instance
        {
            get
            {
                if (instance == null) CheckExsistence();
                return instance;
            }
        }
        public static UpscaleSampler I => Instance;
        private static UpscaleSampler instance;

        private static void CheckExsistence()
        {
            instance = FindObjectOfType<UpscaleSampler>();
            if (instance == null)
            {
                GameObject container = new GameObject("Upscale Sampler");
                instance = container.AddComponent<UpscaleSampler>();
            }
        }

        /// <summary> 
        /// [Awake()에서 호출]
        /// <para/> 싱글톤 스크립트를 미리 오브젝트에 담아 사용하는 경우를 위한 로직
        /// </summary>
        private void CheckInstance()
        {
            if (instance == null) instance = this;
            else if (instance != this)
            {
                Debug.Log("이미 UpscaleSampler 싱글톤이 존재하므로 오브젝트를 파괴합니다.");
                Destroy(this);
                var components = gameObject.GetComponents<Component>();
                if (components.Length <= 2) Destroy(gameObject);
            }
        }

        private void Awake()
        {
            CheckInstance();
        }

        #endregion // ==================================================================

        #region Upscale Sampler

        [Header("Options")]
        [SerializeField]
        [Tooltip("게임 시작 시 동작 여부")]
        private bool _runOnStart = true;

        [SerializeField, Range(0.1f, 1.0f)]
        [Tooltip("게임 시작 시 설정할 비율")]
        private float _targetRatio = 1.0f;

        [SerializeField]
        [Tooltip("UI만 제외하고 렌더링할 카메라")]
        private Camera _targetCamera;

        [SerializeField]
        [Tooltip("UI만 렌더링할 카메라")]
        private Camera _uiCamera;

        [SerializeField]
        [Tooltip("_targetCamera가 설정되지 않은 경우, 자동으로 현재 렌더링 카메라를 탐지할지 여부")]
        private bool _autoDetectMainCamera = true;

        [SerializeField]
        [Tooltip("메인 렌더링 카메라가 달라질 경우, 자동 탐지하여 적용")]
        private bool _autoDetectCameraChange = true;


        [Header("Target UI")]
        [SerializeField]
        [Tooltip("RawImage를 세팅할 대상 캔버스")]
        private Canvas _targetCanvas;

        [Header("Editor Options")]
        [SerializeField]
        [Tooltip("디버그 로그 출력 허용")]
        private bool _allowDebug = true;

        [SerializeField]
        [Tooltip("하이어라키에서 숨기기")]
        private bool _hideFromHiearchy = false;

        // Fields
        private int _currentWidth;
        private int _currentHeight;
        private float _currentRatio;
        private bool _initialized = false; // 한 번이라도 실행됐는지 여부
        private RenderTexture _currentRT;
        private UnityEngine.UI.RawImage _rawImage;

        [SerializeField, HideInInspector]
        private Shader _rawImageShader;

        private void Log(string msg)
        {
            if (!_allowDebug) return;
            Debug.Log($"[Upscale Sampler] {msg}", gameObject);
        }

        private void Reset()
        {
            _rawImageShader = Shader.Find("Unlit/Texture");
        }

        private void Start()
        {
            if (_runOnStart)
            {
                Run(_targetRatio);
            }
        }
        private void OnEnable()
        {
            StopCoroutine(nameof(DetectCameraChangeRoutine));
            StartCoroutine(nameof(DetectCameraChangeRoutine));
        }

        private IEnumerator DetectCameraChangeRoutine()
        {
            while (true)
            {
                if ((_initialized && _autoDetectCameraChange) && 
                    (_targetCamera == null || _targetCamera.enabled == false || _targetCamera.gameObject.activeInHierarchy == false))
                {
                    _targetCamera = null;
                    bool flag = Run(_currentRatio, forceRun: true);
                    if (flag)
                    {
                        Log("Camera Change Auto Detected");
                    }
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private void OnDestroy()
        {
            ReleaseRT();
        }

        // forceRun : 이전 상태 관계 없이 강제 실행
        public bool Run(float ratio, bool forceRun = false)
        {
            if (ratio < 0.1f) ratio = 0.1f;
            if (ratio > 1.0f) ratio = 1.0f;

            int sourceW = Screen.width;
            int sourceH = Screen.height;
#if UNITY_EDITOR
            (sourceW, sourceH) = GetMainGameViewSize();
#endif
            int w = (int)(sourceW * ratio);
            int h = (int)(sourceH * ratio);

            if (!forceRun && _currentWidth == w && _currentHeight == h)
            {
                Log($"기존과 동일합니다. - {w}x{h} ({ratio})");
                return false;
            }

            ReleaseRT();
            if (!CreateRT(w, h)) return false;
            SetCamera();
            SetRawImage();
            HideFromHierarchy();

            _currentWidth  = w;
            _currentHeight = h;
            _currentRatio  = ratio;
            Log($"Screen: {sourceW}x{sourceH} / Sampled: {w}x{h} ({ratio * 100:F2}%)");

            _initialized = true;
            return true;
        }

        private bool CreateRT(int w, int h)
        {
            _currentRT = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
            _currentRT.Create();

            if (_autoDetectMainCamera)
            {
                if (_targetCamera == null) _targetCamera = Camera.main;
                NoUiCam();
                if (_targetCamera == null) _targetCamera = Camera.current;
                NoUiCam();
                if (_targetCamera == null) _targetCamera = FindObjectOfType<Camera>();
                NoUiCam();
            }
            if (_targetCamera == null)
            {
                Log("타겟 카메라를 찾을 수 없습니다.");
                return false;
            }

            return true;

            // --
            void NoUiCam()
            {
                if (_targetCamera != null && _targetCamera == _uiCamera)
                    _targetCamera = null;
            }
        }

        /// <summary> 타겟 카메라, UI 카메라 설정 </summary>
        private void SetCamera()
        {
            int uiLayerMask = 1 << LayerMask.NameToLayer("UI");

            _targetCamera.targetTexture = _currentRT;
            _targetCamera.cullingMask &= ~uiLayerMask; // UI 레이어만 제거

            if (_uiCamera == null)
            {
                GameObject uiCamGo = new GameObject("UI Only Camera");
                _uiCamera = uiCamGo.AddComponent<Camera>();
                _uiCamera.targetDisplay = _targetCamera.targetDisplay;
                _uiCamera.clearFlags = CameraClearFlags.Nothing;
                _uiCamera.cullingMask = uiLayerMask;
            }
        }

        /// <summary> 렌더 타겟을 RawImage에 세팅 </summary>
        private void SetRawImage()
        {
            if (_targetCanvas == null)
            {
                GameObject canvasGo = new GameObject("Upscale Sample Target Canvas");
                _targetCanvas = canvasGo.AddComponent<Canvas>();
                _targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _targetCanvas.sortingOrder = -10000;
            }
            if (_rawImage == null)
            {
                GameObject rawImageGo = new GameObject("Upscale Sample Target RawImage");
                rawImageGo.transform.SetParent(_targetCanvas.transform);

                _rawImage = rawImageGo.AddComponent<UnityEngine.UI.RawImage>();
                _rawImage.raycastTarget = false;
                _rawImage.maskable = false;

                // 기본 마테리얼 할당
                _rawImage.material = new Material(_rawImageShader);

                RectTransform rect = _rawImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
#if UNITY_EDITOR
                ToggleSceneVisibility(_targetCanvas.gameObject);
#endif
            }
            _rawImage.texture = _currentRT;
        }

        private void ReleaseRT()
        {
            if (_currentRT != null)
            {
                _currentRT.Release();
            }
        }

        private void HideFromHierarchy()
        {
            if (_hideFromHiearchy == false) return;
            gameObject.hideFlags = 
            _targetCanvas.gameObject.hideFlags = 
            _uiCamera.gameObject.hideFlags = HideFlags.HideInHierarchy;
            Log("하이어라키에서 숨김처리 되었습니다.");
        }

#endregion

#region Editor Only
#if UNITY_EDITOR
        private static System.Reflection.MethodInfo GetSizeOfMainGameViewMi;

        // 커스텀 에디터에서 Screen.width, height를 참조하면 게임 뷰의 해상도를 가져오지 못하므로 에디터 스크립트 활용
        private static (int x, int y) GetMainGameViewSize()
        {
            if (GetSizeOfMainGameViewMi == null)
            {
                System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                GetSizeOfMainGameViewMi = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            }
            System.Object res = GetSizeOfMainGameViewMi.Invoke(null, null);
            Vector2 resVec = (Vector2)res;
            return ((int)resVec.x, (int)resVec.y);
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void ToggleSceneVisibility(GameObject target)
        {
            UnityEditor.SceneVisibilityManager.instance.DisablePicking(target, true);
            UnityEditor.SceneVisibilityManager.instance.Hide(target, true);
        }
#endif

#endregion

#region Custom Editor
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(UpscaleSampler))]
        private class CE : UnityEditor.Editor
        {
            private UpscaleSampler t;

            private void OnEnable()
            {
                if (t == null) t = target as UpscaleSampler;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                using (new UnityEditor.EditorGUI.DisabledGroupScope(true))
                {
                    t._rawImageShader = (Shader)UnityEditor.EditorGUILayout.ObjectField("Raw Image Shader", t._rawImageShader, typeof(Shader), allowSceneObjects: false);
                    if (t._rawImageShader == null)
                    {
                        t._rawImageShader = Shader.Find("Unlit/Texture");
                    }
                }
                UnityEditor.EditorGUILayout.Space(8f);

                if (Application.isPlaying == false) return;
                if (GUILayout.Button("Apply Now"))
                {
                    t.Run(t._targetRatio);
                }
                using (new UnityEditor.EditorGUILayout.HorizontalScope())
                {
                    DrawApplyButton(0.25f);
                    DrawApplyButton(0.50f);
                    DrawApplyButton(0.75f);
                    DrawApplyButton(1.00f);
                }
                using (new UnityEditor.EditorGUILayout.HorizontalScope())
                {
                    DrawApplyButton(0.2f);
                    DrawApplyButton(0.4f);
                    DrawApplyButton(0.6f);
                    DrawApplyButton(0.8f);
                    DrawApplyButton(1.0f);
                }
                using (new UnityEditor.EditorGUILayout.HorizontalScope())
                {
                    for(float f = 0.1f; f < 1.01f; f += 0.1f)
                        DrawApplyButton2(f);
                }
            }

            private void DrawApplyButton(float ratio)
            {
                if (GUILayout.Button($"{ratio:F2}"))
                {
                    t.Run(ratio);
                }
            }

            private void DrawApplyButton2(float ratio)
            {
                if (GUILayout.Button($"{ratio:F1}"))
                {
                    t.Run(ratio);
                }
            }
        }
#endif
#endregion
    }
}