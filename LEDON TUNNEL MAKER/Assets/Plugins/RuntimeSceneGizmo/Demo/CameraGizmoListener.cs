using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeSceneGizmo
{
    public class CameraGizmoListener : MonoBehaviour
    {
       
#pragma warning disable 0649
        [SerializeField]
        private float cameraAdjustmentSpeed = 3f;
        public TMP_Text projectionModeText;
        [SerializeField]
        private float projectionTransitionSpeed = 2f;
#pragma warning restore 0649
        private Vector3 originalPosition;
        private Camera mainCamera;
        private Transform mainCamParent;
        public Outline outline;
        
        private IEnumerator cameraRotateCoroutine, projectionChangeCoroutine;
        

        private void Awake()
        {
            mainCamera = Camera.main;
            mainCamParent = mainCamera.transform.parent;
            mainCamera.nearClipPlane = 0.01f;
        }

        private void OnDisable()
        {
            cameraRotateCoroutine = projectionChangeCoroutine = null;
        }
        private void Start()
        {
            SwitchOrthographicMode();
            
        }

        public void OnGizmoComponentClicked(GizmoComponent component)
        {
            if (component == GizmoComponent.Center)
                SwitchOrthographicMode();
            else if (component == GizmoComponent.XNegative)
            {
                RotateCameraInDirection(Vector3.right);
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    StartCoroutine(LerpToViewObject(startPart));
                }
                else
                {
                    Debug.LogError("No GameObject named 'StartPart' found.");
                }
            }
            else if (component == GizmoComponent.XPositive)
            {
                RotateCameraInDirection(-Vector3.right);
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    StartCoroutine(LerpToViewObject(startPart));
                }
                else
                {
                    Debug.LogError("No GameObject named 'StartPart' found.");
                }
            }
            else if (component == GizmoComponent.YNegative)
            {
                RotateCameraInDirection(Vector3.up);
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    StartCoroutine(LerpToViewObject(startPart));
                }
                else
                {
                    Debug.LogError("No GameObject named 'StartPart' found.");
                }

            }
            else if (component == GizmoComponent.YPositive)
            {
                RotateCameraInDirection(-Vector3.up);
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    StartCoroutine(LerpToViewObject(startPart));
                }
                else
                {
                    Debug.LogError("No GameObject named 'StartPart' found.");
                }
            }
            else if (component == GizmoComponent.ZNegative)
            {
                RotateCameraInDirection(Vector3.forward);
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    StartCoroutine(LerpToViewObject(startPart));
                }
                else
                {
                    Debug.LogError("No GameObject named 'StartPart' found.");
                }
            }
            else
            {
                RotateCameraInDirection(-Vector3.forward);
                GameObject startPart = GameObject.Find("StartPart");
                if (startPart != null)
                {
                    StartCoroutine(LerpToViewObject(startPart));
                }
                else
                {
                    Debug.LogError("No GameObject named 'StartPart' found.");
                }
            }
        }

        public void SwitchOrthographicMode()
        {
            if (projectionChangeCoroutine != null)
                return;

            projectionChangeCoroutine = SwitchProjection();
            StartCoroutine(projectionChangeCoroutine);
        }

        public void RotateCameraInDirection(Vector3 direction)
        {
            if (cameraRotateCoroutine != null)
                return;

            cameraRotateCoroutine = SetCameraRotation(direction);
            StartCoroutine(cameraRotateCoroutine);
        }



        private IEnumerator SwitchProjection()
        {
            bool isOrthographic = mainCamera.orthographic;
            Matrix4x4 dest, src = mainCamera.projectionMatrix;
            if (isOrthographic)
                dest = Matrix4x4.Perspective(mainCamera.fieldOfView, mainCamera.aspect, mainCamera.nearClipPlane, mainCamera.farClipPlane);
            else
            {
                float orthographicSize = mainCamera.orthographicSize;
                float aspect = mainCamera.aspect;
                dest = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, mainCamera.nearClipPlane, mainCamera.farClipPlane);
            }

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * projectionTransitionSpeed)
            {
                float lerpModifier = isOrthographic ? t * t : Mathf.Pow(t, 0.2f);
                Matrix4x4 matrix = new Matrix4x4();
                for (int i = 0; i < 16; i++)
                    matrix[i] = Mathf.LerpUnclamped(src[i], dest[i], lerpModifier);

                mainCamera.projectionMatrix = matrix;
                yield return null;
            }

            mainCamera.orthographic = !isOrthographic;

            if (mainCamera.orthographic)
            {
                mainCamera.nearClipPlane = -30f;
                originalPosition = mainCamera.transform.localPosition;  // Store the original position
                mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y, -20);
                 // Added this line to set the near clipping plane to -30
            }
            else
            {
                mainCamera.nearClipPlane = 0.1f;
                mainCamera.transform.localPosition = originalPosition;  // Restore the original position\
                
            }

            mainCamera.ResetProjectionMatrix();
            projectionModeText.text = mainCamera.orthographic ? "Isometric" : "Perspective";

            projectionChangeCoroutine = null;
        }

        private IEnumerator SetCameraRotation(Vector3 targetForward)
        {
            Quaternion initialRotation = mainCamParent.localRotation;
            Quaternion targetRotation;
            if (Mathf.Abs(targetForward.y) < 0.99f)
                targetRotation = Quaternion.LookRotation(targetForward);
            else
            {
                Vector3 cameraForward = mainCamParent.forward;
                if (cameraForward.x == 0f && cameraForward.z == 0f)
                    cameraForward.y = 1f;
                else if (Mathf.Abs(cameraForward.x) > Mathf.Abs(cameraForward.z))
                {
                    cameraForward.x = Mathf.Sign(cameraForward.x);
                    cameraForward.y = 0f;
                    cameraForward.z = 0f;
                }
                else
                {
                    cameraForward.x = 0f;
                    cameraForward.y = 0f;
                    cameraForward.z = Mathf.Sign(cameraForward.z);
                }

                if (targetForward.y > 0f)
                    cameraForward = -cameraForward;

                targetRotation = Quaternion.LookRotation(targetForward, cameraForward);
            }

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * cameraAdjustmentSpeed)
            {
                mainCamParent.localRotation = Quaternion.LerpUnclamped(initialRotation, targetRotation, t);
                yield return null;
            }

            mainCamParent.localRotation = targetRotation;
            cameraRotateCoroutine = null;
        }
        private IEnumerator LerpToViewObject(GameObject targetObject)
        {
            float lerpDuration = 1f;
            float startTime = Time.time;
            Vector3 startPosition = mainCamParent.position;

            Bounds bounds = CalculateObjectBounds(targetObject);
            Vector3 targetCenter = bounds.center;

            // Calculate target position of the camera parent
            Vector3 targetPosition = startPosition;

            // Always adjust X, Y and Z positions
            targetPosition.x = targetCenter.x;
            targetPosition.z = targetCenter.z;
            targetPosition.y = targetCenter.y - mainCamParent.GetChild(0).localPosition.y;

            // Perform smooth transition to the target position
            while (Time.time - startTime < lerpDuration)
            {
                float t = (Time.time - startTime) / lerpDuration;
                mainCamParent.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            mainCamParent.position = targetPosition;
        }
        private Bounds CalculateObjectBounds(GameObject rootObject)
        {
            Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(rootObject.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }
    }

}