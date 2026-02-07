using UnityEngine;

namespace Game.Runtime.Services
{
    public static class Log
    {
        private const string Prefix = "[Game]";

        public static void Info(string message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public static void Warn(string message)
        {
            Debug.LogWarning($"{Prefix} {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"{Prefix} {message}");
        }
    }
}
