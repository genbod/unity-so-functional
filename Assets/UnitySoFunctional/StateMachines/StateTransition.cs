using System;
using System.Linq.Expressions;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public class StateTransition
    {
        private readonly string _from;
        private readonly string _to;
        private readonly Func<bool> _condition;
        private string _expression;

        public string From => _from;

        public string To => _to;

        public Func<bool> Condition => _condition;

        public string Expression => _expression;

        public StateTransition(string from, string to, Expression<Func<bool>> condition)
        {
            _from = @from;
            _to = to;
            _expression = condition.ToString();
            _condition = condition.Compile();
        }
    }
}