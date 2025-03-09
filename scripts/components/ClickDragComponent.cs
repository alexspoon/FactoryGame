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
    private PIDController PID;
    private PIDController PIDX;
    private PIDController PIDY;
    [Export] private int Damage = 2;

    public override void _Ready(){
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Main = GetTree().GetRoot().GetNode<Node2D>("Main");
        Walls = Main.GetNode<StaticBody2D>("Start/Walls");
        Parent = GetParent() as Node2D;
        PID = GetNode<PIDController>("PID");
        PIDX = GetNode<PIDController>("PIDX");
        PIDY = GetNode<PIDController>("PIDY");
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
            PID.valueLast = Parent.GlobalPosition.Length();
            PIDX.valueLast = Parent.GlobalPosition.X;
            PIDY.valueLast = Parent.GlobalPosition.Y;
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
            GrabbedObject = null;
            ObjectHeld = false;
           
            // PIDX.errorLast = 0;
            // PIDY.errorLast = 0;
            // PIDX.integrationStored = 0;
            // PIDY.integrationStored = 0;
        }
        
        if (!ObjectHeld)
            return;

        

        Vector2 objPos = GrabbedObject.GlobalPosition;
        Vector2 targetPos = Parent.GlobalPosition;

        float targetMove = PID.UpdatePID(objPos.Length(), targetPos.Length(), (float)delta);

        var objVelocity = GrabbedObject.LinearVelocity;
        objVelocity = new Vector2(targetMove, targetMove);
        GrabbedObject.LinearVelocity = objVelocity;


        // float objPosX = objPos.X;
        // float objPosY = objPos.Y;

        // float targetPosX = targetPos.X;
        // float targetPosY = targetPos.Y;

        // targetMove = new Vector2(PIDX.UpdatePID(objPosX, targetPosX, (float)delta), PIDY.UpdatePID(objPosY, targetPosY, (float)delta));

        // var objVelocity = GrabbedObject.LinearVelocity;
        // objVelocity.X = targetMove.X;
        // objVelocity.Y = targetMove.Y;
        // GrabbedObject.LinearVelocity = objVelocity;

        // GD.Print(targetMove); 

        // Debug override
        // Vector2 targetPos = Parent.GlobalPosition;
        // Vector2 posLerp = GrabbedObject.GlobalPosition.Lerp(targetPos, 1f);
        // GrabbedObject.GlobalPosition = posLerp;
        // GrabbedObject.LinearVelocity = Vector2.Zero;
    }
}

