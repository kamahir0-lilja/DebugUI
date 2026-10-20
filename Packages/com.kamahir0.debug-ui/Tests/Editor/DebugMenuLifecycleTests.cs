using System;
using System.Collections;
using System.Linq;
using Lilja.DebugUI.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Lilja.DebugUI.Tests
{
    public sealed class DebugMenuLifecycleTests
    {
        private bool _previousOptionsEnabled;
        private EnterPlayModeOptions _previousOptions;
        private DebugMenuOpenButton _openButton;
        private DebugMenuEditorWindow _editorWindow;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            _previousOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            yield return new EnterPlayMode(false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_editorWindow != null) _editorWindow.Close();
            if (Application.isPlaying)
            {
                DebugMenu.Shutdown();
                if (_openButton != null) Object.Destroy(_openButton.gameObject);
                yield return null;
                yield return new ExitPlayMode();
            }
            EditorSettings.enterPlayModeOptionsEnabled = _previousOptionsEnabled;
            EditorSettings.enterPlayModeOptions = _previousOptions;
        }

        [UnityTest]
        public IEnumerator DefaultResourcesAndPublicApiWork()
        {
            var settings = Resources.Load<PanelSettings>("DebugMenu/DebugMenuPanelSettings");
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.themeStyleSheet, Is.Not.Null);
            DebugMenu.Initialize(new GenericDebugPage("Root", builder =>
            {
                builder.Label("Hello");
                builder.NavigationButton("Child", page => page.Label("Child page"));
            }));
            Assert.That(DebugMenu.GetPage("Root"), Is.Not.Null);
            Assert.That(DebugMenu.GetPage("Child"), Is.Not.Null);
            Assert.That(DebugMenu.IsVisible, Is.False);
            DebugMenu.Show();
            Assert.That(DebugMenu.IsVisible, Is.True);
            DebugMenu.Hide();
            Assert.That(DebugMenu.IsVisible, Is.False);
            _openButton = DebugMenuOpenButton.Instantiate();
            var document = _openButton.GetComponent<UIDocument>();
            Assert.That(document.rootVisualElement.Q<Button>("debug-menu-open-button"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RepeatedInitializeKeepsOneMenuAndOriginalPage()
        {
            var root = new GenericDebugPage("Root", _ => { });
            DebugMenu.Initialize(root);
            DebugMenu.Initialize(new GenericDebugPage("Ignored", _ => Assert.Fail("Second root was configured")));
            Assert.That(MenuDocuments().Length, Is.EqualTo(1));
            Assert.That(DebugMenu.GetPage("Root"), Is.SameAs(root));
            Assert.That(DebugMenu.GetPage("Ignored"), Is.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShutdownAllowsImmediateReinitialization()
        {
            DebugMenu.Initialize(new GenericDebugPage("Old", _ => { }));
            DebugMenu.Show();
            DebugMenu.Shutdown();
            DebugMenu.Shutdown();
            Assert.That(DebugMenu.IsInitialized, Is.False);
            Assert.That(DebugMenu.IsVisible, Is.False);
            Assert.That(DebugMenu.GetPage("Old"), Is.Null);
            DebugMenu.Initialize(new GenericDebugPage("New", _ => { }));
            yield return null;
            Assert.That(DebugMenu.IsInitialized, Is.True);
            Assert.That(DebugMenu.GetPage("New"), Is.Not.Null);
            Assert.That(MenuDocuments().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DestroyingMenuReleasesStaticState()
        {
            DebugMenu.Initialize(new GenericDebugPage("Root", _ => { }));
            Object.Destroy(MenuDocuments().Single().gameObject);
            yield return null;
            Assert.That(DebugMenu.IsInitialized, Is.False);
            Assert.That(DebugMenu.GetPage("Root"), Is.Null);
        }

        [UnityTest]
        public IEnumerator FailedConfigurationCleansUpAndCanRetry()
        {
            Assert.Throws<InvalidOperationException>(() => DebugMenu.Initialize(
                new GenericDebugPage("Broken", _ => throw new InvalidOperationException("configuration failed"))));
            Assert.That(DebugMenu.IsInitialized, Is.False);
            yield return null;
            Assert.That(MenuDocuments(), Is.Empty);
            DebugMenu.Initialize(new GenericDebugPage("Retry", _ => { }));
            Assert.That(DebugMenu.IsInitialized, Is.True);
        }

        [UnityTest]
        public IEnumerator PlayModeRestartWithoutDomainReloadHasNoStaleState()
        {
            DebugMenu.Initialize(new GenericDebugPage("First session", _ => { }));
            yield return new ExitPlayMode();
            Assert.That(DebugMenu.IsInitialized, Is.False);
            Assert.That(DebugMenu.IsVisible, Is.False);
            yield return new EnterPlayMode(false);
            Assert.That(DebugMenu.IsInitialized, Is.False);
            Assert.That(MenuDocuments(), Is.Empty);
            DebugMenu.Initialize(new GenericDebugPage("Second session", _ => { }));
            Assert.That(DebugMenu.GetPage("Second session"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator EditorWindowReconnectsAfterShutdown()
        {
            DebugMenu.Initialize(new GenericDebugPage("First", _ => { }));
            _editorWindow = EditorWindow.GetWindow<DebugMenuEditorWindow>();
            _editorWindow.Show();
            yield return null;
            DebugMenu.Shutdown();
            yield return null;
            DebugMenu.Initialize(new GenericDebugPage("Second", builder => builder.Label("New page")));
            yield return null;
            yield return null;
            Assert.That(_editorWindow.rootVisualElement.Q<Label>(className: "c-label")?.text, Is.EqualTo("New page"));
        }

        private static UIDocument[] MenuDocuments() => Resources.FindObjectsOfTypeAll<UIDocument>()
            .Where(document => document.gameObject.name == "[DebugMenu]").ToArray();
    }
}
