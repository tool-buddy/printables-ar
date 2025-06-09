using ToolBuddy.PrintablesAR.ARInteraction;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.UI
{
    public class StateMachinesDebugDisplay : MonoBehaviour
    {
        private Application.Application _application;
        private GUIStyle _labelStyle;

        private void Awake()
        {
            _application = FindAnyObjectByType<Application.Application>();
            _labelStyle = new GUIStyle()
            {
                fontSize = 64,
                normal = { textColor = Color.white}
            };
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label($"Application: {_application.State}", _labelStyle);

            ARInteractable _arInteractable = FindAnyObjectByType<ARInteractable>();
            if (_arInteractable != null)
                GUILayout.Label($"Interactable: {_arInteractable.State}", _labelStyle);

            GUILayout.EndVertical();
        }
    }
}