using Godot;
using System;

public class exeAction : Control
{
	//TOGGLE TICKS AUTOMATICALLY
	//BUTTON STARTS AND STEPS
	// REWARD IS COLLECTED AUTOMATICALLY


/////////////////////////////////////////////////////////////////////////////////
/*
TODO:

Make exe actions scriptable objs so that they have the option to provide:
material (visible or hidden) rewards, 
special log text, 
unlock quests 
an option to remove themselves when used a number of times
call any void function (in some actions script, or game?) when triggered
call any void function when clicked
*/
///////////////////////////////////////////////////////////////////////////////////


	//
	public string desc;
	public game gameRef;
	private ProgressBar bar;
	public Button button;
	private CheckButton checkButton;
	public bool running = true;
	public bool repeat = false;
	public float baseTick = 0f;
	public float baseStep = 10f;
	public float tickVal = 0f;
	public float stepVal = 10f;
	public UpgradeResource[] upgradeResources;//stored as resource, step bonus, tick bonus, ...
	public ResourceContainer[] requiredItems;
	public ResourceContainer[] generateItems;

	public void CreateFromTemplate(string id){
		exeActionTemplate temp = gameRef.ActionData[id];
		desc = temp.desc;
		running = temp.running;
		repeat = temp.repeat;
		baseStep = temp.baseStep;
		baseTick = temp.baseTick;
		tickVal = temp.baseTick;
		stepVal = temp.baseStep;
		upgradeResources = temp.upgradeResources;
		requiredItems = temp.requiredItems;
		generateItems = temp.generateItems;
		button.Text = temp.name;
		CheckUpgrade();
	}
	public void Init(float tick, float step, bool run, bool rep,bool repUnlocked, string inText,string description, UpgradeResource[] upgrades){
		tickVal = tick;
		stepVal = step;
		baseTick=tick;
		baseStep=step;
		running = run;
		repeat = rep;
		if (!repUnlocked){
			checkButton.Visible=false;
		}
		button.Text = inText;
		desc = description;
		upgradeResources = upgrades;
		CheckUpgrade();
	}

	[Signal] public delegate void exeActionComplete();
	public override void _Ready()
	{
		checkButton = (CheckButton)GetNode("HBoxContainer/CheckButton");
		button = (Button)GetNode("HBoxContainer/OptionButton");
		bar = (ProgressBar)GetNode("HBoxContainer/ProgressBar");
		gameRef = (game)GetTree().Root.GetNode("Game");
		requiredItems = new ResourceContainer[]{};
	}

	public void SetRep(bool RepEnabled){
		checkButton.Visible=RepEnabled;
	}

	public void CheckUpgrade(){
		stepVal = baseStep;
		tickVal = baseTick;
		for(int i = 0; i < upgradeResources.Length;i++){
			float amt = gameRef.GetResourceAmount(upgradeResources[i].id);
			stepVal += upgradeResources[i].stepUpgrade*amt;
			tickVal += upgradeResources[i].tickUpgrade*amt;
		}
		UpdateTooltip();
	}

	public void UpdateTooltip(){
		button.HintTooltip = desc + RequiredItemsText() + PercentText();
	}
	
	public override void _Process(float delta)
	{
		if (running && !game.paused){
			bar.Value += tickVal * delta;
			if (bar.Value >= bar.MaxValue){
				bar.Value = 0f;
				GD.Print("Tick completed, bar is at ", bar.Value);
				barCompleted();
			}
		}
	}

	public void barCompleted(){
		GD.Print("Bar completed");
		bar.Value = 0f;
		if (!repeat){
			running=false;
		}
		EmitSignal(nameof(exeActionComplete));
		if(generateItems!=null){
			gameRef.addResources(generateItems);
		}
	}
	private void _on_CheckButton_toggled(bool button_pressed)
	{
		repeat = checkButton.Pressed;
		if (!running){
			running = true;
		}
	}


	private void _on_OptionButton_pressed()
	{
		CheckUpgrade();
		if (!game.paused){
			if (running){
				bar.Value= bar.Value + stepVal;
				if (bar.Value >= bar.MaxValue){
					bar.Value = 0f;
					GD.Print("Step Completed, bar is at ", bar.Value);
					barCompleted();
				}
			}else{
				if (requiredItems.Length>0){
					GD.Print(requiredItems.ToString());
					if (IsCraftable()){
						GD.Print("Triggering RemoveCraftItems");
						RemoveCraftItems();
						running = true;
						bar.Value+=stepVal;
					}
				}else{
					running = true;
					bar.Value+=stepVal;
				}
			}
		}
		
	}

	public bool IsCraftable(){//will crash if required items are not defined
		for(int i = 0; i < requiredItems.Length;i++){
			if(gameRef.GetResourceAmount(requiredItems[i].id)<requiredItems[i].value){
				return false;
			}
		}
		return true;
	}

	public string RequiredItemsText(){
		string outstring = "";
		if (requiredItems==null){return outstring;}
		for(int i = 0; i < requiredItems.Length-1;i++){
			outstring+="\n";
			outstring+= gameRef.ResData[requiredItems[i].id];
			outstring+=": " + gameRef.Res[requiredItems[i].id].ToString() + "/" + requiredItems[i].value.ToString();
			if (gameRef.Res[requiredItems[i].id] < requiredItems[i].value){
				outstring+=" MISSING REQUIREMENTS";
			}
		}
		return outstring;
	}

	public string PercentText(){
		string outstr = "";
		if(stepVal!=0){
			outstr+="\n";
			if (stepVal == baseStep){
				outstr+="percent progress per click: "+stepVal.ToString()+"%";
			}else{
				outstr+="percent progress per click: "+stepVal.ToString()+"% ("+baseStep.ToString()+"% +"+(stepVal-baseStep).ToString()+"% bonus)";
			}
		}
		if (tickVal!=0){
			outstr+="\n";
			if (tickVal == baseTick){
				outstr+="percent progress per second: "+tickVal.ToString()+"%";
			}else{
				outstr+="percent progress per second: "+tickVal.ToString()+"% ( +"+((tickVal-baseTick)/baseTick*100f).ToString()+"% bonus)";
			}
		}
		return outstr;	
	}

	//Cutting down trees step bonus = min(arms, axes), tick bonus = arms+axes something like that??

	public void RemoveCraftItems(){//will crash if required items are not defined
		GD.Print("removing craft items");
		for(int i = 0; i < requiredItems.Length-1;i++){
			ResourceContainer res = new ResourceContainer(requiredItems[i].id,-requiredItems[i].value);
			gameRef.addResource(res,true);
		}
		gameRef.UpdateTree();
	}
	private void _on_OptionButton_mouse_entered()
	{
		//button.hint
	}


	private void _on_OptionButton_mouse_exited()
	{
		// Replace with function body.
	}
}







//TODO
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*

convert quests into scriptable objs
convert exe actions into scriptable objs

*/
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////