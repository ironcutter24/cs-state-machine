using System;
using System.Collections.Generic;

namespace Utilities.Patterns
{

    public partial class StateMachine<TState>
    {
        public class State
        {
            internal Dictionary<State, Func<bool>> transitions = new Dictionary<State, Func<bool>>();
            private State parent;

            public StateMachine<TState> OwnerMachine { get; private set; }
            public TState Key { get; private set; }

            public State(StateMachine<TState> owner, TState stateKey)
            {
                OwnerMachine = owner;
                Key = stateKey;
            }

            #region State Configuration

            public State SubstateOf(TState parentState)
            {
                parent = OwnerMachine.Configure(parentState);
                return this;
            }

            public State AddTransition(TState nextState, Func<bool> condition)
            {
                State nextStateInstance = OwnerMachine.Configure(nextState);
                if (transitions.TryGetValue(nextStateInstance, out var existingCondition))
                {
                    transitions[nextStateInstance] = () => existingCondition() || condition();
                }
                else
                {
                    transitions.Add(nextStateInstance, condition);
                }
                return this;
            }

            #endregion

            #region State Actions

            private Action onEntryCallback;
            public State OnEntry(Action entryAction)
            {
                onEntryCallback = entryAction;
                return this;
            }

            private Action onProcessCallback;
            public State OnProcess(Action processAction)
            {
                onProcessCallback = processAction;
                return this;
            }

            private Action onExitCallback;
            public State OnExit(Action exitAction)
            {
                onExitCallback = exitAction;
                return this;
            }

            #endregion

            #region Perform Actions

            internal void PerformOnEntry(int depth)
            {
                if (depth > 1)
                {
                    parent?.PerformOnEntry(depth - 1);
                }
                onEntryCallback?.Invoke();
            }

            internal void PerformOnProcess()
            {
                parent?.PerformOnProcess();
                onProcessCallback?.Invoke();
            }

            internal State PerformTransitionCheck()
            {
                foreach (var pair in transitions)
                {
                    if (pair.Value.Invoke()) return pair.Key;
                }
                return parent?.PerformTransitionCheck();
            }

            internal void PerformOnExit(int depth)
            {
                onExitCallback?.Invoke();
                if (depth > 1)
                {
                    parent?.PerformOnExit(depth - 1);
                }
            }

            #endregion

            #region Helpers

            public static bool HaveSharedParent(State a, State b, out (int from, int to) depth)
            {
                State aParent = a;
                State bParent = b;
                depth = (-1, -1);

                while (aParent != null)
                {
                    bParent = b;
                    depth.to = 0;
                    depth.from++;

                    while (bParent != null)
                    {
                        if (aParent == bParent)
                        {
                            return true; // Found a common ancestor
                        }
                        bParent = bParent.parent;
                        depth.to++;
                    }
                    aParent = aParent.parent;
                }

                // Reset depth values to their maximum if no common ancestor is found
                depth = (int.MaxValue, int.MaxValue);
                return false;
            }

            internal string FormatParents()
            {
                return ((parent != null) ? $"{parent.FormatParents()} -> " : "") + Key.ToString();
            }

            #endregion

        }
    }

}
