using UnityEngine;
using UnityEngine.UIElements;

namespace Akaal.Consoled.Editor
{
    public static class ConsoledResources
    {
        public static VisualTreeAsset WindowAsset()
        {
            return Resources.Load<VisualTreeAsset>("Consoled/ConsoledWindow_UXML");
        }

        public static StyleSheet StyleSheet()
        {
            return Resources.Load<StyleSheet>("Consoled/ConsoledWindow_USS");
        }

        public static Texture2D LogoTexture()
        {
            return Resources.Load<Texture2D>("Consoled/ConsoledLogo");
        }
    }
}