using System;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public class StateTransition
    {
        private readonly string _from;
        private readonly string _to;
        private readonly Func<bool> _condition;

        public string From => _from;

        public string To => _to;
        
        public Func<bool> Condition => _condition;

        public StateTransition(string from, string to, Func<bool> condition)
        {
            _from = @from;
            _to = to;
            _condition = condition;
        }
    }
}