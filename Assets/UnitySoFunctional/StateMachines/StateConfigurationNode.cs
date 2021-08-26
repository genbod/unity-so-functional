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
        
        [SerializeField, ShowIf("_anyTransition"), ReadOnly]
        private TransitionConfiguration _anyTransition;
        
        [SerializeField, ShowIf("_pushTransition"), ReadOnly]
        private TransitionConfiguration _pushTransition;

        [SerializeField, ShowIf("_popTransition"), ReadOnly]
        private TransitionConfiguration _popTransition;

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

        public StateConfigurationNode SetPopTransition(string transitionCondition)
        {
            _popTransition = TransitionConfiguration.Create(transitionCondition);
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

            if (_popTransition != null)
            {
                stateMachine.AddPopTransition(
                    state,
                    transitionConditionCreator.Invoke(_popTransition.Condition));
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
        
        public void AddPushTransition(string condition)
        {
            // Check to see if transition already exists
            if (_pushTransition != null)
            {
                return;
            }
            Undo.RecordObject(this, "Set Push Transition");
            _pushTransition = TransitionConfiguration.Create(
                condition,
                ID,
                null);
            Undo.RegisterCreatedObjectUndo(_pushTransition, "Created Push Transition");
        }

        public void AddPopTransition(string condition)
        {
            // Check to see if transition already exists
            if (_popTransition != null)
            {
                return;
            }
            Undo.RecordObject(this, "Set Pop Transition");
            _popTransition = TransitionConfiguration.Create(
                condition,
                ID,
                null);
            Undo.RegisterCreatedObjectUndo(_popTransition, "Created Pop Transition");
        }

        public void ClearTransitions()
        {
            foreach (var transition in new List<TransitionConfiguration>(_transitions))
            {
                DeleteTransition(transition);
            }
            DeleteTransition(_anyTransition);
            DeleteTransition(_pushTransition);
            DeleteTransition(_popTransition);
        }

        public void DeleteTransition(TransitionConfiguration selectedTransition)
        {
            var foundTransition = false;
            if (_transitions.Contains(selectedTransition))
            {
                Undo.RecordObject(this, "Deleted Transition");
                _transitions.Remove(selectedTransition);
                foundTransition = true;
            }
            else if (_anyTransition != null
                &&_anyTransition == selectedTransition)
            {
                Undo.RecordObject(this, "Deleted Transition");
                _anyTransition = null;
                foundTransition = true;
            }
            else if (_pushTransition != null
                && _pushTransition == selectedTransition)
            {
                Undo.RecordObject(this, "Deleted Transition");
                _pushTransition = null;
                foundTransition = true;
            }
            else if (_popTransition != null
                && _popTransition == selectedTransition)
            {
                Undo.RecordObject(this, "Deleted Transition");
                _popTransition = null;
                foundTransition = true;
            }

            if (foundTransition)
            {
                Undo.DestroyObjectImmediate(selectedTransition);
            }
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
                SetTransitionAssetName(transitionConfiguration, leftSide, rightSide);
            }

            if (_anyTransition != null)
            {
                SetTransitionAssetName(_anyTransition, "Any", name);
            }
            
            if (_pushTransition != null)
            {
                SetTransitionAssetName(_pushTransition, "Push", name);
            }

            if (_popTransition != null)
            {
                SetTransitionAssetName(_popTransition, name, "Pop");
            }
        }

        private void SetTransitionAssetName(TransitionConfiguration transition, string leftSide, string rightSide)
        {
            transition.name = $"{leftSide} -> {rightSide}";
            if (AssetDatabase.GetAssetPath(transition) == "")
            {
                AssetDatabase.AddObjectToAsset(transition, this);
            }
        }

        public void DrawConnections(
            Func<Guid?, StateConfigurationNode> getNode,
            Action<TransitionConfiguration> drawConnection,
            Vector2 anyNodeCenter,
            Vector2 pushNodeCenter,
            Vector2 popNodeCenter)
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
            if (_popTransition != null)
            {
                _popTransition.SetStartPosition(startPosition);
                _popTransition.SetEndPosition(
                    new Vector2(_rect.center.x, popNodeCenter.y));
                drawConnection(_popTransition);
            }

            var endPosition = _rect.center;
            if (_anyTransition != null)
            {
                _anyTransition.SetStartPosition(
                    new Vector2(anyNodeCenter.x, _rect.center.y));
                _anyTransition.SetEndPosition(endPosition);
                drawConnection(_anyTransition);
            }

            if (_pushTransition != null)
            {
                _pushTransition.SetStartPosition(
                    new Vector2(_rect.center.x, pushNodeCenter.y));
                _pushTransition.SetEndPosition(endPosition);
                drawConnection(_pushTransition);
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
            if (_pushTransition?.Arrow.Contains(mousePoint) == true) return _pushTransition;
            if (_popTransition?.Arrow.Contains(mousePoint) == true) return _popTransition;

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