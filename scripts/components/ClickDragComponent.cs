using Godot;
using System;

public partial class ClickDragComponent : Node
{
    private CustomSignals Signals;
    private Node2D Main;
    private StaticBody2D Walls;
    private Node2D Parent;
    private Camera2D Camera;
    private RigidBody2D GrabbedObject;
    private bool ObjectHeld;
    [Export] private float DragSpeed;
    private DampedSpringJoint2D DragSpring;
    [Export] private int Damage = 2;

    public override void _Ready(){
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Main = GetTree().GetRoot().GetNode<Node2D>("Main");
        Walls = Main.GetNode<StaticBody2D>("Start/Walls");
        Parent = GetParent() as Node2D;
        DragSpring = Parent.GetNode<DampedSpringJoint2D>("DragSpring");
        ObjectHeld = false;
    }

    public override void _PhysicsProcess(double delta){
        HandleObject(delta);
        ClickDrag();
        ClickDamage();
    }

    private void ClickDrag(){
        PhysicsDirectSpaceState2D worldState = Parent.GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D pointParams = new PhysicsPointQueryParameters2D
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

    private void ClickDamage(){
        PhysicsDirectSpaceState2D worldState = Parent.GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D pointParams = new PhysicsPointQueryParameters2D
        {
            CollideWithBodies = true,
            Position = Parent.GlobalPosition
        };
        var pointResults = worldState.IntersectPoint(pointParams);

        if (pointResults.Count == 0)
            return;

        var hoveredObject = pointResults[0]["collider"];

        if (hoveredObject.Obj is not StaticBody2D)
            return;

        StaticBody2D hoveredOre = hoveredObject.Obj as StaticBody2D;

        if (Input.IsActionJustPressed("LeftClick")){
            Signals.EmitSignal(nameof(Signals.OreDamage), hoveredOre, Damage);
        }
    }

    private void HandleObject(double delta){
        if (Input.IsActionJustReleased("LeftClick") && ObjectHeld){
            GrabbedObject.GravityScale = 1f;
            DragSpring.NodeB = null;
            GrabbedObject = null;
            ObjectHeld = false;
        }
        
        if (!ObjectHeld)
            return;

        Vector2 targetPos = Parent.GlobalPosition;
        Vector2 posLerp = GrabbedObject.GlobalPosition.Lerp(targetPos, 1f);
        GrabbedObject.GlobalPosition = posLerp;
        GrabbedObject.LinearVelocity = Vector2.Zero;

        // GrabbedObject.GravityScale = 0f;
        // DragSpring.NodeB = GetPathTo(GrabbedObject);

        // var grabTarget = (Parent.GlobalPosition - GrabbedObject.GlobalPosition).Normalized();
        // if (GrabbedObject.GlobalPosition.DistanceSquaredTo(Parent.GlobalPosition) > 64f)
        //     GrabbedObject.ApplyCentralForce(grabTarget * DragSpeed * 10);

        // if (GrabbedObject.GlobalPosition.DistanceTo(Parent.GlobalPosition) < 4f){
        //     GrabbedObject.LinearVelocity *= 0.9f;
        //     return;
        // }

        // GrabbedObject.LinearDamp = 0.1f;
        // var grabTarget = (Parent.GlobalPosition - GrabbedObject.GlobalPosition).Normalized();
        // var objectVelocity = GrabbedObject.LinearVelocity;
        // var targetVelocity = grabTarget * DragSpeed * (float)delta * 100;
        // var targetVelocityChange = objectVelocity.MoveToward(targetVelocity, 50f);
        // var targetAcceleration = targetVelocityChange;
        // GrabbedObject.LinearVelocity = targetAcceleration;
        // GrabbedObject.GravityScale = 0f;
    }
}

