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
    public float fallGravity = 8.0f;
    [Export]
    public float fallTerminalVelocity = 256.0f;

    private Sprite sprite;
    private AnimationPlayer animationPlayer;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationPlayback;

    private Area2D hitbox;

    public enum State
    {
        Normal,
        Airborne
    }
    private State state = State.Normal;

    public override void _Ready()
    {
        base._Ready();

        sprite = GetNode("Sprite") as Sprite;
        animationPlayer = GetNode("AnimationPlayer") as AnimationPlayer;
        animationTree = GetNode("AnimationTree") as AnimationTree;
        animationPlayback = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
        animationPlayback.Start("stand");

        hitbox = GetNode("Sprite/Hitbox") as Area2D;
        hitbox.Connect("area_entered", this, "_On_Hitbox_AreaEntered");
    }

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
                HandleAttacking();

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

    private void _On_Hitbox_AreaEntered(Area2D area)
    {
        Node owner = area.GetOwner();

        if (owner is Enemy)
        {
            Enemy enemy = owner as Enemy;
            enemy.Attack();
        }
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
            sprite.SetScale(new Vector2(-1.0f, 1.0f));
        }
        else if (Input.IsActionPressed("move_right"))
        {
            inputAxis += 1.0f;
            sprite.SetScale(new Vector2(1.0f, 1.0f));
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

    private void HandleAttacking()
    {
        if (Input.IsActionJustPressed("move_attack"))
        {
            animationPlayback.Travel("attack");
        }
    }
}
