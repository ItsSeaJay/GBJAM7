using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    private float moveSpeed = 128.0f;
    [Export]
    float moveAcceleration = 16.0f;
    [Export]
    float moveFriction = 8.0f;
    private Vector2 moveVelocity = new Vector2();

    [Export]
    public float jumpDuration = 0.64f;
    [Export]
    public float jumpHeight = 48.0f;
    public float jumpSpeed;
    public float jumpGravity;

    [Export]
    public float fallDuration = 0.48f;
    public float fallGravity;

    [Export]
    public float wallSlideTerminalVelocity = 64.0f;
    [Export]
    public float airborneTerminalVelocity = 128.0f;
    private float terminalVelocity;

    [Export]
    public float climbSpeed = 64.0f;

    private Sprite sprite;
    private AnimationPlayer animationPlayer;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationPlayback;

    private Area2D hitbox;

    public enum State
    {
        Grounded,
        Airborne,
        WallSlide,
        Climbing,
        Attack
    }
    private State state = State.Grounded;

    public override void _Ready()
    {
        base._Ready();

        jumpGravity = 2.0f * jumpHeight / Mathf.Pow(jumpDuration / 2.0f, 2);
        fallGravity = 2.0f * jumpHeight / Mathf.Pow(fallDuration / 2.0f, 2);
        jumpSpeed = -Mathf.Sqrt(2.0f * jumpGravity * jumpHeight);
        terminalVelocity = airborneTerminalVelocity;

        sprite = GetNode("Sprite") as Sprite;
        animationPlayer = GetNode("AnimationPlayer") as AnimationPlayer;
        animationTree = GetNode("AnimationTree") as AnimationTree;
        animationTree.SetActive(true);
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

                if (moveVelocity.x != 0.0f)
                {
                    AnimationSafeTravel("run");
                }
                break;
            case State.Airborne:
                HandleMovement();
                HandleFalling(delta);
                MoveAndSlide(moveVelocity, Vector2.Up);

                if (moveVelocity.y < 0.0f)
                {
                    AnimationSafeTravel("jump");
                }
                else
                {
                    AnimationSafeTravel("fall");
                }

                if (IsOnWall())
                {
                    Transition(State.WallSlide);
                }

                if (IsOnFloor())
                {
                    Transition(State.Grounded);
                }
                break;
            case State.WallSlide:
                HandleMovement();
                HandleFalling(delta);
                HandleWallJumping();
                MoveAndSlide(moveVelocity, Vector2.Up);

                if (!IsOnWall())
                {
                    Transition(State.Airborne);
                }

                if (IsOnFloor())
                {
                    Transition(State.Grounded);
                }

                if (Input.IsActionPressed("button_b"))
                {
                    Transition(State.Climbing);
                }
                break;
            case State.Climbing:
                MoveAndSlide(moveVelocity, Vector2.Up);

                if (!Input.IsActionPressed("button_b"))
                {
                    Transition(State.WallSlide);
                }

                var spaceState = GetWorld2d().DirectSpaceState;
                var facing = new Vector2(sprite.GetScale().x, 0.0f);
                var result = spaceState.IntersectRay
                (
                    GlobalPosition, // From
                    GlobalPosition + facing, // To
                    new Godot.Collections.Array()
                );
                GD.Print(result["collider"]);

                if (IsOnFloor())
                {
                    Transition(State.Grounded);
                }

                if (Input.IsActionPressed("dpad_up"))
                {
                    moveVelocity.y = -climbSpeed;
                }
                else if (Input.IsActionPressed("dpad_down"))
                {
                    moveVelocity.y = climbSpeed;
                }
                else
                {
                    moveVelocity.y = 0.0f;
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
            case State.WallSlide:
                break;
            case State.Climbing:
                // Cancel any previous momentum when climbing walls
                moveVelocity = Vector2.Zero;
                break;
        }

        if (nextState == State.WallSlide)
        {
            terminalVelocity = wallSlideTerminalVelocity;
        }
        else
        {
            terminalVelocity = airborneTerminalVelocity;
        }

        state = nextState;
    }

    private void HandleMovement()
    {
        if (Input.IsActionPressed("dpad_left"))
        {
            moveVelocity.x = Utils.Approach(moveVelocity.x, -moveSpeed, moveAcceleration);
            sprite.SetScale(new Vector2(-1.0f, 1.0f));

            if (animationPlayback.IsPlaying())
                animationPlayback.Travel("run");
        }
        else if (Input.IsActionPressed("dpad_right"))
        {
            moveVelocity.x = Utils.Approach(moveVelocity.x, moveSpeed, moveAcceleration);
            sprite.SetScale(new Vector2(1.0f, 1.0f));

            if (animationPlayback.IsPlaying())
                animationPlayback.Travel("run");
        }
        else
        {
            moveVelocity.x = Utils.Approach(moveVelocity.x, 0.0f, moveFriction);

            if (animationPlayback.IsPlaying())
                animationPlayback.Travel("stand");
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

    private void HandleWallJumping()
    {
        if (Input.IsActionJustPressed("button_a"))
        {
            moveVelocity.x = jumpSpeed * sprite.GetScale().x;
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

        if (moveVelocity.y < 0.0f)
        {
            moveVelocity.y = Utils.Approach(moveVelocity.y, terminalVelocity, jumpGravity * delta);
        }
        else
        {
            moveVelocity.y = Utils.Approach(moveVelocity.y, terminalVelocity, fallGravity * delta);
        }
    }

    private void HandleAttacking()
    {
        if (Input.IsActionJustPressed("button_b"))
        {
            animationPlayback.Travel("attack");
        }
    }

    private void AnimationSafeTravel(string name)
    {
        if (animationPlayback.IsPlaying())
        {
            animationPlayback.Travel(name);
        }
    }
}
