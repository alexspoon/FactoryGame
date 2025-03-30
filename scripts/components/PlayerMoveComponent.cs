using Godot;
using System;

public partial class PlayerMoveComponent : Node
{
    private bool StayUpright = true;
    private RigidBody2D Parent;
    [Export] private float MoveSpeed = 1000f;
    private Vector2 JumpForce = new Vector2(0, -10000);
    private Vector2 InputDir = Vector2.Zero;
    private PIDController PID;
    private GpuParticles2D JetpackParticles;
    private Marker2D JetpackMarker;
    
    public override void _Ready()
    {
        Parent = GetParent<RigidBody2D>();
        PID = Parent.GetNode<PIDController>("PID");
        JetpackParticles = Parent.GetNode<GpuParticles2D>("JetpackParticles");
        JetpackMarker = Parent.GetNode<Marker2D>("JetpackMarker");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        BasicMovement();
        if (StayUpright)
        { 
            RotateCorrection(delta);
            Parent.AngularDamp = 5;
        } else Parent.AngularDamp = 0;
    }

    private void BasicMovement()
    {
        InputDir = Input.GetVector("KeyA", "KeyD", "KeyW", "KeyS").Normalized();

        if (Input.IsActionJustPressed("KeyTab")) StayUpright = !StayUpright;
        
        if (Input.IsActionPressed("KeySpace"))
        {
            Parent.ApplyForce(JumpForce, JetpackMarker.Position);
            JetpackParticles.GlobalPosition = JetpackMarker.GlobalPosition;
            JetpackParticles.RotationDegrees = Parent.RotationDegrees;
            JetpackParticles.ProcessMaterial.Set(ParticleProcessMaterial.PropertyName.Direction, new Vector3(0,JetpackMarker.GlobalPosition.Y + 1,0));
            JetpackParticles.Emitting = true;
        } else JetpackParticles.Emitting = false;
        
        if (InputDir == Vector2.Zero) return;
        Parent.ApplyCentralForce(InputDir * MoveSpeed * Parent.Mass);
    }
    
    private void RotateCorrection(double delta)
    {
        if (Parent.Rotation == 0f) return;

        float targetRotation = PID.UpdatePIDX(Parent.Rotation, 0f, (float)delta);
        Parent.ApplyTorqueImpulse(targetRotation);
    }
}
