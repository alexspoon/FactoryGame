using Godot;
using Godot.Collections;
using System;

public partial class OreSpawnComponent : Node
{
    private Node2D SceneRoot;
    private Node2D ChunkPool;
    private CustomSignals Signals;
    private StaticBody2D Parent;

    //Chunk types
    [Export] private PackedScene IronChunk;
    [Export] private PackedScene CarbonChunk;


    public override void _Ready()
    {
        Parent = GetParent() as StaticBody2D;
        SceneRoot = GetTree().Root.GetNode<Node2D>("Main");
        ChunkPool = SceneRoot.GetNode<Node2D>("Ore Chunks");
        Signals = GetNode<CustomSignals>("/root/CustomSignals");
        Signals.OreBroken += OreSpawn;
    }

    private void OreSpawn(StaticBody2D Ore, int OreType)
    {
        if (Ore == Parent){
            Vector2 parentPos = Parent.GlobalPosition;
            RigidBody2D chunk = null;
            Array<Vector2> localPos = [new Vector2(-2,2), new Vector2(2,2), new Vector2(-2,-2), new Vector2(2,-2)];
            Array<PackedScene> oreTypes = [IronChunk, CarbonChunk];

            foreach(Vector2 pos in localPos){
                chunk = oreTypes[OreType].Instantiate() as RigidBody2D;
                Vector2 relPos = parentPos + pos;
                chunk.GlobalPosition = relPos;
                ChunkPool.AddChild(chunk);
            }
        }
    }
}
