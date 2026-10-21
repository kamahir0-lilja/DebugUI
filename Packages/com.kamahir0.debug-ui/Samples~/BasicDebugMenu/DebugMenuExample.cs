using UnityEngine;

namespace Lilja.DebugUI.Samples
{
    public sealed class DebugMenuExample : MonoBehaviour
    {
        private void Start()
        {
            if (!Debug.isDebugBuild) return;
            DebugMenu.Initialize(new GenericDebugPage("Debug", builder =>
            {
                builder.Label("Lilja Debug UI");
                builder.PrimaryButton("Log message", () => Debug.Log("Debug action"));
                builder.NavigationButton("System", page =>
                {
                    page.Label($"Unity {Application.unityVersion}");
                    page.Label($"Platform: {Application.platform}");
                });
            }));
            DebugMenuOpenButton.Instantiate();
            DebugMenu.Show();
        }
    }
}
