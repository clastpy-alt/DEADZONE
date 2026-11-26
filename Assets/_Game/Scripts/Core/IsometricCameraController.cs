using Deadzone.Buildings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Deadzone.Core
{
    /// <summary>
    /// Orthographic isometric camera. Mouse drag / touch drag pan, wheel / pinch zoom,
    /// smoothed movement, XZ boundaries and building selection (tap without dragging).
    /// Uses the new Input System only.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private float yaw = 45f;
        [SerializeField] private float pitch = 35.264f;
        [SerializeField] private float cameraDistance = 120f;
        [SerializeField] private Vector3 startFocus = new Vector3(0f, 0f, 2f);

        [Header("Zoom (orthographic size)")]
        [SerializeField] private float startZoom = 14f;
        [SerializeField] private float minZoom = 7f;
        [SerializeField] private float maxZoom = 34f;
        [SerializeField] private float wheelZoomStep = 1.5f;

        [Header("Feel")]
        [SerializeField] private float smoothing = 12f;
        [SerializeField] private float dragThresholdPixels = 12f;

        [Header("Boundaries (world XZ)")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-40f, -45f);
        [SerializeField] private Vector2 boundsMax = new Vector2(40f, 40f);

        [Header("Selection")]
        [SerializeField] private LayerMask selectionMask = ~0;
        [SerializeField] private float selectionDistance = 400f;

        private static readonly List<RaycastResult> UiHits = new List<RaycastResult>();

        private Camera cam;
        private Vector3 targetFocus;
        private Vector3 currentFocus;
        private float targetZoom;
        private float currentZoom;

        private bool isPressing;
        private bool startedOverUi;
        private bool moved;
        private bool suppressGesture;
        private Vector2 pressPosition;
        private BuildingSelectable selected;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = cameraDistance * 3f;

            targetFocus = currentFocus = ClampFocus(startFocus);
            targetZoom = currentZoom = Mathf.Clamp(startZoom, minZoom, maxZoom);
            ApplyTransform();
        }

        private void Update()
        {
            if (!TryHandlePinch())
            {
                HandlePointer();
            }
            HandleWheel();

            float t = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            currentFocus = Vector3.Lerp(currentFocus, targetFocus, t);
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, t);
            ApplyTransform();
        }

        public void FocusOn(Vector3 worldPoint)
        {
            targetFocus = ClampFocus(new Vector3(worldPoint.x, 0f, worldPoint.z));
        }

        public void ResetView()
        {
            targetFocus = ClampFocus(startFocus);
            targetZoom = Mathf.Clamp(startZoom, minZoom, maxZoom);
        }

        private void ApplyTransform()
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = currentFocus - transform.forward * cameraDistance;
            cam.orthographicSize = currentZoom;
        }

        private Vector3 ClampFocus(Vector3 focus)
        {
            focus.x = Mathf.Clamp(focus.x, boundsMin.x, boundsMax.x);
            focus.z = Mathf.Clamp(focus.z, boundsMin.y, boundsMax.y);
            focus.y = 0f;
            return focus;
        }

        private void HandleWheel()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f || IsOverUi(mouse.position.ReadValue())) return;

            // Wheel delta is +-120 per notch on some platforms and +-1 on others: normalize to notches.
            float notches = Mathf.Sign(scroll) * Mathf.Clamp(Mathf.Abs(scroll) / 120f, 1f, 3f);
            targetZoom = Mathf.Clamp(targetZoom - notches * wheelZoomStep, minZoom, maxZoom);
        }

        private bool TryHandlePinch()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null) return false;

            int found = 0;
            Vector2 p0 = default, p1 = default, d0 = default, d1 = default;
            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (!touch.isInProgress) continue;

                Vector2 position = touch.position.ReadValue();
                Vector2 delta = touch.delta.ReadValue();
                if (found == 0) { p0 = position; d0 = delta; }
                else if (found == 1) { p1 = position; d1 = delta; }
                found++;
            }

            if (found < 2) return false;

            float current = (p0 - p1).magnitude;
            float previous = ((p0 - d0) - (p1 - d1)).magnitude;
            if (current > 1f && previous > 1f)
            {
                targetZoom = Mathf.Clamp(targetZoom * (previous / current), minZoom, maxZoom);
            }

            suppressGesture = true;
            return true;
        }

        private void HandlePointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                isPressing = true;
                moved = false;
                suppressGesture = false;
                pressPosition = pointer.position.ReadValue();
                startedOverUi = IsOverUi(pressPosition);
            }

            if (!isPressing) return;

            if (pointer.press.isPressed && !startedOverUi && !suppressGesture)
            {
                Vector2 position = pointer.position.ReadValue();
                if (!moved && (position - pressPosition).sqrMagnitude > dragThresholdPixels * dragThresholdPixels)
                {
                    moved = true;
                }
                if (moved)
                {
                    Pan(pointer.delta.ReadValue());
                }
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                if (!moved && !startedOverUi && !suppressGesture)
                {
                    TrySelect(pressPosition);
                }
                isPressing = false;
            }
        }

        private void Pan(Vector2 deltaPixels)
        {
            if (Screen.height <= 0) return;

            float unitsPerPixel = 2f * currentZoom / Screen.height;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 right = yawRotation * Vector3.right;
            Vector3 forward = yawRotation * Vector3.forward;
            float sinPitch = Mathf.Max(0.2f, Mathf.Sin(pitch * Mathf.Deg2Rad));

            Vector3 move = (-right * deltaPixels.x - forward * (deltaPixels.y / sinPitch)) * unitsPerPixel;
            targetFocus = ClampFocus(targetFocus + move);
        }

        private void TrySelect(Vector2 screenPosition)
        {
            BuildingSelectable hit = null;
            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit info, selectionDistance, selectionMask))
            {
                hit = info.collider.GetComponentInParent<BuildingSelectable>();
            }
            SetSelected(hit);
        }

        private void SetSelected(BuildingSelectable building)
        {
            if (selected != null && selected != building) selected.SetSelected(false);
            selected = building;
            if (selected != null) selected.SetSelected(true);
            GameEvents.RaiseBuildingSelected(selected);
        }

        /// <summary>Raycasts the UI directly, so it does not depend on which input module / pointer id is active.</summary>
        private static bool IsOverUi(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            var data = new PointerEventData(eventSystem) { position = screenPosition };
            UiHits.Clear();
            eventSystem.RaycastAll(data, UiHits);
            return UiHits.Count > 0;
        }
    }
}
