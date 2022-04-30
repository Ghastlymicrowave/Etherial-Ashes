using Godot;
using System;
using System.Collections.Generic;

public class game : Control
{
	public static bool paused = false;
	public object external;
	private Node actionPanel;
	private Node craftingPanel;
	private Quests quests;
	private static PackedScene action;
	private List<exeAction> actions;
	private List<exeAction> crafts;
	private inventory inventory;
	private RichTextLabel textLog;
	private RandomNumberGenerator rng;
	private int reboot = 0;
	SortedDictionary<string, GameResource> resourceData;//id, resource
	SortedDictionary<string, float> currentResources;//id resource amount
	SortedDictionary<string, exeActionTemplate> actionData;//aciton templates
	public SortedDictionary<string, float> Res => currentResources;
	public SortedDictionary<string, GameResource> ResData => resourceData;
	public SortedDictionary<string, exeActionTemplate> ActionData => actionData;
	public Dictionary<string,QuestTemplate> QuestData => questData;
	Dictionary<string,QuestTemplate> questData;

	[Export] GameResource[] importResources;
	[Export] QuestTemplate[] importQuests;
	[Export] exeActionTemplate[] importActions;

	public int[] ToArray(params int[] list){
		return list;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		resourceData = new SortedDictionary<string, GameResource>();//TODO replace below with this
		GenItemDict();
		actionData = new SortedDictionary<string, exeActionTemplate>();
		GenActionDict();
		questData = new Dictionary<string, QuestTemplate>();
		GenQuestDict();
		currentResources = new SortedDictionary<string, float>();

		actionPanel = FindNode("actionPanel");
		inventory = (inventory)FindNode("inventory");
		textLog = (RichTextLabel)FindNode("TextLog");
		craftingPanel = FindNode("craftingPanel");
		quests = (Quests)FindNode("questTree");
		action = GD.Load<PackedScene>("res://exeAction.tscn");
		actions = new List<exeAction>();
		crafts = new List<exeAction>();
		rng = new RandomNumberGenerator();
		GameStart();
		
		//DEBUG STUFF VVVV
		//addResource(new ResourceContainer("Reboot",3));
		//addResource("Scrap",20f, true, false);
		/// DEBUG STUFF ^^^^

		UpdateTree();
	}

	#region gameSpecific
	public void GameStart(){
		

		TabContainer craftTab = (TabContainer)craftingPanel.GetParent().GetParent();
		craftTab.SetTabDisabled(1,true);
		craftTab.SetTabTitle(1,"ERROR://SYSTEM CORRUPTED");
		NewAction("RebootSequence","Reboot","Attempt reboot sequence",25f,0f,false,false);
	}

	public void RebootSequence(){
		switch(Mathf.RoundToInt(GetResourceAmount("Reboot"))){
			case 0:
			if(rng.Randf()>0.4f){
				Log("REBOOT INITALIZED...FAILED, TRY AGAIN");
			}else{
				Log("REBOOT INITALIZED...SUCCESS, RESTORE POWER TO MAIN SYSTEMS TO CONTNUE");
				addResource(new ResourceContainer("Reboot",1));
			}
			break;
			case 1:
				Log("PHYSICAL SENSORY OBSERVER RESTORED");
				Log("The air feels warm. I think I am underneath a large slab, presumably rubble. What happened? Is there a fire?");
				addResource(new ResourceContainer("Reboot",1));
			break;
			case 2:
				Log("AUDITORY SENSORY OBSERVER RESTORED");
				Log("I can hear emergency sirens in the distance. Flames crackle faintly somewhere else. The sounds bounce around the area as if in a grand hall.");
				addResource(new ResourceContainer("Reboot",1));
			break;
			case 3:
				Log("OPTICAL SENSORY OBSERVER MALFUNCTIONING- GATHER MATERIALS FOR REPAIR");
				RemoveAction("Reboot");
				//NewAction("Scavenge","Scavenge","Gather assorted metal scraps littered around the ground.",0f, 10f, true, false, ToArray((int)RESOURCES.Makeshift_Apendage,5,5));
				NewAction("Scavenge");
				quests.AddQuest("intro_1");
				Log("My optics appear to have been damaged in whatever caused the destruction around me. Assuming there's some mechanical rubble nearby I should be able to reuse it with my matter reassembler.");
			break;

		}
	}

