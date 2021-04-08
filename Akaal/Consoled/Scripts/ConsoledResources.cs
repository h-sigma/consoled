using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;

#endif

namespace Akaal.Consoled
{
    public static class ConsoledResources
    {
#if UNITY_EDITOR
        public static VisualTreeAsset WindowAsset()
        {
            return Resources.Load<VisualTreeAsset>("Consoled/ConsoledWindow_UXML");
        }

        public static StyleSheet StyleSheet()
        {
            return Resources.Load<StyleSheet>("Consoled/ConsoledWindow_USS");
        }
#endif

        public static Texture2D LogoTexture()
        {
            return Resources.Load<Texture2D>("Consoled/ConsoledLogo");
        }

        public static Font Font()
        {
            return Resources.Load<Font>("Consoled/CascadiaMono");
        }
    }
}