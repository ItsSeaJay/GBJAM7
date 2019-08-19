using Godot;
using System;

public class EnemySandbag : Enemy
{
    private Vector2 moveDirection;
    [Export]
    public float moveGravity = 8.0f;
    [Export]
    public float moveTerminalVelocity = 256.0f;
    [Export]
    public float moveKnockback = 256.0f;
    [Export]
    public float moveAirResistance = 0.1f;

    private KinematicBody2D body2D;

    private Sprite sprite;
    private AnimationPlayer animationPlayer;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationPlayback;

    public override void _Ready()
    {
        base._Ready();

        body2D = this as KinematicBody2D;

        sprite = GetNode("Sprite") as Sprite;
        animationPlayer = GetNode("AnimationPlayer") as AnimationPlayer;
        animationTree = GetNode("AnimationTree") as AnimationTree;
        animationPlayback = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
        animationPlayback.Start("idle");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        moveDirection.y = Mathf.Min(moveDirection.y + moveGravity, moveTerminalVelocity);
        moveDirection.x = Mathf.Lerp(moveDirection.x, 0.0f, moveAirResistance);

        if (IsOnWall())
        {
            moveDirection.x *= -1.0f;
        }

        if (IsOnCeiling())
        {
            moveDirection.y *= -1.0f;
        }

        body2D.MoveAndSlide
        (
            moveDirection, // Linear velocity
            Vector2.Up // Floor normal
        );
    }

    public override void Attack()
    {
        moveDirection.x += -sprite.GetScale().x * moveKnockback;
        moveDirection.y = -moveKnockback;
    }
}
