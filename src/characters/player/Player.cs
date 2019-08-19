using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    public float moveSpeed = 16.0f;
    private Vector2 moveDirection = new Vector2(); 

    [Export]
    public float jumpSpeed = 16.0f;

    [Export]
    public float fallGravity = 1.0f;
    [Export]
    public float fallTerminalVelocity = 32.0f;

    enum State
    {
        Normal,
        Airborne
    }
    private State state = State.Normal;

    public override void _Ready()
    {
        base._Ready();

        GD.Print("Hello there!");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        switch (state)
        {
            case State.Normal:
                float inputAxis = 0.0f;

                if (Input.IsActionPressed("move_left"))
                {
                    inputAxis -= 1.0f;
                }
                else if (Input.IsActionPressed("move_right"))
                {
                    inputAxis += 1.0f;
                }

                if (Input.IsActionJustPressed("move_jump"))
                {
                    moveDirection.y -= jumpSpeed;
                }

                moveDirection.x = inputAxis;
                moveDirection.y = Mathf.Min(moveDirection.y + fallGravity, fallTerminalVelocity);

                var velocity = MoveAndSlide(moveDirection * moveSpeed);
                break;
        }
    }
}
