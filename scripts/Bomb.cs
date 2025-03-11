using System.Collections.Specialized;
using System.Reflection.Metadata;
using Godot;
using Godot.Collections;

public partial class Bomb : RigidBody2D
{
    [Signal] public delegate void BombLitEventHandler();
    private CustomSignals Signals;
    private bool Lit;
    private Timer Fuse;
    private Area2D ExplosionRadius;
    [Export] private PackedScene ExplosionParticle;
    [Export] private int Damage;
    [Export] private float ExplosionForce;
    
    public override void _Ready()
    {
        Fuse = GetNode<Timer>("Fuse");
        ExplosionRadius = GetNode<Area2D>("ExplosionRadius");
        Fuse.Timeout += FuseTimeout;
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        BombLit += OnBombLight;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (LinearVelocity.Length() > 1000)
        {
            EmitSignalBombLit();
        }
    }

    private void OnBombLight()
    {
        Fuse.Start();
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
                GD.Print(impulseDir);
                rigidbody.ApplyCentralImpulse(impulseDir);
            }
        }
        
        var explosionParticle = ExplosionParticle.Instantiate() as Node2D;
        explosionParticle.GlobalPosition = GlobalPosition;
        AddSibling(explosionParticle);
        //GD.Print("overlapping bodies: " + bodiesToDamage);
        Signals.EmitSignal(nameof(Signals.OreDamage), bodiesToDamage, Damage);
        QueueFree();
    }
}
