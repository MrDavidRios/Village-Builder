#if (UNITY_EDITOR)
using UnityEditor;
using System.Reflection;

static class UsefulShortcuts
{
    public static void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}
#endif
