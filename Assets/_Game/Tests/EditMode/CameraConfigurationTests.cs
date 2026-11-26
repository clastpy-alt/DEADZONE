using Deadzone.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deadzone.Tests
{
    /// <summary>Checks the default camera tuning without needing Play Mode (reads serialized defaults).</summary>
    public sealed class CameraConfigurationTests
    {
        private GameObject cameraObject;
        private SerializedObject serialized;

        [SetUp]
        public void SetUp()
        {
            cameraObject = new GameObject("TestCamera", typeof(Camera));
            var controller = cameraObject.AddComponent<IsometricCameraController>();
            serialized = new SerializedObject(controller);
        }

        [TearDown]
        public void TearDown()
        {
            serialized.Dispose();
            Object.DestroyImmediate(cameraObject);
        }

        private float Float(string field) => serialized.FindProperty(field).floatValue;

        [Test]
        public void Angles_AreTrueIsometric()
        {
            Assert.AreEqual(45f, Float("yaw"), 0.001f);
            Assert.AreEqual(35.264f, Float("pitch"), 0.01f);
        }

        [Test]
        public void ZoomRange_IsValidAndContainsStartZoom()
        {
            float min = Float("minZoom");
            float max = Float("maxZoom");
            float start = Float("startZoom");

            Assert.Greater(min, 0f);
            Assert.Less(min, max);
            Assert.GreaterOrEqual(start, min);
            Assert.LessOrEqual(start, max);
        }

        [Test]
        public void Bounds_AreOrderedAndContainStartFocus()
        {
            Vector2 min = serialized.FindProperty("boundsMin").vector2Value;
            Vector2 max = serialized.FindProperty("boundsMax").vector2Value;
            Vector3 focus = serialized.FindProperty("startFocus").vector3Value;

            Assert.Less(min.x, max.x);
            Assert.Less(min.y, max.y);
            Assert.GreaterOrEqual(focus.x, min.x);
            Assert.LessOrEqual(focus.x, max.x);
            Assert.GreaterOrEqual(focus.z, min.y);
            Assert.LessOrEqual(focus.z, max.y);
        }

        [Test]
        public void InputTuning_IsPositive()
        {
            Assert.Greater(Float("smoothing"), 0f);
            Assert.Greater(Float("dragThresholdPixels"), 0f);
            Assert.Greater(Float("wheelZoomStep"), 0f);
            Assert.Greater(Float("selectionDistance"), 0f);
            Assert.Greater(Float("cameraDistance"), 0f);
        }

        [Test]
        public void StartView_IsWideEnoughForTheWholeBase()
        {
            Assert.GreaterOrEqual(Float("startZoom"), 24f);
        }

        [Test]
        public void BuildingFocus_IsWithinZoomRangeAndLiftsBuildingAboveSheet()
        {
            float zoom = Float("selectionZoom");
            Assert.GreaterOrEqual(zoom, Float("minZoom"));
            Assert.LessOrEqual(zoom, Float("startZoom"), "Selecting a building must approach, never zoom out.");
            Assert.That(Float("selectionScreenLift"), Is.InRange(0.05f, 0.4f));
            Assert.IsTrue(serialized.FindProperty("focusOnSelect").boolValue);
            Assert.Greater(Float("panInertia"), 0f);
        }
    }
}
