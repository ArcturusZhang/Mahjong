using IngameDebugConsole;
using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        private const KeyCode ConsoleKey = KeyCode.BackQuote;
        public DebugLogManager debugLogManager;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(ConsoleKey))
            {
                ToggleDebugLogConsole();
            }
        }

        private void ToggleDebugLogConsole()
        {
            var active = debugLogManager.gameObject.activeSelf;
            debugLogManager.gameObject.SetActive(!active);
        }
    }
}
