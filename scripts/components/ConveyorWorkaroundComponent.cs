using Godot;
using System;
using System.Threading;

public partial class ConveyorWorkaroundComponent : Node
{
    private StaticBody2D Parent;
    private Area2D ConveyorArea;
    [Export] private float ConveyorSpeed;
    [Export] private bool Right; //true = right false = left
    private float ConveyorDirection;
    private Godot.Collections.Array<Node2D> Overlapping;
    public override void _Ready()
    {
        Parent = GetParent() as StaticBody2D;
        ConveyorArea = Parent.GetNode<Area2D>("WorkaroundArea");
        if (Right == true){
            ConveyorDirection = 1f;
        } else
            ConveyorDirection = -1f;
    }

    public override void _Process(double delta)
    {
        MoveBodies();
    }

    private void MoveBodies(){
        Overlapping = ConveyorArea.GetOverlappingBodies();
        foreach(Node2D body in Overlapping){
            if (body is RigidBody2D){
                var rigidBody = body as RigidBody2D;
                var rigidVel = rigidBody.LinearVelocity;
                rigidVel.X = (ConveyorDirection * ConveyorSpeed);
                rigidBody.LinearVelocity = rigidVel;
            }
        }
    }
}
