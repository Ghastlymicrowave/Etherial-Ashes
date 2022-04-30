using Godot;
using System;

public class ResourceContainer : Resource{
    [Export] string _id;
    [Export] float _value;
    public string id => _id;
    public float value => _value;
    public ResourceContainer(string id, float value){
        _id = id;
        _value = value;
    }
    public ResourceContainer(){
        _id = "";
        _value = 0f;
    }
}
