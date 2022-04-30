using Godot;
using System;

public class exeActionTemplate : Resource
{
    public string callFunction => _callFunction;
    public string name => _name;
    public string desc => _desc;
    public bool running => _running;
    public bool repUnlocked => _repUnlocked;
	public bool repeat => _repeat;
	public float baseTick => _baseTick;
	public float baseStep => _baseStep;
	public UpgradeResource[] upgradeResources => _upgradeResources;
	public ResourceContainer[] requiredItems => _requiredItems;
    public ResourceContainer[] generateItems => _generateItems;
    public string id => GetId();
    //
    [Export] string _callFunction;
    [Export] string _overriteID;
    [Export] string _name;
    [Export] string _desc;
    [Export] bool _running = false;
    [Export] bool _repUnlocked = false;
	[Export] bool _repeat = false;
	[Export] float _baseTick = 0f;
	[Export] float _baseStep = 10f;
	[Export] UpgradeResource[] _upgradeResources;
	[Export] ResourceContainer[] _requiredItems;
	[Export] ResourceContainer[] _generateItems;
    string GetId(){
        if (_overriteID==null){
            return name;
        }else{
            return _overriteID;
        }
    }
}
