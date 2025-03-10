using System.Linq;
using Godot;
using Godot.Collections;

public partial class ClickDragComponent : Node
{
    private CustomSignals Signals;
    private Node2D Main;
    private StaticBody2D Walls;
    private Node2D Parent;
    private Camera2D Camera;
    private RigidBody2D GrabbedObject;
    private bool ObjectHeld;
    private PIDController PID;
    [Export] private int Damage = 2;

    public override void _Ready(){
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Main = GetTree().GetRoot().GetNode<Node2D>("Main");
        Walls = Main.GetNode<StaticBody2D>("Start/Walls");
        Parent = GetParent() as Node2D;
        PID = GetNode<PIDController>("PID");
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
            PID.valueLastX = Parent.GlobalPosition.X;
            PID.valueLastY = Parent.GlobalPosition.Y;
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
        Array<StaticBody2D> oreArray = [];

        if (Input.IsActionJustPressed("LeftClick"))
        {
            oreArray.Add(hoveredOre);
            Signals.EmitSignal(nameof(Signals.OreDamage), oreArray, Damage);
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
        
        Vector2 objPos = GrabbedObject.GlobalPosition;
        Vector2 targetPos = Parent.GlobalPosition;
        
        Vector2 targetMove = new Vector2(PID.UpdatePIDX(objPos.X, targetPos.X, (float)delta), PID.UpdatePIDY(objPos.Y, targetPos.Y, (float)delta));
        
        var objVelocity = GrabbedObject.LinearVelocity;
        objVelocity = targetMove;
        GrabbedObject.LinearVelocity = objVelocity;
        
        GrabbedObject.ApplyTorque(GrabbedObject.GetAngleTo(Parent.GlobalPosition) * 1000);

        if (GrabbedObject == null)
            return;

        // Debug override
        // Vector2 targetPos = Parent.GlobalPosition;
        // Vector2 posLerp = GrabbedObject.GlobalPosition.Lerp(targetPos, 1f);
        // GrabbedObject.GlobalPosition = posLerp;
        // GrabbedObject.LinearVelocity = Vector2.Zero;
    }
}

