using UnityEngine;

namespace Rito.CUT
{ 
    public class FlyCamera : MonoBehaviour
    {
        [Header("Values")]
        [Range(0, 100)] [SerializeField] private float rotateSpeed    = 20f;
        [Range(0, 100)] [SerializeField] private float moveSpeed      = 10f;
        [Range(0, 100)] [SerializeField] private float upDownSpeed    = 5f;
        [Range(0,   1)] [SerializeField] private float slowMoveFactor = 0.25f;
        [Range(1,  10)] [SerializeField] private float fastMoveFactor = 3f;

        [Header("Options")]
        [SerializeField] private bool rotateOnLeftClick = true;  // 좌클릭 상태에서만 회전
        [SerializeField] private bool moveOnLeftClick   = false; // 좌클릭 상태에서만 이동
        [SerializeField] private bool hideCursorOnLeftClick = false;

        [Header("Keys")]
        [SerializeField] private KeyCode upKey     = KeyCode.E;
        [SerializeField] private KeyCode downKey   = KeyCode.Q;
        [SerializeField] private KeyCode accelKey1 = KeyCode.LeftShift;
        [SerializeField] private KeyCode accelKey2 = KeyCode.RightShift;
        [SerializeField] private KeyCode decelKey1 = KeyCode.LeftControl;
        [SerializeField] private KeyCode decelKey2 = KeyCode.RightControl;
     
        private float rotX = 0.0f;
        private float rotY = 0.0f;

        private bool isLeftClicked;
        private float dt;
        private Transform tr;

        private void OnEnable()
        {
            tr   = transform;
            rotX = tr.eulerAngles.y;
        }

        private void Update()
        {
            dt = Time.deltaTime;
            isLeftClicked = Input.GetMouseButton(0);
            HideCursor();
            Rotate();
            Move();
        }

        private void HideCursor()
        {
            bool flag = hideCursorOnLeftClick && isLeftClicked;
            Cursor.lockState = flag ? CursorLockMode.Confined : CursorLockMode.None;
            Cursor.visible   = !flag;
        }

        private void Rotate()
        {
            if (rotateOnLeftClick && !isLeftClicked) return;
            float r = 20f * rotateSpeed * dt;

            rotX += Input.GetAxis("Mouse X") * r;
            rotY += Input.GetAxis("Mouse Y") * r;
            rotY = Mathf.Clamp(rotY, -90f, 90f);

            var targetRot  = Quaternion.AngleAxis(rotX, Vector3.up);
                targetRot *= Quaternion.AngleAxis(rotY, Vector3.left);
            tr.localRotation = Quaternion.Slerp(tr.localRotation, targetRot, dt * 6.0f);
        }

        private void Move()
        {
            if (moveOnLeftClick && !isLeftClicked) return;

            float z = Input.GetAxis("Vertical");
            float x = Input.GetAxis("Horizontal");
            float y = 0f;
            float a = 1f; // 가속 or 감속

            if      (Input.GetKey(accelKey1) || Input.GetKey(accelKey2)) a = fastMoveFactor;
            else if (Input.GetKey(decelKey1) || Input.GetKey(decelKey2)) a = slowMoveFactor;
            if      (Input.GetKey(upKey)  ) y = +upDownSpeed;
            else if (Input.GetKey(downKey)) y = -upDownSpeed;

            float nz = moveSpeed * a * z * dt;
            float nx = moveSpeed * a * x * dt;
            float ny = (y != 0f) ? (y * a * dt) : 0f;

            tr.position += (nz * tr.forward) + (nx * tr.right) + (ny * Vector3.up);
        }
    }
}