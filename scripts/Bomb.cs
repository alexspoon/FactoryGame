using System.Collections.Specialized;
using System.Reflection.Metadata;
using Godot;
using Godot.Collections;

public partial class Bomb : RigidBody2D
{
    [Signal] public delegate void BombLitEventHandler();
    private AnimationPlayer BombAnimation;
    private CustomSignals Signals;
    private bool Lit;
    private Timer Fuse;
    private Area2D ExplosionRadius;
    private CollisionShape2D ExplosionShape;
    [Export] private PackedScene ExplosionParticle;
    [Export] private int Damage;
    [Export] private float ExplosionForce;
    [Export] private float ExplosionRange;
    
    public override void _Ready()
    {
        Fuse = GetNode<Timer>("Fuse");
        BombAnimation =  GetNode<AnimationPlayer>("BombAnimation");
        ExplosionRadius = GetNode<Area2D>("ExplosionRadius");
        ExplosionShape = ExplosionRadius.GetChild<CollisionShape2D>(0);
        Fuse.Timeout += FuseTimeout;
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        BombLit += OnBombLight;
        
        var explosionShape = new CircleShape2D();
        explosionShape.Radius = ExplosionRange;
        ExplosionShape.Shape = explosionShape;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (Lit)
            return;
        
        if (LinearVelocity.Length() > 1000)
        {
            EmitSignalBombLit();
        }
    }

    private void OnBombLight()
    {
        Fuse.Start();
        Lit = true;
        BombAnimation.Play("BombLit");
        GD.Print("bomb lit!");
    }
    
    private void FuseTimeout()
    {
        var overlapping = ExplosionRadius.GetOverlappingBodies();
        Array<StaticBody2D> bodiesToDamage = [];
        foreach (var body in overlapping)
        {
            var rigidbody = body as RigidBody2D;
            var staticbody = body as StaticBody2D;
            if (staticbody != null)
            {
                bodiesToDamage.Add(staticbody);
            } else if (rigidbody != null)
            {
                var impulseDir = (rigidbody.GlobalPosition - GlobalPosition).Normalized() * ExplosionForce * 10f;
                rigidbody.ApplyCentralImpulse(impulseDir);
            }
        }
        
        var explosionParticle = ExplosionParticle.Instantiate() as Node2D;
        explosionParticle.GlobalPosition = GlobalPosition;
        AddSibling(explosionParticle);
        Signals.EmitSignal(nameof(Signals.OreDamage), bodiesToDamage, Damage);
        QueueFree();
    }
}
