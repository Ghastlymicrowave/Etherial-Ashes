using Godot;
using System;
using System.Collections.Generic;

public class Quests : Tree
{

	public class questItem{
		bool _completed = false;
		public bool completed => _completed;
		TreeItem _item;
		public TreeItem item => _item;
		int _btnId;
		public int btnId => _btnId;
		Quest _quest;
		public Quest quest => _quest;
		public questItem(TreeItem item, int btnId, Quest quest){
			_item = item;
			_btnId = btnId;
			_quest = quest;
		}
		public void SetComplete(){
			_completed = true;
		}
	}

	TreeItem quest;
	List<questItem> quests;
	Texture btnTex;
	game gameRef;
	public override void _Ready()
	{
		quest = CreateItem();
		quest.SetText(0,"Primary_Objectives");
		quests = new List<questItem>();
		btnTex = ResourceLoader.Load<Texture>("res://complete.png");
		gameRef = (game)GetTree().Root.GetNode("Game");
		quest.SetIconMaxWidth(0,600);
	}

	public void SetQuestCompleteable(string questID){
		for(int i = 0; i < quests.Count; i++){
			if (quests[i].quest.id==questID){
				quests[i].quest.setCompleteable(true);
			}
		}
		updateQuestStatus();
	}

	public void updateQuestStatus(){
		for(int num = 0; num < quests.Count; num++){
			Quest i = quests[num].quest;
			if (i.requirements.Length>0){
				bool ready = true;
				for(int item = 0; item < i.requirements.Length;item++){
					if (gameRef.GetResourceAmount(i.requirements[item].id)<i.requirements[item].value){
						ready = false;
						break;
					}
				}
				if (i.autocomplete&&ready&&!quests[num].completed){//AUTOCOMPLETE NOT WORKING?
					GD.Print("completed: ",quests[num].completed);
					CompleteQuest(quests[num].item);
					quests[num].SetComplete();
				}else{
					quests[num].item.SetButtonDisabled(0,0,!ready);
				}
				
			}else{
				if (i.completeable&&!quests[num].completed){
					if (i.autocomplete){
						CompleteQuest(quests[num].item);
						quests[num].SetComplete();
					}else{
						quests[num].item.SetButtonDisabled(0,0,false);
					}
				}
			}			
		}
	}

	public void AddQuest(string questId){
		
		QuestTemplate temp = gameRef.QuestData[questId];

		TreeItem newItem = CreateItem();
		newItem.AddButton(0,btnTex,-1,true);
		Quest thisQuest = new Quest(temp);
		quests.Add(new questItem(newItem,newItem.GetButtonCount(0)-1,thisQuest));
		questItem thisQuestItem = quests[quests.Count-1];
		thisQuestItem.item.SetText(0,thisQuestItem.quest.name);
		TreeItem req = null;
		TreeItem desc = CreateItem(thisQuestItem.item);
		desc.SetText(0,"Description");

		if (thisQuestItem.quest.requirements.Length>0){
			req = CreateItem(thisQuestItem.item);
			req.SetText(0,"Requirements");
		}
		for(int i = 0; i < thisQuestItem.quest.requirements.Length; i++){
			TreeItem reqitem = CreateItem(req);
			reqitem.SetText(0, gameRef.ResData[thisQuestItem.quest.requirements[i].id].name+" : "+thisQuestItem.quest.requirements[i].value.ToString());
		}

		for(int i = 0; i < thisQuestItem.quest.description.Length;i++){
			TreeItem descitem = CreateItem(desc);
			descitem.SetText(0,thisQuestItem.quest.description[i]);
		}

		updateQuestStatus();
		//res://icon.png
		//TODO:
		//set on click to complete reward
		//check rewards
		//give reward
		//do reward function if not ""
	}

	public TreeItem getQuestTreeItem(string questName){
		for(int i = 0; i < quests.Count;i++){
			if (quests[i].item.GetText(0)==questName){
				return quests[i].item;
			}
		}
		return null;
	}
	public void CompleteQuest(TreeItem item){
		for(int i = 0; i < quests.Count;i++){
			if (quests[i].item==item && !quests[i].completed){
				//do all the things
				Quest thisQuest = quests[i].quest;
				if (thisQuest.rewards.Length>0 && thisQuest.rewards!=null){
					GD.Print("getting quest items");
					gameRef.addResources(thisQuest.rewards,false);
				}
				if(thisQuest.completeFunction!=""){
					typeof(game).GetMethod(thisQuest.completeFunction).Invoke(gameRef, null);
				}
				if (thisQuest.consume){
					GD.Print("consuming quest item");
					for(int r = 0 ; r < thisQuest.requirements.Length; r++){
						ResourceContainer res = new ResourceContainer(thisQuest.requirements[r].id,-thisQuest.requirements[r].value);
						gameRef.addResource(res,false);
					}
				}
				string next = "";
				if (thisQuest.nextQuestName!=""){
					next = thisQuest.nextQuestName;
				}
				quests[i].SetComplete();//WHY THIS LINE NOT WORK LIKE IT SHOULD? have to have a backup elsewhere so stinky :(
				TreeItem current = item;
				quest.RemoveChild(current);
				quests.Remove(quests[i]);  
				if (thisQuest.nextQuestName!=""){
					AddQuest(thisQuest.nextQuestName);
				}
				break;
			}
		}
		gameRef.UpdateTree();
	}
	private void _on_questTree_button_pressed(object item, int column, int id)
	{
		for(int i = 0; i < quests.Count;i++){
			if (quests[i].item==item){
				//do all the things
				questItem questItem = quests[i];
				if (questItem.quest.rewards.Length>1){
					gameRef.addResources(questItem.quest.rewards, false);
				}
				if (questItem.quest.nextQuestName!=""){
					AddQuest(questItem.quest.nextQuestName);
				}
				if(questItem.quest.completeFunction!=""){
					typeof(game).GetMethod(questItem.quest.completeFunction).Invoke(gameRef, null);
				}
				if (questItem.quest.consume){
					GD.Print("consuming quest items");
					for(int r = 0; r < questItem.quest.requirements.Length;r++){
						ResourceContainer res = new ResourceContainer(questItem.quest.requirements[r].id,-questItem.quest.requirements[r].value);
						gameRef.addResource(res);
					}
				}
				TreeItem current = quests[i].item;
				quest.RemoveChild(current);
				quests.Remove(quests[i]);  
				break;
			}
		}
		gameRef.UpdateTree();
	}
}



