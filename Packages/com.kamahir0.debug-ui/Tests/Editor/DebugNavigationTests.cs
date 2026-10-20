using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Lilja.DebugUI.Tests
{
    public sealed class DebugNavigationTests
    {
        private bool _oldEnabled;
        private EnterPlayModeOptions _oldOptions;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _oldEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            _oldOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            yield return new EnterPlayMode(false);
            DebugMenu.Initialize(new GenericDebugPage("Root", builder =>
            {
                builder.NavigationButton("A", a => a.Label("Page A"));
                builder.NavigationButton("B", b => b.Label("Page B"));
            }));
            DebugMenu.Show();
            yield return Settle();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            DebugMenu.Shutdown();
            foreach (var button in Resources.FindObjectsOfTypeAll<DebugMenuOpenButton>())
                Object.Destroy(button.gameObject);
            yield return null;
            yield return new ExitPlayMode();
            EditorSettings.enterPlayModeOptionsEnabled = _oldEnabled;
            EditorSettings.enterPlayModeOptions = _oldOptions;
        }

        [UnityTest]
        public IEnumerator NavigationBackAndRootPreserveInstances()
        {
            var a = DebugMenu.GetPage("A");
            DebugMenu.NavigateTo("A"); yield return Settle(); AssertCurrent("A");
            DebugMenu.NavigateTo("B"); yield return Settle(); AssertCurrent("B");
            DebugMenu.Back(); yield return Settle(); AssertCurrent("A");
            Assert.That(DebugMenu.GetPage("A"), Is.SameAs(a));
            DebugMenu.BackToRoot(); yield return Settle(); AssertCurrent("Root");
            DebugMenu.Back(); yield return Settle(); AssertCurrent("Root");
        }

        [UnityTest]
        public IEnumerator RepeatedPageNavigationDoesNotCorruptHistory()
        {
            DebugMenu.NavigateTo("A"); yield return Settle();
            DebugMenu.NavigateTo("A"); yield return Settle(); AssertCurrent("A");
            DebugMenu.Back(); yield return Settle(); AssertCurrent("Root");
        }

        [UnityTest]
        public IEnumerator CyclicNavigationCanReturnToRoot()
        {
            foreach (var name in new[] { "A", "B", "A", "Root", "B" })
            {
                DebugMenu.NavigateTo(name); yield return Settle(); AssertCurrent(name);
            }
            DebugMenu.BackToRoot(); yield return Settle(); AssertCurrent("Root");
        }

        [UnityTest]
        public IEnumerator TemporaryPagesAreReleasedAfterBack()
        {
            var initial = Document().rootVisualElement.Query<DebugPage>().ToList().Count;
            for (var i = 0; i < 5; i++)
            {
                DebugMenu.NavigateToTemp("Temporary", builder => builder.Label("Temporary content"));
                yield return Settle();
                AssertCurrent("Temporary");
                DebugMenu.Back(); yield return Settle(); AssertCurrent("Root");
            }
            Assert.That(Document().rootVisualElement.Query<DebugPage>().ToList().Count, Is.EqualTo(initial));
        }

        [UnityTest]
        public IEnumerator RapidShowHideEndsInRequestedState()
        {
            for (var i = 0; i < 20; i++) { DebugMenu.Hide(); DebugMenu.Show(); }
            DebugMenu.Hide(); yield return Settle();
            Assert.That(DebugMenu.IsVisible, Is.False);
            DebugMenu.Show(); yield return Settle();
            Assert.That(DebugMenu.IsVisible, Is.True);
            Assert.That(Document().rootVisualElement.Q("debug-menu-window").resolvedStyle.opacity, Is.EqualTo(1).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator ShutdownDuringNavigationAllowsCleanRestart()
        {
            DebugMenu.NavigateTo("A");
            DebugMenu.Shutdown();
            DebugMenu.Initialize(new GenericDebugPage("Restart", builder => builder.Label("Restart")));
            DebugMenu.Show(); yield return Settle();
            AssertCurrent("Restart");
            Assert.That(Resources.FindObjectsOfTypeAll<UIDocument>().Count(d => d.gameObject.name == "[DebugMenu]"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OpenButtonFactoryIsIdempotentBeforeStart()
        {
            var one = DebugMenuOpenButton.Instantiate();
            var two = DebugMenuOpenButton.Instantiate();
            Assert.That(two, Is.SameAs(one));
            yield return null;
            Assert.That(Resources.FindObjectsOfTypeAll<DebugMenuOpenButton>().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OpenButtonSettingsClampAndAllPositionsRemainOnScreen()
        {
            var button = DebugMenuOpenButton.Instantiate();
            button.RequiredClicks = -10;
            button.ThresholdSeconds = -10;
            Assert.That(button.RequiredClicks, Is.EqualTo(2));
            Assert.That(button.ThresholdSeconds, Is.EqualTo(0.01f));
            foreach (DebugMenuButtonPosition position in Enum.GetValues(typeof(DebugMenuButtonPosition)))
            {
                button.ButtonPosition = position;
                yield return null;
                var element = button.GetComponent<UIDocument>().rootVisualElement.Q<Button>("debug-menu-open-button");
                var viewport = element.panel.visualTree.worldBound;
                Assert.That(viewport.Contains(element.worldBound.center), Is.True, position.ToString());
            }
        }

        private static IEnumerator Settle() { yield return new WaitForSecondsRealtime(0.48f); }
        private static UIDocument Document() => Resources.FindObjectsOfTypeAll<UIDocument>().Single(d => d.gameObject.name == "[DebugMenu]");
        private static void AssertCurrent(string name)
        {
            var title = Document().rootVisualElement.Q<Label>(className: "c-menu-window__title");
            Assert.That(title.text, Is.EqualTo(name));
            Assert.That(Document().rootVisualElement.Query<DebugPage>().ToList().Any(page => page.name == name && Mathf.Abs(page.resolvedStyle.left) < 1), Is.True);
        }
    }
}
