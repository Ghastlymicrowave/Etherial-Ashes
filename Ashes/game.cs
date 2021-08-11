using Godot;
using System;
using System.Collections.Generic;

public class game : Control
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
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

	public enum RESOURCES{
		Scrap,
		Metal_Chunk,
		Glass_Bead,
		Wooden_Scrap,
		Metal_Plate,
		Makeshift_Apendage
	}

	public string getEnumDescription(int val){
		switch(val){
			case 0://scrap
			return "Assorted metal scraps gathered from the rubble. Could be compressed into somethign usable.";
			case 1://metal chunk
			return "Compressed metal scrap, can be used for rudimentery machinery.";
			case 2://bead
			return "Pristine and small glass beads, seemingly untouched.";
			case 3://wooden scrap
			return "Wooden rubble. With enough, these could be compressed into a sort of plywood.";
			case 4://metal plate
			return "Formed metal. This could be usefull to create anything strudy.";
			case 5://makeshift apendage
			return "A very basic replacement arm. Makes picking up rubble a little easier. Also can craft items on it's own.";
			default: return "";
		}
	}

	public int[] resources;

	public static string GetEnumItemName(string inputStr){
		///string outstr = inputStr.Remove(inputStr.Length-1,1);
		return inputStr.Replace("_"," ");
	}
	public static string GetEnumItemName(int resourceVal){
		return GetEnumItemName(typeof(RESOURCES).GetEnumNames()[resourceVal]);	
	}

	public int[] ToArray(params int[] list){
		return list;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		resources = new int[typeof(RESOURCES).GetEnumNames().Length];
		for(int i = 0; i <typeof(RESOURCES).GetEnumNames().Length; i++ ){
			resources[i]=0;
		}
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
		reboot = 3;
		addResource((int)RESOURCES.Scrap,20);
		/// DEBUG STUFF ^^^^

		UpdateTree();
	}

	#region gameSpecific
	public void GameStart(){
		

		TabContainer craftTab = (TabContainer)craftingPanel.GetParent().GetParent();
		craftTab.SetTabDisabled(1,true);
		craftTab.SetTabTitle(1,"ERROR://SYSTEM CORRUPTED");
		reboot = 0;
		NewAction("RebootSequence","Reboot","Attempt reboot sequence",25f,0f,false,false,new int[0]);
	}

	public void RebootSequence(){
		switch(reboot){
			case 0:
			if(rng.Randf()>0.8f){
				Log("REBOOT INITALIZED...FAILED, TRY AGAIN");
			}else{
				Log("REBOOT INITALIZED...SUCCESS, RESTORE POWER TO MAIN SYSTEMS TO CONTNUE");
				reboot++;
			}
			break;
			case 1:
				Log("PHYSICAL SENSORY OBSERVER RESTORED");
				Log("The air feels warm. I think I am underneath a large slab, presumably rubble. What happened? Is there a fire?");
				reboot++;
			break;
			case 2:
				Log("AUDITORY SENSORY OBSERVER RESTORED");
				Log("I can hear emergency sirens in the distance. Flames crackle faintly somewhere else. The sounds bounce around the area as if in a grand hall.");
				reboot++;
			break;
			case 3:
				Log("OPTICAL SENSORY OBSERVER MALFUNCTIONING- GATHER MATERIALS FOR REPAIR");
				RemoveAction("Reboot");
				NewAction("Scavenge","Scavenge","Gather assorted metal scraps littered around the ground.",0f, 10f, true, false, ToArray((int)RESOURCES.Makeshift_Apendage,5,5));
				quests.AddQuest(1);
				Log("My optics appear to have been damaged in whatever caused the destruction around me. Assuming there's some mechanical rubble nearby I should be able to reuse it with my matter reassembler.");
			break;

		}
	}

	public void enableCrafting(){
		TabContainer craftTab = (TabContainer)craftingPanel.GetParent().GetParent();
		craftTab.SetTabDisabled(1,false);
		craftTab.SetTabTitle(1,"crafting");
		NewCraft("CraftChunk","Metal Chunk","Combine metal scraps into a metal chunk using the heat of nearby fires.",
		0f,5f,false,false, GenRequirements((int)RESOURCES.Scrap,2),ToArray((int)RESOURCES.Makeshift_Apendage,0,10)
		);
		NewCraft("CraftApendage", "Makeshift Apendage","Simple apendages can be welded together and when fed though with internal wiring, can become rudimentary limbs",
		0f,5f,false,false, GenRequirements((int)RESOURCES.Metal_Chunk,3),ToArray((int)RESOURCES.Makeshift_Apendage,0,10)
		);
		Log("Now that I can see, It's clear now that i'm missing an arm or two. That explains why it was so hard to pick up bits of rubble. I can probably make myself some makeshift arms in the same way I repared my opics.");
	}
	public void Scavenge(){
		addResource(RESOURCES.Scrap,1);
	}
	public void CraftChunk(){
		addResource(RESOURCES.Metal_Chunk,1);
	}

	public void CraftApendage(){
		addResource(RESOURCES.Makeshift_Apendage,1);
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

	public void NewAction(string reference, string inText, string desc, float tick, float step, bool run, bool rep,int[] upgrade){
		exeAction newAction = (exeAction)action.Instance();
		actions.Add(newAction);
		actionPanel.AddChild(newAction);
		newAction.Connect("exeActionComplete",this,reference);
		newAction.Init(tick, step, run, rep,false, inText,desc,upgrade);
	}

	public void NewCraft(string reference, string inText, string desc, float tick, float step, bool run, bool rep, int?[,] requirements, int[] upgrade){
		exeAction newCraft = (exeAction)action.Instance();
		crafts.Add(newCraft);
		craftingPanel.AddChild(newCraft);
		newCraft.Connect("exeActionComplete",this,reference);
		newCraft.requiredItems = requirements; 
		newCraft.Init(tick, step, run, rep,false, inText, desc, upgrade);
	}

	public void UpdateAction(string buttonName, string inText, string desc, float tick, float step, bool run, bool rep, bool repUnlocked, int?[,] requirements, int[] upgrade){
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

	public void addResource(int resource, int amount){//resource val is the enum val converted to int
		resources[resource] += amount;
		GD.Print(textLog.Text);
		GD.Print(GetLogTime());
		textLog.Newline();
		textLog.Text+="LOG "+ GetLogTime()+ ": "+ amount.ToString() +" "+ GetEnumItemName(resource);
		if (amount>1){textLog.Text+="s";} 
		textLog.Text += " gained";
		UpdateTree();
	}
	public void addResource(RESOURCES resource, int amount){//resource val is the enum val converted to int
		addResource((int)resource,amount);
	}

	public void addResources(int[] inRes){//organized like resource,ammount,resource,amount
	textLog.Newline();
	textLog.Text+="LOG "+ GetLogTime()+ ": ";
	GD.Print(inRes.ToString());
		for(int i=0; i < Mathf.FloorToInt(inRes.Length/2);i++){	
			resources[inRes[i*2]] += inRes[i*2+1];
			textLog.Text += inRes[i*2+1].ToString() +" "+ GetEnumItemName(inRes[i*2].ToString());
			if (inRes[i*2+1]>1){textLog.Text+="s";};
			textLog.Text += " gained";
		}
		UpdateTree();
	}

	public void Log(string text){
		textLog.Newline();
		textLog.Text+="LOG "+ GetLogTime()+ ": "+text;
	}
	public void UpdateTree(){
		inventory.UpdateTree();
		quests.updateQuestStatus();
	}

	#endregion
}
