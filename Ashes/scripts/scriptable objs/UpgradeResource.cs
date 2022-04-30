using Godot;
using System;

public class UpgradeResource : Resource{
    [Export] string _id;
    public string id => _id;
    [Export] float _stepUpgrade;
    public float stepUpgrade => _stepUpgrade;
    [Export] float _tickUpgrade;
    public float tickUpgrade => _tickUpgrade;
    public UpgradeResource(string id, float stepUgrade = 0f, float tickUpgrade = 0f){
        _id = id;
        _stepUpgrade = stepUgrade;
        _tickUpgrade = tickUpgrade;
    }
    public UpgradeResource(){
        _id = "";
        _stepUpgrade = 0f;
        _tickUpgrade = 0f;
    }
}
