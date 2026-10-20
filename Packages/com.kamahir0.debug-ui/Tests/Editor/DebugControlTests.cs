using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lilja.DebugUI.Tests
{
    public sealed class DebugControlTests
    {
        private DebugControlTestWindow _host;

        [SetUp]
        public void SetUp()
        {
            _host = ScriptableObject.CreateInstance<DebugControlTestWindow>();
            _host.Show();
        }

        [TearDown]
        public void TearDown() => _host.Close();

        [TestCase("Integer")]
        [TestCase("Long")]
        [TestCase("Float")]
        [TestCase("Double")]
        [TestCase("Text")]
        [TestCase("Slider")]
        [TestCase("SliderInt")]
        [TestCase("Range")]
        [TestCase("Vector2")]
        [TestCase("Vector2Int")]
        [TestCase("Vector3")]
        [TestCase("Vector3Int")]
        [TestCase("Vector4")]
        [TestCase("Rect")]
        [TestCase("RectInt")]
        [TestCase("Bounds")]
        [TestCase("BoundsInt")]
        [TestCase("Enum")]
        [TestCase("Radio")]
        public void FieldsRoundTripAndNotifyExactlyOnce(string kind)
        {
            switch (kind)
            {
                case "Integer": Verify(new DebugIntegerField(), int.MinValue, int.MaxValue); break;
                case "Long": Verify(new DebugLongField(), long.MinValue, long.MaxValue); break;
                case "Float": Verify(new DebugFloatField(), -1.25f, 2.5f); break;
                case "Double": Verify(new DebugDoubleField(), -1.23456789012345, 2.5); break;
                case "Text": Verify(new DebugTextField(), "", "日本語 / multiline\nsecond line"); break;
                case "Slider": Verify(new DebugSlider { lowValue = -10, highValue = 10 }, -10f, 10f); break;
                case "SliderInt": Verify(new DebugSliderInt { lowValue = -10, highValue = 10 }, -10, 10); break;
                case "Range": Verify(new DebugMinMaxSlider { lowLimit = 0, highLimit = 10 }, new Vector2(0, 1), new Vector2(5, 10)); break;
                case "Vector2": Verify(new DebugVector2Field(), Vector2.zero, new Vector2(-1, 2)); break;
                case "Vector2Int": Verify(new DebugVector2IntField(), Vector2Int.zero, new Vector2Int(-1, 2)); break;
                case "Vector3": Verify(new DebugVector3Field(), Vector3.zero, new Vector3(-1, 2, 3)); break;
                case "Vector3Int": Verify(new DebugVector3IntField(), Vector3Int.zero, new Vector3Int(-1, 2, 3)); break;
                case "Vector4": Verify(new DebugVector4Field(), Vector4.zero, new Vector4(-1, 2, 3, 4)); break;
                case "Rect": Verify(new DebugRectField(), Rect.zero, new Rect(-1, 2, 3, 4)); break;
                case "RectInt": Verify(new DebugRectIntField(), new RectInt(), new RectInt(-1, 2, 3, 4)); break;
                case "Bounds": Verify(new DebugBoundsField(), new Bounds(), new Bounds(Vector3.one, Vector3.one * 2)); break;
                case "BoundsInt": Verify(new DebugBoundsIntField(), new BoundsInt(), new BoundsInt(Vector3Int.one, Vector3Int.one * 2)); break;
                case "Enum":
                    var enumField = new DebugEnumField();
                    enumField.Init(TestChoice.Alpha);
                    Verify<Enum>(enumField, TestChoice.Alpha, TestChoice.Beta);
                    break;
                case "Radio": Verify(new DebugRadioButtonGroup { choices = new[] { "One", "Two", "Three" } }, 0, 2); break;
            }
        }

        private void Verify<T>(BaseField<T> field, T initial, T next)
        {
            _host.rootVisualElement.Add(field);
            var count = 0;
            T previous = default, current = default;
            field.RegisterValueChangedCallback(evt => { count++; previous = evt.previousValue; current = evt.newValue; });
            field.SetValueWithoutNotify(initial);
            Assert.That(field.value, Is.EqualTo(initial));
            Assert.That(count, Is.Zero);
            field.value = next;
            Assert.That(field.value, Is.EqualTo(next));
            Assert.That(previous, Is.EqualTo(initial));
            Assert.That(current, Is.EqualTo(next));
            Assert.That(count, Is.EqualTo(1));
            field.value = next;
            Assert.That(count, Is.EqualTo(1));
        }

        [TestCase(-100f, 0f)]
        [TestCase(100f, 10f)]
        public void SliderClampsAtEndpoints(float input, float expected)
        {
            var slider = new DebugSlider { lowValue = 0, highValue = 10, value = input };
            Assert.That(slider.value, Is.EqualTo(expected));
        }

        [Test]
        public void OrderedGroupIsStableAndDisposalIsIdempotent()
        {
            var group = new OrderedGroup<string>((item, builder) => builder.Label(item));
            _host.rootVisualElement.Add(group);
            group.Add(20, "last");
            var first = group.Add(10, "first");
            group.Add(10, "second");
            Assert.That(group.Query<DebugLabel>().ToList().Select(label => label.text), Is.EqualTo(new[] { "first", "second", "last" }));
            first.Dispose();
            first.Dispose();
            Assert.That(group.Query<DebugLabel>().ToList().Select(label => label.text), Is.EqualTo(new[] { "second", "last" }));
        }

        [Test]
        public void VirtualFoldoutTracksMultipleHandles()
        {
            var foldout = new VirtualFoldout("Dynamic");
            _host.rootVisualElement.Add(foldout);
            Assert.That(foldout.style.display.value, Is.EqualTo(DisplayStyle.None));
            var one = foldout.AddDebugUI(builder => builder.Label("One"));
            var two = foldout.AddDebugUI(builder => builder.Label("Two"));
            Assert.That(foldout.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            one.Dispose(); one.Dispose();
            Assert.That(foldout.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            two.Dispose();
            Assert.That(foldout.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(foldout.contentContainer.childCount, Is.Zero);
        }

        [Test]
        public void RelativeInsertionsHaveCorrectOrderAndCanBeDisposed()
        {
            var anchor = new DebugLabel("Anchor");
            _host.rootVisualElement.Add(anchor);
            var before = anchor.PlaceInFront(builder => builder.Label("Before"));
            var after = anchor.PlaceBehind(builder => builder.Label("After"));
            Assert.That(_host.rootVisualElement.Query<DebugLabel>().ToList().Select(label => label.text), Is.EqualTo(new[] { "Before", "Anchor", "After" }));
            before.Dispose(); after.Dispose();
            Assert.That(_host.rootVisualElement.childCount, Is.EqualTo(1));
        }

        [Test]
        public void DetachedAnchorRejectsRelativeInsertion()
        {
            Assert.Throws<InvalidOperationException>(() => new DebugLabel().PlaceBehind(builder => builder.Label("Child")));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(64)]
        public void ToggleGroupReflectsAttachedItemsAndSilentUpdates(int count)
        {
            var group = new DebugToggleGroup("Group");
            for (var i = 0; i < count; i++) group.Add(new DebugToggleGroupItem($"Item {i}"));
            _host.rootVisualElement.Add(group);
            Assert.That(group.Value.length, Is.EqualTo(count));
            var notifications = 0;
            group.RegisterValueChangedCallback(_ => notifications++);
            var state = new ToggleButtonGroupState(0, count);
            if (count > 0) state[count - 1] = true;
            group.SetValueWithoutNotify(state);
            Assert.That(notifications, Is.Zero);
            if (count > 0) Assert.That(group.Value[count - 1], Is.True);
            group.Clear();
            Assert.That(group.Value.length, Is.Zero);
        }

        [Test]
        public void ToggleGroupNotifiesOnceForOneItemChange()
        {
            var group = new DebugToggleGroup();
            var item = new DebugToggleGroupItem("One");
            group.Add(item);
            _host.rootVisualElement.Add(group);
            var count = 0;
            group.RegisterValueChangedCallback(_ => count++);
            item.Value = true;
            Assert.That(group.Value[0], Is.True);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void ToggleItemSupportsKeyboardSubmit()
        {
            var item = new DebugToggleGroupItem("Keyboard");
            _host.rootVisualElement.Add(item);
            item.Focus();
            using var evt = NavigationSubmitEvent.GetPooled();
            evt.target = item;
            item.SendEvent(evt);
            Assert.That(item.Value, Is.True);
        }

        private enum TestChoice { Alpha, Beta }
    }

    internal sealed class DebugControlTestWindow : EditorWindow { }
}
