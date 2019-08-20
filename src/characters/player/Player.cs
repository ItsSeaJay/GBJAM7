using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    private float moveSpeed = 64.0f;
    [Export]
    float moveAcceleration = 8.0f;
    [Export]
    float moveFriction = 8.0f;
    private Vector2 moveVelocity = new Vector2();

    [Export]
    public float jumpDuration = 0.4f;
    [Export]
    public float jumpHeight = 48.0f;
    [Export]
    public float jumpTerminalVelocity = 128.0f;
    public float jumpSpeed;
    public float jumpGravity;

    public float fallGravity = 1.0f;

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

        jumpGravity = 2.0f * jumpHeight / Mathf.Pow(jumpDuration / 2.0f, 2);
        jumpSpeed = -Mathf.Sqrt(2.0f * jumpGravity * jumpHeight);



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

        GD.Print(moveVelocity.ToString());
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
                MoveAndSlide(moveVelocity, Vector2.Up);

                if (!IsOnFloor())
                {
                    Transition(State.Airborne);
                }

                if (IsOnWall())
                {
                    moveVelocity.x = 0.0f;
                }
                break;
            case State.Airborne:
                HandleMovement();
                HandleFalling(delta);
                MoveAndSlide(moveVelocity, Vector2.Up);

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
                MoveAndSlide(moveVelocity, Vector2.Up);

                if (Input.IsActionJustPressed("button_a"))
                {
                    moveVelocity.x = jumpSpeed * sprite.GetScale().x;
                    moveVelocity.y = jumpSpeed;
                }

                if (!IsOnWall())
                {
                    Transition(State.Airborne);
                }

                if (IsOnFloor())
                {
                    Transition(State.Grounded);
                }
                break;
        }
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
        if (Input.IsActionPressed("dpad_left"))
        {
            moveVelocity.x = Utils.Approach(moveVelocity.x, -moveSpeed, moveAcceleration);
            sprite.SetScale(new Vector2(-1.0f, 1.0f));
        }
        else if (Input.IsActionPressed("dpad_right"))
        {
            moveVelocity.x = Utils.Approach(moveVelocity.x, moveSpeed, moveAcceleration);
            sprite.SetScale(new Vector2(1.0f, 1.0f));
        }
        else
        {
            moveVelocity.x = Utils.Approach(moveVelocity.x, 0.0f, moveFriction);
        }
    }

    private bool IsMoving()
    {
        return Input.IsActionPressed("dpad_left") || Input.IsActionPressed("dpad_right");
    }

    private void HandleJumping()
    {
        if (Input.IsActionJustPressed("button_a"))
        {
            moveVelocity.y = jumpSpeed;
        }
    }

    private void HandleFalling(float delta)
    {
        // Don't maintain momentum when the player hits a cieling.
        if (IsOnCeiling())
        {
            moveVelocity.y = 0.0f;
        }

        moveVelocity.y += jumpGravity * delta;
    }

    private void HandleAttacking()
    {
        if (Input.IsActionJustPressed("button_b"))
        {
            animationPlayback.Travel("attack");
        }
    }
}