	public void qst_enableCrafting(){
		TabContainer craftTab = (TabContainer)craftingPanel.GetParent().GetParent();
		craftTab.SetTabDisabled(1,false);
		craftTab.SetTabTitle(1,"crafting");
		/*
		NewCraft("CraftChunk","Metal Chunk","Combine metal scraps into a metal chunk using the heat of nearby fires.",
		0f,5f,false,false, GenRequirements((int)RESOURCES.Scrap,2),ToArray((int)RESOURCES.Makeshift_Apendage,0,10)
		);
		NewCraft("CraftApendage", "Makeshift Apendage","Simple apendages can be welded together and when fed though with internal wiring, can become rudimentary limbs",
		0f,5f,false,false, GenRequirements((int)RESOURCES.Metal_Chunk,3),ToArray((int)RESOURCES.Makeshift_Apendage,0,10)
		);*/
		Log("Now that I can see, It's clear now that i'm missing an arm or two. That explains why it was so hard to pick up bits of rubble. I can probably make myself some makeshift arms in the same way I repared my opics.");
	}
	/*
	public void act_scavenge(){
		addResource("Scrap",1);
	}
	public void CraftChunk(){
		addResource("Metal Chunk",1);
	}*/

	public void CraftApendage(){
		addResource("Makeshift Apendage",1f,true,true);
		exeAction scavenge = GetAction("Scavenge");
		scavenge.stepVal+=5f;
	}
	#endregion
	
	#region  engineSpecific
	public int?[,] GenRequirements(params int[] list){
		int?[,] require = new int?[Mathf.FloorToInt(list.Length/2),2];
		for(int i = 0; i < require.Length-1;i++){
			GD.Print(i.ToString());
			require[i,0]=list[i*2];
			require[i,1]=list[i*2+1];
		}
		return require;
	}

	public void NewAction(string reference, string inText, string desc, float tick, float step, bool run, bool rep,UpgradeResource[] upgrade = null){//create action verbose
		if (upgrade==null){
			upgrade = new UpgradeResource[]{};
		}
		exeAction newAction = (exeAction)action.Instance();
		actions.Add(newAction);
		actionPanel.AddChild(newAction);
		newAction.Connect("exeActionComplete",this,reference);
		newAction.Init(tick, step, run, rep,false, inText,desc,upgrade);
	}
	public void NewAction(string id){//create aciton from template ID
		exeActionTemplate temp = ActionData[id];
		exeAction newAction = (exeAction)action.Instance();
		actions.Add(newAction);
		actionPanel.AddChild(newAction);
		if(temp.callFunction!=null&&temp.callFunction!=""){
			newAction.Connect("exeActionComplete",this,temp.callFunction);
		}
		newAction.CreateFromTemplate(id);
	}
	public void NewCraft(string reference, string inText, string desc, float tick, float step, bool run, bool rep, ResourceContainer[] requirements, UpgradeResource[] upgrade){//create craft verbose
		exeAction newCraft = (exeAction)action.Instance();
		crafts.Add(newCraft);
		craftingPanel.AddChild(newCraft);
		newCraft.Connect("exeActionComplete",this,reference);
		newCraft.requiredItems = requirements; 
		newCraft.Init(tick, step, run, rep,false, inText, desc, upgrade);
	}
	public void NewCraft(string id){//create craft from template ID
		exeActionTemplate temp = ActionData[id];
		exeAction newCraft = (exeAction)action.Instance();
		crafts.Add(newCraft);
		craftingPanel.AddChild(newCraft);
		if(temp.callFunction!=null&&temp.callFunction!=""){
			newCraft.Connect("exeActionComplete",this,temp.callFunction);
		}
		newCraft.CreateFromTemplate(id);
	}

	public void UpdateAction(string buttonName, string inText, string desc, float tick, float step, bool run, bool rep, bool repUnlocked, int?[,] requirements, UpgradeResource[] upgrade){
		for(int i = 0; i < actions.Count;i++){
			if (actions[i].button.Text == buttonName){
				exeAction thisAction = actions[i];
				thisAction.Init(tick,step,run,rep,repUnlocked,inText,desc,upgrade);
			}
		}
	}

