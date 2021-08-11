using Godot;
using System;

public class exeAction : Control
{
	//TOGGLE TICKS AUTOMATICALLY
	//BUTTON STARTS AND STEPS
	// REWARD IS COLLECTED AUTOMATICALLY
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
	public int[] upgradeResources;//stored as resource, step bonus, tick bonus, ...

	public int?[,] requiredItems;

	public void Init(float tick, float step, bool run, bool rep,bool repUnlocked, string inText,string description, int[] upgrades){
		tickVal = tick;
		stepVal = step;
		running = run;
		repeat = rep;
		if (!repUnlocked){
			checkButton.Visible=false;
		}
		button.Text = inText;
		desc = description;
		baseTick=tick;
		baseStep=step;
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
		requiredItems = new int?[0,0];
	}

	public void SetRep(bool RepEnabled){
		checkButton.Visible=RepEnabled;
	}

	public void CheckUpgrade(){
		stepVal = baseStep;
		tickVal = baseTick;
		for(int i = 0; i < Mathf.FloorToInt( upgradeResources.Length/3f);i++){
			int amt = gameRef.resources[upgradeResources[i*3]];
			int stepUpgrade = upgradeResources[i*3+1];
			int tickUpgrade = upgradeResources[i*3+2];
			stepVal += stepUpgrade*amt;
			tickVal += tickUpgrade*amt;
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
				barCompleted();
			}
		}
	}

	public void barCompleted(){
		bar.Value = 0f;
		if (!repeat){
			running=false;
			EmitSignal(nameof(exeActionComplete));
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
				bar.Value+=stepVal;
				if (bar.Value >= bar.MaxValue){
					barCompleted();
				}
			}else{
				if (requiredItems.Length>0){
					GD.Print(requiredItems.ToString());
					if (IsCraftable()){
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
		for(int i = 0; i < requiredItems.Length-1;i++){
			if (gameRef.resources[(int)requiredItems[i,0]]<requiredItems[i,1]){
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
			outstring+=game.GetEnumItemName((int)requiredItems[i,0]) + ": "+gameRef.resources[(int)requiredItems[i,0]].ToString()+"/"+ requiredItems[i,1].ToString();

			if (gameRef.resources[(int)requiredItems[i,0]]<requiredItems[i,1]){
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
				outstr+="percent progress per second: "+stepVal.ToString()+"% ("+baseStep.ToString()+"% +"+(stepVal-baseStep).ToString()+"% bonus)";
			}
		}
		if (tickVal!=0){
			outstr+="\n";
			if (tickVal == baseTick){
				outstr+="percent progress per click: "+tickVal.ToString()+"%";
			}else{
				outstr+="percent progress per second: "+tickVal.ToString()+"% ("+baseTick.ToString()+"% +"+(tickVal-baseTick).ToString()+"% bonus)";
			}
		}
		return outstr;	
	}

	//Cutting down trees step bonus = min(arms, axes), tick bonus = arms+axes something like that??

	public void RemoveCraftItems(){//will crash if required items are not defined
		for(int i = 0; i < requiredItems.Length-1;i++){
			gameRef.resources[(int)requiredItems[i,0]]-=(int)requiredItems[i,1];
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







