using Godot;
using System;

public partial class ConveyorMoveComponent : Node
{

    // THIS CURRENTLY DOESNT WORK EVEN THOUGH IT SHOULD, USE CONVEYORWORKAROUNDCOMPONENT INSTEAD UNTIL ITS FIXED

    private StaticBody2D Parent;
    private CollisionShape2D ParentCollider;
    [Export] private float ConveyorSpeed;
    [Export] private bool Right; //true = right false = left
    private float ConveyorDirection;

    public override void _Ready()
    {
        Parent = GetParent() as StaticBody2D;
        if (Right == true){
            ConveyorDirection = 1f;
        } else
            ConveyorDirection = -1f;

        var constVel = Parent.ConstantLinearVelocity;
        constVel.X = (ConveyorDirection * ConveyorSpeed);
        Parent.ConstantLinearVelocity = constVel;
        // Parent.ConstantLinearVelocity = new Vector2(ConveyorDirection * ConveyorSpeed, 0.0f);
    }
}
