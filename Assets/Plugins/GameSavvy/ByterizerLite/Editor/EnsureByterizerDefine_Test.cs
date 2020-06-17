#if BYTERIZER
#else
    #define BYTERIZER
#endif

#if UNITY_EDITOR

namespace GameSavvy.Byterizer.Tests
{
    using System;
    using System.Linq;
    using UnityEditor;

    internal static class EnsureByterizerDefine_Test
    {
        private const string _DEFINE = "BYTERIZER";

        [InitializeOnLoadMethod]
        private static void EnsureScriptingDefineSymbol()
        {
            var currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            if (currentTarget == BuildTargetGroup.Unknown)
            {
                return;
            }

            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Trim();
            var defines       = definesString.Split(';');

            if (defines.Contains(_DEFINE)) return;

            if (definesString.EndsWith(";", StringComparison.InvariantCulture) == false)
            {
                definesString += ";";
            }

            definesString += _DEFINE;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
        }
    }
}
#endif