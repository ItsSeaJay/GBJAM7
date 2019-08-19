using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    private float movementSpeed = 128.0f;
    private Vector2 movementDirection = new Vector2();
    private Vector2 movementVelocity = new Vector2(); 

    [Export]
    public float jumpSpeed = 256.0f;
    [Export]
    public float jumpCoyoteTime = 1.0f;
    private float coyoteTime = 0.0f;

    [Export]
    public float fallGravity = 8.0f;
    [Export]
    public float fallTerminalVelocity = 256.0f;

    public enum State
    {
        Normal,
        Airborne
    }
    private State state = State.Normal;

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        switch (state)
        {
            case State.Normal:
                HandleMovement();
                HandleJumping();

                if (!IsOnFloor())
                {
                    Transition(State.Airborne);
                }
                break;
            case State.Airborne:
                HandleMovement();
                HandleFalling();

                if (IsOnFloor())
                {
                    Transition(State.Normal);
                }
                break;
        }

        movementVelocity = MoveAndSlide
        (
            movementDirection, // Linear velocity
            Vector2.Up // Floor normal vector
        );
    }

    public void Transition(State nextState)
    {
        switch (nextState)
        {
            default:
                break;
        }
        
        state = nextState;
    }

    private void HandleMovement()
    {
        float inputAxis = 0.0f;

        if (Input.IsActionPressed("move_left"))
        {
            inputAxis -= 1.0f;
        }
        else if (Input.IsActionPressed("move_right"))
        {
            inputAxis += 1.0f;
        }

        movementDirection.x = inputAxis * movementSpeed;
    }

    private void HandleJumping()
    {
        if (Input.IsActionJustPressed("move_jump"))
        {
            movementDirection.y = -jumpSpeed;
        }
    }

    private void HandleFalling()
    {
        if (IsOnCeiling())
        {
            movementDirection.y = 0.0f;
        }

        movementDirection.y = Mathf.Min(movementDirection.y + fallGravity, fallTerminalVelocity);
    }
}
