using Godot;
using System;

public abstract class Enemy : KinematicBody2D
{
    public abstract void Attack();
}