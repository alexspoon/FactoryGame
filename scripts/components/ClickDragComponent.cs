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
    private int BodiesHeld;
    private PIDController PID;
    private Array<PIDController> localPID = [];
    private Area2D GrabArea;
    private Array<RigidBody2D> GrabbedBodies = [];
    [Export] private int Damage = 2;

    public override void _Ready(){
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Main = GetTree().GetRoot().GetNode<Node2D>("Main");
        Parent = GetParent() as Node2D;
        GrabArea = Parent.GetNode<Area2D>("GrabArea");
        PID = GetNode<PIDController>("PID");
        ObjectHeld = false;
        BodiesHeld = 0;
    }

    public override void _PhysicsProcess(double delta){
        //HandleObject(delta);
        //ClickDrag();
        ClickArea();
        HandleObjects(delta);
        ClickDamage();
        GD.Print(BodiesHeld);
    }

    private void ClickArea()
    {
        if (Input.IsActionJustPressed("LeftClick") && BodiesHeld == 0)
        {
            var overlapping = GrabArea.GetOverlappingBodies();
            
            if (overlapping.Count == 0)
                return;
            
            foreach (var body in overlapping)
            {
                if (body is RigidBody2D)
                {
                    BodiesHeld++;
                    GrabbedBodies.Add(body as RigidBody2D);
                    localPID.Add(new PIDController
                    {
                        proportionalGain = PID.proportionalGain,
                        integralGain = PID.integralGain,
                        derivativeGain = PID.derivativeGain,
                        valueLastX = Parent.GlobalPosition.X,
                        valueLastY = Parent.GlobalPosition.Y,
                        errorLastX = Parent.GlobalPosition.X,
                        errorLastY = Parent.GlobalPosition.Y
                    });
                }
            }
        }
    }

    private void HandleObjects(double delta)
    {
        if (Input.IsActionJustReleased("LeftClick"))
        {
            for (var i = 0; i < localPID.Count; i++)
            {
                localPID[i].valueLastX = Parent.GlobalPosition.X;
                localPID[i].valueLastY = Parent.GlobalPosition.Y;
                localPID[i].errorLastX = Parent.GlobalPosition.X;
                localPID[i].errorLastY = Parent.GlobalPosition.Y;
            }
            
            GrabbedBodies.Clear();
            BodiesHeld = 0;
        }
        
        if (BodiesHeld == 0)
            return;
        
        for (var i = 0; i < GrabbedBodies.Count; i++ )
        {
            if (!IsInstanceValid(GrabbedBodies[i]))
            {
                GrabbedBodies.Remove(GrabbedBodies[i]);
                BodiesHeld--;
                return;
            }
            
            var objPos = GrabbedBodies[i].GlobalPosition;
            var targetPos = Parent.GlobalPosition;
            var targetMove = new Vector2(localPID[i].UpdatePIDX(objPos.X, targetPos.X, (float)delta), localPID[i].UpdatePIDY(objPos.Y, targetPos.Y, (float)delta));

            var objVelocity = GrabbedBodies[i].LinearVelocity;
            objVelocity = targetMove;
            GrabbedBodies[i].LinearVelocity = objVelocity;
        }
        
        
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
            PID.valueLastX = Parent.GlobalPosition.X;
            PID.valueLastY = Parent.GlobalPosition.Y;
        }
    }

    private void ClickDamage(){
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
        if (!IsInstanceValid(GrabbedObject))
        {
            GrabbedObject = null;
            ObjectHeld = false;
            return;
        }
            
        
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
        

        // Debug override
        // Vector2 targetPos = Parent.GlobalPosition;
        // Vector2 posLerp = GrabbedObject.GlobalPosition.Lerp(targetPos, 1f);
        // GrabbedObject.GlobalPosition = posLerp;
        // GrabbedObject.LinearVelocity = Vector2.Zero;
    }
}

