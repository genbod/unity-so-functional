using System;
using System.Collections.Generic;
using System.Linq;
using DragonDogStudios.UnitySoFunctional.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class StateConfigurationNode : SerializedScriptableObject
    {
        [HideInInspector, SerializeField] private string _id = Guid.NewGuid().ToString();
        [SerializeField, ValueDropdown("GetStateNames")]
        private string _name;

        [HideInInspector, SerializeField] private Vector2 _pos;
        [SerializeField, ReadOnly] private List<TransitionConfiguration> _transitions = new List<TransitionConfiguration>();
        [SerializeField, ShowIf("_anyTransition"), ReadOnly] private TransitionConfiguration _anyTransition;
        [SerializeField] private TransitionConfiguration _pushTransition = new TransitionConfiguration();
        [SerializeField] private List<TransitionConfiguration> _popTransition = new List<TransitionConfiguration>();

        private Rect _rect = new Rect(0, 0, 200, 50);
        private readonly UnityEvent _onEnter = new UnityEvent();
        private readonly UnityEvent _onExit = new UnityEvent();

        private readonly UnityEvent _onTick = new UnityEvent();

        private static List<string> _stateNames;

        public Rect Rect
        {
            get
            {
                _rect.position = _pos;
                return _rect;
            }
        }

        public IEnumerable<TransitionConfiguration> Transitions => _transitions;

        public Guid ID => Guid.Parse(_id);

        public StateConfigurationNode OnEnter(UnityAction enterAction)
        {
            _onEnter.AddListener(enterAction);
            return this;
        }

        public StateConfigurationNode OnExit(UnityAction exitAction)
        {
            _onExit.AddListener(exitAction);
            return this;
        }

        public StateConfigurationNode OnTick(UnityAction tickAction)
        {
            _onTick.AddListener(tickAction);
            return this;
        }

        public StateConfigurationNode Transition(string stateName, string transitionCondition)
        {
            var transitionConfiguration = TransitionConfiguration.Create(
                transitionCondition,
                stateName);
            _transitions.Add(transitionConfiguration);
            return this;
        }

        public StateConfigurationNode SetAnyTransition(string transitionCondition)
        {
            _anyTransition = TransitionConfiguration.Create(transitionCondition);
            return this;
        }

        public StateConfigurationNode SetPushTransition(string transitionCondition)
        {
            _pushTransition = TransitionConfiguration.Create(transitionCondition);
            return this;
        }

        public StateConfigurationNode PopTransition(string transitionCondition)
        {
            _popTransition.Add(
                TransitionConfiguration.Create(transitionCondition));
            return this;
        }

        public void Build(
            StateMachine stateMachine,
            IState state,
            Func<string, ITransitionCondition> transitionConditionCreator)
        {
            state.SetTickActions(_onTick);
            state.SetOnEnterActions(_onEnter);
            state.SetOnExitActions(_onExit);
            foreach (var transition in _transitions)
            {
                stateMachine.AddTransition(
                    state,
                    transition.ToStateName,
                    transitionConditionCreator.Invoke(transition.Condition));
            }

            if (_anyTransition != null)
            {
                stateMachine.AddAnyTransition(
                    state,
                    transitionConditionCreator.Invoke(_anyTransition.Condition));
            }

            if (_pushTransition != null)
            {
                stateMachine.AddPushTransition(
                    state,
                    transitionConditionCreator.Invoke(_pushTransition.Condition));
            }

            foreach (var transition in _popTransition)
            {
                stateMachine.AddPopTransition(
                    state,
                    transitionConditionCreator.Invoke(transition.Condition));
            }
        }

#if UNITY_EDITOR

        public void SetName(string newName)
        {
            _name = name = newName;
        }

        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move State Node");
            _pos = newPosition;
            EditorUtility.SetDirty(this);
        }

        public void AddTransition(Guid toStateID, string condition)
        {
            // Check to see if transition already exists
            if (_transitions.Exists(x => x.ToStateID == toStateID))
            {
                return;
            }
            Undo.RecordObject(this, "Added Transition");
            _transitions.Add(
                TransitionConfiguration.Create(
                    condition,
                    ID,
                    toStateID));
            Undo.RegisterCreatedObjectUndo(Transitions.Last(), "Created Transition");
        }

        public void AddAnyTransition(string condition)
        {
            // Check to see if transition already exists
            if (_anyTransition != null)
            {
                return;
            }
            Undo.RecordObject(this, "Set Any Transition");
            _anyTransition = TransitionConfiguration.Create(
                    condition,
                    ID,
                    null);
            Undo.RegisterCreatedObjectUndo(_anyTransition, "Created Any Transition");
        }

        public void ClearTransitions()
        {
            foreach (var transition in new List<TransitionConfiguration>(_transitions))
            {
                DeleteTransition(transition);
            }
            DeleteTransition(_anyTransition);
        }

        public void DeleteTransition(TransitionConfiguration selectedTransition)
        {
            Undo.RecordObject(this, "Deleted Transition");
            if (_transitions.Contains(selectedTransition))
            {
                _transitions.Remove(selectedTransition);
            }
            else if (_anyTransition == selectedTransition)
            {
                _anyTransition = null;
            }
            Undo.DestroyObjectImmediate(selectedTransition);
        }

        public void UpdateTransitionNames(Func<Guid?, string> getStateName)
        {
            foreach (var transitionConfiguration in _transitions)
            {
                // Update Name
                if (getStateName(transitionConfiguration.ToStateID) != null)
                {
                    transitionConfiguration.SetToStateName(
                        getStateName(transitionConfiguration.ToStateID));
                }
                var stateName = transitionConfiguration.ToStateName;
                var leftSide = stateName != null ? name : "";
                var rightSide = stateName ?? name;
                transitionConfiguration.name = $"{leftSide} -> {rightSide}";
                if (AssetDatabase.GetAssetPath(transitionConfiguration) == "")
                {
                    AssetDatabase.AddObjectToAsset(transitionConfiguration, this);
                }
            }

            if (_anyTransition != null)
            {
                var leftSide = "Any";
                var rightSide = name;
                _anyTransition.name = $"{leftSide} -> {rightSide}";
                if (AssetDatabase.GetAssetPath(_anyTransition) == "")
                {
                    AssetDatabase.AddObjectToAsset(_anyTransition, this);
                }
            }
        }

        public void DrawConnections(
            Func<Guid?, StateConfigurationNode> getNode,
            Action<TransitionConfiguration> drawConnection,
            Vector2 anyNodeCenter)
        {
            var startPosition = _rect.center;
            foreach (var transition in _transitions)
            {
                var transitionNode = getNode(transition.ToStateID);
                if (transitionNode == null) continue;
                transition.SetStartPosition(startPosition);
                transition.SetEndPosition(transitionNode.Rect.center);
                drawConnection(transition);
            }

            var endPosition = _rect.center;
            if (_anyTransition != null)
            {
                startPosition = new Vector2(anyNodeCenter.x, _rect.center.y);
                _anyTransition.SetStartPosition(startPosition);
                _anyTransition.SetEndPosition(endPosition);
                drawConnection(_anyTransition);
            }
        }

        public TransitionConfiguration GetTransitionAtPoint(Vector2 mousePoint)
        {
            foreach (var transition in _transitions)
            {
                if (!transition.Arrow.Contains(mousePoint)) continue;
                return transition;
            }

            if (_anyTransition?.Arrow.Contains(mousePoint) == true)return _anyTransition;

            return null;
        }

        private void OnValidate()
        {
            name = _name;
        }

        private static IEnumerable<string> GetStateNames()
        {
            if (_stateNames == null)
            {
                UpdateStateNames();
            }

            return _stateNames;
        }

        [Button]
        private static void UpdateStateNames()
        {
            _stateNames = new List<string>();
            _stateNames.AddRange(
                EditorHelpers
                    .GetFieldsFromInterfaceImplementations(typeof(IStateMachineNames)));
        }

        private string GetStateName(ObjectIdentifier assetID)
        {
            var asset = ObjectIdentifier.ToObject(assetID) as StateConfigurationNode;
            return asset?.name;
        }

#endif
    }
}