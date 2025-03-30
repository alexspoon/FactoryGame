using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class ClickDragComponent : Node
{
    private CustomSignals Signals;
    private Node2D Main;
    private StaticBody2D Walls;
    private Node2D Parent;
    private RigidBody2D GrabbedObject;
    private int BodiesHeld;
    private PIDController PID;
    private Array<PIDController> localPID = [];
    private Area2D GrabArea;
    private CollisionShape2D GrabShape;
    private Array<RigidBody2D> GrabbedBodies = [];
    private CircleShape2D LocalShape;
    [Export] private int Damage = 2;
    [Export] private float GrabRange = 32f;
    private bool GrabMultiple;
    private GpuParticles2D DragParticles;
    private GpuParticles2D SingleParticles;
    private GpuParticles2D ClickParticles;

    public override void _Ready(){
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Main = GetTree().GetRoot().GetNode<Node2D>("Main");
        Parent = GetParent() as Node2D;
        GrabArea = Parent.GetNode<Area2D>("GrabArea");
        GrabShape = GrabArea.GetNode<CollisionShape2D>("GrabCollider");
        PID = GetNode<PIDController>("PID");
        DragParticles = Parent.GetNode<GpuParticles2D>("Particles/DragParticles");
        SingleParticles = Parent.GetNode<GpuParticles2D>("Particles/SingleParticles");
        ClickParticles = Parent.GetNode<GpuParticles2D>("Particles/ClickParticles");
        BodiesHeld = 0;
        
        LocalShape = new CircleShape2D();
        LocalShape.Radius = GrabRange;
        GrabShape.Shape = LocalShape;
    }

    public override void _PhysicsProcess(double delta){
        HandleClick();
        HandleDrag(delta);
        ClickDamage();
        UpdateGrabMode();
    }
    
    private void HandleClick()
    {
        if (Input.IsActionJustPressed("LeftClick")) ClickParticles.Emitting = true;
        
        if (!GrabMultiple)
        {
            ClickDrag();
        }
        else
        {
            ClickArea();
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

        if (Input.IsActionJustPressed("LeftClick") && BodiesHeld == 0){
            GrabbedObject = hoveredObject.Obj as RigidBody2D;
            BodiesHeld = 1;
            PID.valueLastX = Parent.GlobalPosition.X;
            PID.valueLastY = Parent.GlobalPosition.Y;
        }
    }
    private void ClickArea()
    {
        if (Input.IsActionJustPressed("LeftClick") && BodiesHeld == 0)
        {
            //Update range if value is changed
            LocalShape.Radius = GrabRange;
            
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
                        valueLastY = Parent.GlobalPosition.Y
                    });
                }
            }
        }
    }

    private void HandleDrag(double delta)
    {
        if (!GrabMultiple)
        {
            HandleObject(delta);
        }
        else
        {
            HandleObjects(delta);
        }
    }
    private void HandleObject(double delta){
        SingleParticles.GlobalPosition = Parent.GlobalPosition;
        if (!IsInstanceValid(GrabbedObject))
        {
            GrabbedObject = null;
            SingleParticles.Emitting = false;
            BodiesHeld = 0;
            return;
        }
            
        
        if (Input.IsActionJustReleased("LeftClick") && BodiesHeld == 1){
            GrabbedObject.GravityScale = 1f;
            GrabbedObject = null;
            SingleParticles.Emitting = false;
            BodiesHeld = 0;
        }
        
        if (BodiesHeld == 0)
            return;

        SingleParticles.GlobalPosition = GrabbedObject.GlobalPosition;
        SingleParticles.Emitting = true;
        Vector2 objPos = GrabbedObject.GlobalPosition;
        Vector2 targetPos = Parent.GlobalPosition;
        
        Vector2 targetMove = new Vector2(PID.UpdatePIDX(objPos.X, targetPos.X, (float)delta), PID.UpdatePIDY(objPos.Y, targetPos.Y, (float)delta));
        
        var objVelocity = GrabbedObject.LinearVelocity;
        objVelocity = targetMove;
        GrabbedObject.LinearVelocity = objVelocity;
    }
    private void HandleObjects(double delta)
    {
        DragParticles.GlobalPosition = Parent.GlobalPosition;
        if (Input.IsActionJustReleased("LeftClick"))
        {
            localPID.Clear();
            GrabbedBodies.Clear();
            BodiesHeld = 0;
            DragParticles.Emitting = false;
        }
        
        if (BodiesHeld == 0)
            return;
        
        for (var i = 0; i < GrabbedBodies.Count; i++ )
        {
            if (!IsInstanceValid(GrabbedBodies[i]))
            {
                GrabbedBodies.Clear();
                BodiesHeld = 0;
                DragParticles.Emitting = false;
                return;
            }
            DragParticles.Emitting = true;
            var objPos = GrabbedBodies[i].GlobalPosition;
            var targetPos = Parent.GlobalPosition;
            var targetMove = new Vector2(localPID[i].UpdatePIDX(objPos.X, targetPos.X, (float)delta), localPID[i].UpdatePIDY(objPos.Y, targetPos.Y, (float)delta));

            var objVelocity = GrabbedBodies[i].LinearVelocity;
            objVelocity = targetMove;
            GrabbedBodies[i].LinearVelocity = objVelocity;
        }
        
        
    }
    
    private void ClickDamage(){
        ClickParticles.GlobalPosition = Parent.GlobalPosition;
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
    private void UpdateGrabMode()
    {
        if (Input.IsActionJustPressed("KeyQ") && BodiesHeld == 0)
            GrabMultiple = !GrabMultiple;
    }
    
}

