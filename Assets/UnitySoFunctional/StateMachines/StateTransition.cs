namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public class StateTransition
    {
        private readonly string _from;
        private readonly string _to;
        private ITransitionCondition _transitionCondition;

        public string From => _from;

        public string To => _to;

        public bool ConditionMatches() => _transitionCondition.Evaluate();

        public string Expression => _transitionCondition.ToString();

        public StateTransition(string from, string to, ITransitionCondition transitionCondition)
        {
            _from = @from;
            _to = to;
            _transitionCondition = transitionCondition;
        }
    }
}