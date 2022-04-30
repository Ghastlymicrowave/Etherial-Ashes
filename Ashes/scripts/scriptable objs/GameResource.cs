using Godot;
using System;

public class GameResource : Resource
{
    [Export] bool _hidden = false;
    [Export] string _name = "unnamed";
    [Export] string _desc = "no description";
    [Export] string _identifierOverride = "";
    public bool hidden => _hidden;
    public string name => _name;
    public string description => _desc;
    public string id => identifierName();
    public string identifierName() {
        if (_identifierOverride==""){
            return name;
        }else{
            return _identifierOverride;
        }
    }
    public GameResource(){
        _hidden = false;
        _name = "";
        _desc = "";
        _identifierOverride = "";
    }
}