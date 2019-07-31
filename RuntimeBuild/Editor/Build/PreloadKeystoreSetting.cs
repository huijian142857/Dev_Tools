#if UNITY_EDITOR
namespace HCR.Client.Editor
{
    using UnityEditor;
    [InitializeOnLoad]
    public class PreloadKeystoreSetting
    {
        static PreloadKeystoreSetting()
        {
            PlayerSettings.Android.keystoreName = "./RuntimeBuild/keystore/debug.keystore";
            PlayerSettings.Android.keystorePass = "android";
            PlayerSettings.Android.keyaliasName = "androiddebugkey";
            PlayerSettings.Android.keyaliasPass = "android";
        }
    }
}
#endif
