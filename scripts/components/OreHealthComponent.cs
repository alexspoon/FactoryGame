using Godot;
using System.Linq;

public partial class OreHealthComponent : Node
{
    private CustomSignals Signals;
    [Export] private PackedScene DamageParticle;
    [Export] private PackedScene BreakParticle;
    private StaticBody2D Parent;
    public enum OreTypeList{Iron, Carbon, Copper, Gold, Aluminum};
    [Export] public OreTypeList OreType;
    [Export] private int MaxHealth;
    [Export] private int DamageResistance;
    private float CurrentHealth;

    public override void _Ready()
    {
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Signals.OreDamage += HandleDamage;
        Parent = GetParent() as StaticBody2D;
        CurrentHealth = MaxHealth;
    }

    private void HandleDamage(StaticBody2D[]Ores, int Damage){
        if(Ores.Contains(Parent)){
            Node2D particleInstance = DamageParticle.Instantiate() as Node2D;
            particleInstance.GlobalPosition = Parent.GlobalPosition;
            int damageToTake = Damage - DamageResistance;
            Parent.AddSibling(particleInstance);
            CurrentHealth -= damageToTake;
            GD.Print(Parent.Name + " took " + damageToTake + " damage!");
            if (CurrentHealth <= 0)
                Kill();
        }
    }

    private void Kill(){
        Signals.EmitSignal(nameof(Signals.OreBroken), Parent, (int)OreType);
        Node2D particleInstance = BreakParticle.Instantiate() as Node2D;
        particleInstance.GlobalPosition = Parent.GlobalPosition;
        Parent.AddSibling(particleInstance);
        Parent.QueueFree();
    }
}
