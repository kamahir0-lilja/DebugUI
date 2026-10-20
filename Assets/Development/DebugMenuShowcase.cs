using System;
using System.Collections.Generic;
using Lilja.DebugUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lilja.DebugUI.Development
{
    public sealed class DebugMenuShowcase : MonoBehaviour
    {
        public static DebugMenuShowcase Current { get; private set; }
        public int ActionCount { get; private set; }
        public int ChangeCount { get; private set; }
        public DebugLabel Status { get; private set; }
        public DebugMenuOpenButton OpenButton { get; private set; }
        private readonly List<IDisposable> _dynamic = new();
        public enum Choice { Alpha, Beta, Gamma }

        private void Start()
        {
            Current = this;
            Initialize();
        }

        public void Initialize()
        {
            DebugMenu.Initialize(new GenericDebugPage("Debug UI • Validation", Home));
            OpenButton = DebugMenuOpenButton.Instantiate();
            DebugMenu.Show();
        }

        private void Changed() => ChangeCount++;
        private void Action()
        {
            ActionCount++;
            if (Status != null) Status.text = $"Actions: {ActionCount}";
        }

        private void Home(IDebugUIBuilder b)
        {
            b.Label("Debug UI validation / 動作確認").name = "qa-title";
            Status = b.Label("Actions: 0");
            Status.name = "qa-status";
            b.HorizontalScope(row =>
            {
                row.PrimaryButton("Primary", Action).name = "qa-primary";
                row.SecondaryButton("Secondary", Action).name = "qa-secondary";
                row.DangerButton("Danger", Action).name = "qa-danger";
            });
            b.PrimaryButton("Disabled", Action).SetEnabled(false);
            b.NavigationButton("Text and selections", Inputs).name = "qa-nav-inputs";
            b.NavigationButton("Numbers and sliders", Numbers).name = "qa-nav-numbers";
            b.NavigationButton("Vectors and bounds", Vectors).name = "qa-nav-vectors";
            b.NavigationButton("Groups and dynamic UI", Groups).name = "qa-nav-groups";
            b.NavigationButton("Long content (200 rows)", LongContent).name = "qa-nav-long";
            b.NavigationButton("Empty page", _ => { }).name = "qa-nav-empty";
            b.NavigationButton("Deep navigation", Deep).name = "qa-nav-deep";
            b.TempNavigationButton("Temporary page", p => p.Label("Temporary content")).name = "qa-nav-temp";
            b.SecondaryButton("Hide menu", DebugMenu.Hide).name = "qa-hide";
        }

        private void Inputs(IDebugUIBuilder b)
        {
            b.TextField("Text", "Hello 日本語", _ => Changed()).name = "qa-text";
            b.TextField("Long label / 長いラベル / long label", "Editable", _ => Changed()).name = "qa-long-text";
            b.EnumField("Enum", Choice.Beta, _ => Changed()).name = "qa-enum";
            var radio = b.VisualElement(new DebugRadioButtonGroup("Radio") { choices = new[] { "Alpha", "Beta", "Gamma" }, value = 1 });
            radio.name = "qa-radio";
            radio.RegisterValueChangedCallback(_ => Changed());
            var group = b.VisualElement(new DebugToggleGroup("Multiple choices"));
            group.name = "qa-toggles";
            group.Add(new DebugToggleGroupItem("One") { name = "qa-toggle-one" });
            group.Add(new DebugToggleGroupItem("Two") { name = "qa-toggle-two", Value = true });
            group.Add(new DebugToggleGroupItem("Three") { name = "qa-toggle-three" });
            group.RegisterValueChangedCallback(_ => Changed());
            b.Label("日本語 / English / العربية / 0123456789");
            b.Label("A deliberately long line to inspect wrapping and clipping when the menu or Editor pane is narrow.");
        }

        private void Numbers(IDebugUIBuilder b)
        {
            b.IntegerField("Integer", -42, _ => Changed()).name = "qa-integer";
            b.LongField("Long", 9223372036854775807L, _ => Changed()).name = "qa-long";
            b.FloatField("Float", 1.25f, _ => Changed()).name = "qa-float";
            b.DoubleField("Double", 1.23456789012345, _ => Changed()).name = "qa-double";
            b.Slider("Slider", 0.5f, 0, 1, _ => Changed()).name = "qa-slider";
            b.SliderInt("Slider int", 5, 0, 10, _ => Changed()).name = "qa-slider-int";
            b.MinMaxSlider("Range", new Vector2(2, 8), 0, 10, _ => Changed()).name = "qa-range";
            b.ProgressBar("Progress 0%", 0).name = "qa-progress-0";
            b.ProgressBar("Progress 50%", 50).name = "qa-progress-50";
            b.ProgressBar("Progress 100%", 100).name = "qa-progress-100";
        }

        private void Vectors(IDebugUIBuilder b)
        {
            b.Vector2Field("Vector2", new Vector2(1, 2), _ => Changed()).name = "qa-vector2";
            b.Vector2IntField("Vector2Int", new Vector2Int(-1, 2), _ => Changed()).name = "qa-vector2int";
            b.Vector3Field("Vector3", new Vector3(1, 2, 3), _ => Changed()).name = "qa-vector3";
            b.Vector3IntField("Vector3Int", new Vector3Int(1, -2, 3), _ => Changed()).name = "qa-vector3int";
            b.Vector4Field("Vector4", new Vector4(1, 2, 3, 4), _ => Changed()).name = "qa-vector4";
            b.RectField("Rect", new Rect(1, 2, 3, 4), _ => Changed()).name = "qa-rect";
            b.RectIntField("RectInt", new RectInt(-1, 2, 3, 4), _ => Changed()).name = "qa-rectint";
            b.BoundsField("Bounds", new Bounds(Vector3.one, Vector3.one * 2), _ => Changed()).name = "qa-bounds";
            b.BoundsIntField("BoundsInt", new BoundsInt(Vector3Int.zero, Vector3Int.one * 2), _ => Changed()).name = "qa-boundsint";
        }

        private void Groups(IDebugUIBuilder b)
        {
            b.Foldout("Nested foldout", f =>
            {
                f.Label("Inside foldout");
                f.Foldout("Nested again", nested => nested.PrimaryButton("Nested button", Action));
            }).name = "qa-foldout";
            var virtualFoldout = b.VirtualFoldout("Appears when populated");
            virtualFoldout.name = "qa-virtual-foldout";
            b.PrimaryButton("Add dynamic row", () => _dynamic.Add(virtualFoldout.AddDebugUI(p => p.Label($"Dynamic row {_dynamic.Count}")))).name = "qa-add";
            b.SecondaryButton("Remove dynamic rows", () =>
            {
                foreach (var handle in _dynamic) handle.Dispose();
                _dynamic.Clear();
            }).name = "qa-remove";
            var ordered = b.OrderedGroup<string>((item, builder) => builder.Label(item));
            ordered.name = "qa-ordered";
            ordered.Add(30, "Order 30");
            ordered.Add(10, "Order 10");
            ordered.Add(20, "Order 20");
        }

        private void LongContent(IDebugUIBuilder b)
        {
            for (var i = 0; i < 200; i++) b.Label($"Row {i:000} — long scroll content");
            b.PrimaryButton("Last row action", Action).name = "qa-last-row";
        }

        private void Deep(IDebugUIBuilder b)
        {
            b.Label("Depth 1");
            b.NavigationButton("Depth 2", child =>
            {
                child.Label("Depth 2");
                child.NavigationButton("Depth 3", leaf => leaf.Label("Depth 3"));
            });
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
