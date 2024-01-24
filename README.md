# C# State Machine

Finite State Machine for game applications, with elegant syntax inspired by [Stateless](https://github.com/dotnet-state-machine/stateless?tab=readme-ov-file).

## Usage
``` csharp

private StateMachine<State> sm = new StateMachine<State>(State.FirstState);

sm.Configure(State.FirstState)
    .SubstateOf(State.BaseState)
    .OnEntry(() => { /* Entered */ })
    .OnProcess(() => { /* Processing... */ })
    .OnExit(() => { /* Exited */ })
    .AddTransition(State.OtherState, () => firstCondition())
    .AddTransition(State.AnotherState, () => secondCondition());
```

## Examples

### Character controller
``` csharp
private enum State { Idle, BaseMove, Walk, Jump, Fall }
private StateMachine<State> sm = new StateMachine<State>(State.Idle);

// Called on first frame from engine of choice
private void Enter()
{
    InitStates();
}

// Called on frame update from engine of choice
private void Process()
{
      sm.Process();
}

private void InitStates()
{
    sm.Configure(State.Idle)
        .OnEntry(() => {
            // Set Idle animation
        })
        .AddTransition(State.Walk, () => Input.Move() != 0f)
        .AddTransition(State.Jump, () => Input.Jump());

    sm.Configure(State.BaseMove)
        .OnProcess(() => {
            // Move character based on player input
        });

    sm.Configure(State.Walk)
        .SubstateOf(State.BaseMove)
        .OnEntry(() => {
            // Set Walk animation
        })
        .AddTransition(State.Idle, () => Input.Move() == 0f)
        .AddTransition(State.Jump, () => Input.Jump())
        .AddTransition(State.Fall, () => !IsGrounded());

    sm.Configure(State.Jump)
        .SubstateOf(State.BaseMove)
        .OnEntry(() => {
            // Set Jump animation
        })
        .AddTransition(State.Fall, () => IsFalling());

    sm.Configure(State.Fall)
        .SubstateOf(State.BaseMove)
        .OnEntry(() => {
            // Set Fall animation
        })
        .AddTransition(State.Idle, () => IsGrounded() && Input.Move() == 0f)
        .AddTransition(State.Walk, () => IsGrounded() && Input.Move() != 0f);
}
```
