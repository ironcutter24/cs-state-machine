using System;
using System.Collections.Generic;

namespace Utilities.Patterns
{

    public partial class StateMachine<TState>
    {
        public event Action<string> StateChanged;

        private enum Event { Entry, Process, Exit }
        private Event stateEvent = Event.Entry;
        private Dictionary<TState, State> states = new Dictionary<TState, State>();

        private State CurrentState { get; set; }
        private State NextState { get; set; }


        public StateMachine(TState entryState)
        {
            CurrentState = Configure(entryState);
        }

        public State Configure(TState stateKey)
        {
            if (states.TryGetValue(stateKey, out var state))
            {
                return state;
            }
            else
            {
                state = new State(this, stateKey);
                states.Add(stateKey, state);
                return state;
            }
        }

        (int from, int to) depth = (int.MaxValue, int.MaxValue);
        public void Process()
        {
            if (stateEvent == Event.Entry)
            {
                StateChanged?.Invoke(CurrentState.FormatParents());
                CurrentState.PerformOnEntry(depth.to);
            }

            CurrentState.PerformOnProcess();

            NextState = CurrentState.PerformTransitionCheck();
            if (NextState != null)
            {
                State.HaveSharedParent(CurrentState, NextState, out depth);
                CurrentState.PerformOnExit(depth.from);
                stateEvent = Event.Exit;
                CurrentState = NextState;
            }

            if (stateEvent == Event.Entry) { stateEvent = Event.Process; }
            if (stateEvent == Event.Exit) { stateEvent = Event.Entry; }
        }

        public TState GetCurrentState()
        {
            return CurrentState.Key;
        }
    }

}
