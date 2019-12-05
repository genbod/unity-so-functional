using DragonDogStudios.UnitySoFunctional.Functional;
using Sirenix.OdinInspector;
using System;
using UnityEngine.UI;

namespace DragonDogStudios.UnitySoFunctional.Utilities
{
    public class FormattedTextReplacer : SerializedMonoBehaviour
    {
        public Func<System.Object, Option<string>> GetFormattedValueToString;

        public Func<Option<System.Object>> GetValue;

        public Text Text;

        public bool AlwaysUpdate;

        private void OnEnable()
        {
            if (Text != null && GetFormattedValueToString != null
            && GetValue != null)
            {
                Text.text = GetValue().Bind(GetFormattedValueToString)
                .Match(
                    () => "None",
                    (f) => f
                );
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (AlwaysUpdate)
            {
                Text.text = GetValue().Bind(GetFormattedValueToString)
                .Match(
                    () => "None",
                    (f) => f
                );
            }
        }
    }
}