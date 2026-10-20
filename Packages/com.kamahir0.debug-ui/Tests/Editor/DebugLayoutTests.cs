using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Lilja.DebugUI.Tests
{
    public sealed class DebugLayoutTests
    {
        private DebugControlTestWindow _host;
        private const string LeftKey = "LiljaDebugMenu.WindowLeft";
        private const string TopKey = "LiljaDebugMenu.WindowTop";
        private const string PositionKey = "LiljaDebugMenu.WindowHasPosition";
        private bool _hadLeft, _hadTop, _hadPosition, _position;
        private float _left, _top;

        [SetUp]
        public void SetUp()
        {
            _hadLeft = EditorPrefs.HasKey(LeftKey); _left = EditorPrefs.GetFloat(LeftKey);
            _hadTop = EditorPrefs.HasKey(TopKey); _top = EditorPrefs.GetFloat(TopKey);
            _hadPosition = EditorPrefs.HasKey(PositionKey); _position = EditorPrefs.GetBool(PositionKey);
            _host = ScriptableObject.CreateInstance<DebugControlTestWindow>();
            _host.rootVisualElement.styleSheets.Add(Resources.Load<PanelSettings>("DebugMenu/DebugMenuPanelSettings").themeStyleSheet);
            _host.Show();
        }

        [TearDown]
        public void TearDown()
        {
            _host.Close();
            if (_hadLeft) EditorPrefs.SetFloat(LeftKey, _left); else EditorPrefs.DeleteKey(LeftKey);
            if (_hadTop) EditorPrefs.SetFloat(TopKey, _top); else EditorPrefs.DeleteKey(TopKey);
            if (_hadPosition) EditorPrefs.SetBool(PositionKey, _position); else EditorPrefs.DeleteKey(PositionKey);
        }

        [UnityTest]
        public IEnumerator SavedPositionOutsideViewportIsRecovered()
        {
            EditorPrefs.SetBool(PositionKey, true);
            EditorPrefs.SetFloat(LeftKey, 4000);
            EditorPrefs.SetFloat(TopKey, 4000);
            _host.position = new Rect(100, 100, 320, 600);
            var template = Resources.Load<VisualTreeAsset>("DebugMenu/DebugMenu").Instantiate();
            template.style.flexGrow = 1;
            _host.rootVisualElement.Add(template);
            yield return null; yield return null; yield return null;
            AssertMenuIsAccessible(template);
        }

        [UnityTest]
        public IEnumerator ViewportShrinkKeepsSavedMenuAccessible()
        {
            EditorPrefs.SetBool(PositionKey, true);
            EditorPrefs.SetFloat(LeftKey, 350);
            EditorPrefs.SetFloat(TopKey, 100);
            _host.position = new Rect(100, 100, 960, 700);
            var template = Resources.Load<VisualTreeAsset>("DebugMenu/DebugMenu").Instantiate();
            template.style.flexGrow = 1;
            _host.rootVisualElement.Add(template);
            yield return null; yield return null;
            Assert.That(template.Q("debug-menu-window").resolvedStyle.left, Is.GreaterThan(0));
            _host.position = new Rect(100, 100, 320, 300);
            yield return null; yield return null; yield return null;
            AssertMenuIsAccessible(template);
        }

        private void AssertMenuIsAccessible(VisualElement template)
        {
            var menu = template.Q("debug-menu-window");
            var viewport = _host.rootVisualElement.worldBound;
            var header = menu.Q(className: "c-menu-window__header").worldBound;
            Assert.That(header.xMin, Is.GreaterThanOrEqualTo(viewport.xMin - 1));
            Assert.That(header.xMax, Is.LessThanOrEqualTo(viewport.xMax + 1));
            Assert.That(header.yMin, Is.GreaterThanOrEqualTo(viewport.yMin - 1));
            Assert.That(header.yMax, Is.LessThanOrEqualTo(viewport.yMax + 1));
            Assert.That(menu.worldBound.yMax, Is.LessThanOrEqualTo(viewport.yMax + 1));
        }

        [UnityTest]
        public IEnumerator HorizontalButtonsWrapWithoutOverlapAtNarrowWidths()
        {
            var page = new DebugPage();
            _host.rootVisualElement.Add(page);
            page.AddDebugUI(builder => builder.HorizontalScope(row =>
            {
                row.PrimaryButton("Primary");
                row.SecondaryButton("Secondary");
                row.DangerButton("Danger");
            }).name = "row");
            foreach (var width in new[] { 240, 480, 960 })
            {
                _host.position = new Rect(100, 100, width, 600);
                yield return null; yield return null;
                var row = page.Q("row");
                var buttons = row.Query<Button>().ToList();
                Assert.That(buttons.Count, Is.EqualTo(3));
                for (var i = 0; i < buttons.Count; i++)
                {
                    var bounds = buttons[i].worldBound;
                    Assert.That(bounds.width, Is.GreaterThan(0));
                    Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(row.worldBound.xMin - 1));
                    Assert.That(bounds.xMax, Is.LessThanOrEqualTo(row.worldBound.xMax + 1));
                    for (var j = i + 1; j < buttons.Count; j++)
                        Assert.That(bounds.Overlaps(buttons[j].worldBound), Is.False, $"Width {width}");
                }
            }
        }

        [UnityTest]
        public IEnumerator LongLabelsWrapAndStayInsideTheirPage()
        {
            _host.position = new Rect(100, 100, 320, 600);
            var page = new DebugPage();
            _host.rootVisualElement.Add(page);
            var label = new DebugLabel("A long description that must remain readable when the page is narrow. 日本語の長い説明文も途中で切れずに折り返します。");
            page.Add(label);
            yield return null; yield return null;
            Assert.That(label.worldBound.xMax, Is.LessThanOrEqualTo(page.worldBound.xMax + 1));
            Assert.That(label.resolvedStyle.height, Is.GreaterThan(label.resolvedStyle.fontSize * 2));
        }

        [UnityTest]
        public IEnumerator MenuWidthFitsBothCompactAndWideViewports()
        {
            var template = Resources.Load<VisualTreeAsset>("DebugMenu/DebugMenu").Instantiate();
            template.style.flexGrow = 1;
            _host.rootVisualElement.Add(template);
            foreach (var width in new[] { 240, 480, 960 })
            {
                _host.position = new Rect(100, 100, width, 600);
                yield return null; yield return null;
                var menu = template.Q("debug-menu-window");
                Assert.That(menu.resolvedStyle.width, Is.GreaterThan(0));
                Assert.That(menu.resolvedStyle.width, Is.LessThanOrEqualTo(width + 1));
            }
        }

        [UnityTest]
        public IEnumerator ProgressBarUsesItsControlHeight()
        {
            var progress = new DebugProgressBar { value = 50, title = "Progress 50%" };
            _host.rootVisualElement.Add(progress);
            yield return null; yield return null;
            var container = progress.Q(className: "unity-progress-bar__container");
            Assert.That(container.worldBound.height, Is.GreaterThanOrEqualTo(progress.worldBound.height - 1));
        }
    }
}
