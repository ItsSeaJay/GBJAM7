using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    private float moveSpeed = 128.0f;
    private Vector2 moveDirection = new Vector2();
    private Vector2 moveVelocity = new Vector2();

    [Export]
    public float jumpDuration = 4.0f;
    [Export]
    public float jumpHeight = 128.0f;
    [Export]
    public float jumpTerminalVelocity = 128.0f;
    public float jumpSpeed;
    public float jumpGravity;

    public float wallSlideSpeed = 1.0f;

    [Export]
    public float climbStaminaMax = 100.0f;
    public float climbStamina;

    private Sprite sprite;
    private AnimationPlayer animationPlayer;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationPlayback;

    private Area2D hitbox;

    public enum State
    {
        Grounded,
        Airborne,
        Sliding,
        Climbing,
        Attack
    }
    private State state = State.Grounded;

    public override void _Ready()
    {
        base._Ready();

        jumpGravity = 2 * jumpHeight / Mathf.Pow(jumpDuration, 2);
        jumpSpeed = -Mathf.Sqrt(2 * jumpGravity * jumpHeight);

        GD.Print(jumpSpeed, " ", jumpGravity);

        sprite = GetNode("Sprite") as Sprite;
        animationPlayer = GetNode("AnimationPlayer") as AnimationPlayer;
        animationTree = GetNode("AnimationTree") as AnimationTree;
        animationPlayback = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
        animationPlayback.Start("stand");

        hitbox = GetNode("Sprite/Hitbox") as Area2D;
        hitbox.Connect("area_entered", this, "_On_Hitbox_AreaEntered");

        climbStamina = climbStaminaMax;
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
            case State.Grounded:
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
                HandleFalling(delta);

                if (IsOnWall())
                {
                    Transition(State.Sliding);
                }

                if (IsOnFloor())
                {
                    Transition(State.Grounded);
                }
                break;
            case State.Sliding:
                HandleMovement();
                HandleFalling(delta);

                if (Input.IsActionJustPressed("button_a"))
                {
                    moveDirection.x = jumpSpeed * -sprite.GetScale().x;
                    moveDirection.y = -jumpSpeed;
                }

                if (!IsOnWall())
                {
                    Transition(State.Airborne);
                }
                break;
        }

        moveVelocity = MoveAndSlide
        (
            moveDirection, // Linear velocity
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
            case State.Grounded:
                break;
            case State.Airborne:
                break;
            case State.Attack:
                break;
            case State.Sliding:
                break;
            case State.Climbing:
                break;
        }
        
        state = nextState;
    }

    private void HandleMovement()
    {
        float inputAxis = 0.0f;

        if (Input.IsActionPressed("dpad_left"))
        {
            inputAxis -= 1.0f;
            sprite.SetScale(new Vector2(-1.0f, 1.0f));
        }
        else if (Input.IsActionPressed("dpad_right"))
        {
            inputAxis += 1.0f;
            sprite.SetScale(new Vector2(1.0f, 1.0f));
        }

        moveDirection.x = inputAxis * moveSpeed;
    }

    private void HandleJumping()
    {
        if (Input.IsActionJustPressed("button_a"))
        {
            moveDirection.y = jumpSpeed;
        }
    }

    private void HandleFalling(float delta)
    {
        // Don't maintain momentum when the player hits a cieling.
        if (IsOnCeiling())
        {
            moveDirection.y = 0.0f;
        }

        moveDirection.y += jumpGravity * delta;
    }

    private void HandleAttacking()
    {
        if (Input.IsActionJustPressed("button_b"))
        {
            animationPlayback.Travel("attack");
        }
    }
}
