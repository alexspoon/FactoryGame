using Godot;
using System;

public partial class CustomSignals : Node
{
    [Signal] public delegate void OreBrokenEventHandler(StaticBody2D Ore, int OreType);
    [Signal] public delegate void OreDamageEventHandler(StaticBody2D Ore, int Damage);
}
