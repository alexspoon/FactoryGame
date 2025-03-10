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
            if (body is StaticBody2D)
            {
                bodiesToDamage.Add(body as StaticBody2D);
            } else if (body is RigidBody2D)
            {
                var rigid = body as RigidBody2D;
                var impulseDir = rigid.GlobalPosition - GlobalPosition * ExplosionForce;
                rigid.ApplyCentralImpulse(impulseDir);
            }
        }
        
        GD.Print("overlapping bodies: " + bodiesToDamage);
        Signals.EmitSignal(nameof(Signals.OreDamage), bodiesToDamage, Damage);
        //QueueFree();
    }
}
