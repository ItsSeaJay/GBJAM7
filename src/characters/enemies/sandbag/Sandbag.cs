using Godot;
using System;

public class Sandbag : KinematicBody2D
{
    private Vector2 moveDirection;

    private Sprite sprite;
    private AnimationPlayer animationPlayer;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationPlayback;

    public override void _Ready()
    {
        base._Ready();

        sprite = GetNode("Sprite") as Sprite;
        animationPlayer = GetNode("AnimationPlayer") as AnimationPlayer;
        animationTree = GetNode("AnimationTree") as AnimationTree;
        animationPlayback = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
        animationPlayback.Start("idle");
    }

    public void Attack()
    {
        // Do attack
    }
}
