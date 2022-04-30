using Godot;
using System;

public class QuestTemplate : Resource
{
    [Export] string _name;
    public string name => _name;
    [Export] string _overriteId = "";//if not 0, overrites the ID, otherwise it's whatever the position in the folder it is.
    public string id => GetID();
    [Export] string _nextQuestName;//, 0 means no follow up quests after this
    public string nextQuestName => _nextQuestName;
    [Export] ResourceContainer[] _requirements;
    public ResourceContainer[] requirements => _requirements;
    [Export] ResourceContainer[] _rewards;
    public ResourceContainer[] rewards => _rewards;
    [Export] string[] _desc;
    public string[] description => _desc;
    [Export] bool _consume;
    public bool consume => _consume;
    [Export] string _completeFunction;
    public string completeFunction => _completeFunction;
    [Export] bool _autocomplete;
    public bool autocomplete => _autocomplete;
    [Export] bool _completeable;
    public bool completeable => _completeable;
    string GetID(){
        if (_overriteId==""){
            return _name;
        }else{
            return _overriteId;
        }
    }
}

public class Quest{
    string _name;
    string _overriteId = "";
    string _nextQuestName;
    ResourceContainer[] _requirements;
    ResourceContainer[] _rewards;
    string[] _desc;
    bool _consume;
    string _completeFunction;
    bool _autocomplete;
    bool _completeable;
    public string id => GetID();
    string GetID(){
        if (_overriteId!=""){
            return _name;
        }else{
            return _overriteId;
        }
    }
    public string name => _name;
    public string nextQuestName => _nextQuestName;
    public ResourceContainer[] requirements => _requirements;
    public ResourceContainer[] rewards => _rewards;
    public string[] description => _desc;
    public bool consume => _consume;
    public string completeFunction => _completeFunction;
    public bool autocomplete => _autocomplete;
    public bool completeable => _completeable;


    //TODO: => custom quest data public Quest(//stuff
    public Quest(QuestTemplate template){
        _name = template.name;
        _nextQuestName = template.nextQuestName;
        _requirements = template.requirements;
        _rewards = template.rewards;
        _desc = template.description;
        _consume = template.consume;
        _completeFunction = template.completeFunction;
        _completeable = template.completeable;
        _autocomplete = template.autocomplete;
    }

    public void setCompleteable(bool complete){_completeable = complete;}
    
}