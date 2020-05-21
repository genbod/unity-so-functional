using DragonDogStudios.UnitySoFunctional.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    [CreateAssetMenu]
    public class DialData : ScriptableObject
    {
        [InlineEditor]
        [MakeLocalMenu]
        public BoolVariable Enabled;

        [InlineEditor]
        [MakeLocalMenu]
        public StringVariable Name;

        [InlineEditor]
        [MakeLocalMenu]
        public IntVariable Count;

        [InlineEditor]
        [MakeLocalMenu]
        public FloatVariable FillAmount;

        [InlineEditor]
        [MakeLocalMenu]
        public IntVariable Index;

        [InlineEditor]
        public StringVariableChangedEventListener StringListener;

        private void Awake()
        {
            if (Name == null)
            {
                Name = ScriptableObject.CreateInstance<StringVariable>();
            }
            if (Count == null)
            {
                Count = ScriptableObject.CreateInstance<IntVariable>();
            }
            if (FillAmount == null)
            {
                FillAmount = ScriptableObject.CreateInstance<FloatVariable>();
            }
            if (Index == null)
            {
                Index = ScriptableObject.CreateInstance<IntVariable>();
            }
            if (Enabled == null)
            {
                Enabled = ScriptableObject.CreateInstance<BoolVariable>();
            }
        }
    }
}