	public exeAction GetAction(string buttonName){
		for(int i = 0; i < actions.Count;i++){
			if (actions[i].button.Text == buttonName){
				exeAction thisAction = actions[i];
				return thisAction;
			}
		}
		return null;
	}

	public void RemoveAction(string buttonName){
		for(int i = 0; i < actions.Count;i++){
			if (actions[i].button.Text == buttonName){
				exeAction thisAction = actions[i];
				actions.RemoveAt(i);
				thisAction.QueueFree();
			}
		}
	}
	public void RemoveCraft(string buttonName){
		for(int i = 0; i < crafts.Count;i++){
			if (crafts[i].button.Text == buttonName){
				exeAction thisAction = crafts[i];
				crafts.RemoveAt(i);
				thisAction.QueueFree();
			}
		}
	}
	public string GetLogTime(){
		//year month day weekday dst hour minute second
		DateTime time = DateTime.Now;
		string outTime;
		outTime = (time.Year+12004).ToString()+"/"+time.Month.ToString()+"-"+time.Day.ToString();
		outTime += "/" + time.Hour.ToString() +"."+ time.Minute.ToString() +"."+ time.Second.ToString();
		return outTime;
	}
	public void addResource(ResourceContainer container, bool updateTree = true, bool crafting = false){//// CHECK REFERENCES
		GD.Print("adding resource from resourcecontainer ", container.id, " : ", container.value);
		addResource(container.id,container.value,updateTree,crafting);
	}
	public void addResource(string id, float amount, bool updateTree = true, bool crafting = false){//resource val is the enum val converted to int
		GD.Print("Adding resource verbose");
		float current = 0;
		if (currentResources.TryGetValue(id, out current)){
			currentResources[id] = current+amount;
		}else{
			currentResources.Add(id,amount);
		}
		if(!resourceData[id].hidden){
			LogResourceChange(new ResourceContainer(id,amount),crafting);
		}
		if(updateTree){
			UpdateTree();
		}
	}
	public void addResources(ResourceContainer[] resources, bool updateTree = true, bool crafting = false){//organized like resource,ammount,resource,amount
		GD.Print("adding resources from container array");
		GD.Print(resources);
		for(int i=0; i < resources.Length;i++){	
			GD.Print("on adding multiple items, currently item :",i.ToString()," / ",resources.Length);
			addResource(resources[i],updateTree,crafting);
		}
		if(updateTree){
			UpdateTree();
		}
	}

	public float GetResourceAmount(string id){
		float value = 0f;
		currentResources.TryGetValue(id,out value);
		return value;
	}

	public void LogResourceChange(ResourceContainer res, bool crafting){
		textLog.Newline();
		textLog.Text+="LOG "+ GetLogTime()+ ": ";
		textLog.Text += res.value.ToString() + " " + resourceData[res.id].name;
		if (Mathf.Abs(res.value)>1){textLog.Text+="s";}
		if(crafting){
			if (res.value>0){
				textLog.Text += " created";
			}else{
				textLog.Text += " consumed";
			}
		}else{
			if (res.value>0){
				textLog.Text += " gained";
			}else{
				textLog.Text += " lost";
			}
		}
	}

	public void Log(string text){
		textLog.Newline();
		textLog.Text+="LOG "+ GetLogTime()+ ": "+text;
	}
	public void UpdateTree(){
		GD.Print("updating tree from game");
		inventory.UpdateTree();
		quests.updateQuestStatus();
	}

	#endregion

	//GameResource GetGameResource(resourceIdentifiableName)

	void GenItemDict(){
		foreach(GameResource res in importResources){
			resourceData.Add(res.id,res);
		}
	}

	void GenActionDict(){
		foreach(exeActionTemplate res in importActions){
			actionData.Add(res.id,res);
		}
	}

	void GenQuestDict(){
            foreach (QuestTemplate res in importQuests){
				GD.Print("loading quest: ",res.id);
                questData.Add(res.id,res);
            }
	    }
}
