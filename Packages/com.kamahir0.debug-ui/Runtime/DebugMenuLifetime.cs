using UnityEngine;
using UnityEngine.UIElements;

namespace Lilja.DebugUI
{
    [AddComponentMenu("")]
    internal sealed class DebugMenuLifetime : MonoBehaviour
    {
        internal UIDocument Document { get; set; }

        private void OnDestroy() => DebugMenu.OnDocumentDestroyed(Document);
    }
}
