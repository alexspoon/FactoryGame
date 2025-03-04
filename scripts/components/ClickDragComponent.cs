using Godot;
using System;

public partial class ClickDragComponent : Node
{
    private Node2D Main;
    private StaticBody2D Walls;
    private Node2D Parent;
    private Camera2D Camera;
    private RigidBody2D GrabbedObject;
    private bool ObjectHeld;
    [Export] private float DragSpeed;

    public override void _Ready(){
        Main = GetTree().GetRoot().GetNode<Node2D>("Main");
        Walls = Main.GetNode<StaticBody2D>("Start/Walls");
        Camera = Main.GetNode<Camera2D>("Camera");
        Parent = GetParent() as Node2D;
        ObjectHeld = false;
    }

    public override void _Process(double delta){
        HandleObject(delta);
        ClickDrag();
    }

    private void ClickDrag(){
        var worldState = Parent.GetWorld2D().DirectSpaceState;
        var pointParams = new PhysicsPointQueryParameters2D
        {
            CollideWithBodies = true,
            Position = Parent.GlobalPosition
        };
        var pointResults = worldState.IntersectPoint(pointParams);

        if (pointResults.Count == 0)
            return;

        var hoveredObject = pointResults[0]["collider"];

        if (hoveredObject.Obj is not RigidBody2D)
            return;

        if (Input.IsActionJustPressed("LeftClick") && !ObjectHeld){
            GrabbedObject = hoveredObject.Obj as RigidBody2D;
            ObjectHeld = true;
        }
    }

    private void HandleObject(double delta){
        if (Input.IsActionJustReleased("LeftClick") && ObjectHeld){
            GrabbedObject.GravityScale = 1f;
            GrabbedObject = null;
            ObjectHeld = false;
        }
        
        if (!ObjectHeld)
            return;

        var grabTarget = (Parent.GlobalPosition - GrabbedObject.GlobalPosition);
        var objectVelocity = GrabbedObject.LinearVelocity;
        var targetVelocity = grabTarget * DragSpeed;
        var targetVelocityChange = (objectVelocity - targetVelocity);
        var targetAcceleration = targetVelocityChange;
        GrabbedObject.LinearVelocity = -targetAcceleration;
        GrabbedObject.GravityScale = 0f;
    }
}

