using DragonDogStudios.UnitySoFunctional.ScriptableObjects;
using Unit = System.ValueTuple;
using UnityEngine;
using UnityEngine.UI;

namespace DragonDogStudios.UnitySoFunctional.Utilities
{
    public class ToggleSetter : MonoBehaviour
    {
        public bool AlwaysUpdate;

        public bool ShouldInvert;

        public BoolVariable Toggled;

        public Toggle Toggle;


        // Use this for initialization
        void Start()
        {
            Toggle.onValueChanged.AddListener(ToggleClicked);
        }

        // Update is called once per frame
        void Update()
        {
            if (AlwaysUpdate)
            {
                Toggle.onValueChanged.RemoveAllListeners();
                Toggle.isOn = Toggled.GetValue() ^ ShouldInvert;
                Toggle.onValueChanged.AddListener(ToggleClicked);
            }
        }

        private void ToggleClicked(bool toggleValue)
        {
            Toggled.SetValue(toggleValue ^ ShouldInvert);
        }
    }
}