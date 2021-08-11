using Godot;
using System;
using System.Collections.Generic;

public class Quests : Tree
{

	public class questItem{
		public TreeItem item;
		public int btnId;
		public questItem(TreeItem quest, int btn){
			item = quest;
			btnId = btn;
		}
	}

	TreeItem quest;
	questList list;
	List<questItem> quests;
	List<questList.questStruct> questStructs;
	Texture btnTex;
	game gameRef;
	public override void _Ready()
	{
		quest = CreateItem();
		quest.SetText(0,"Primary_Objectives");
		quests = new List<questItem>();
		questStructs = new List<questList.questStruct>();
		btnTex = ResourceLoader.Load<Texture>("res://complete.png");
		gameRef = (game)GetTree().Root.GetNode("Game");
	}

	public void UpdateQuestCompletion(string questName){
		for(int i = 0; i < questStructs.Count; i++){
			questList.questStruct thisQuest = questStructs[i];
			thisQuest.completeable=true;
		}
		updateQuestStatus();
	}

	public void updateQuestStatus(){
		for(int num = 0; num< questStructs.Count; num++){
			questList.questStruct i = questStructs[num];
			if (i.requirements.Length>0){
				bool ready = true;
				for(int item = 0; item < Mathf.FloorToInt(i.requirements.Length/2);item++){
					if (gameRef.resources[i.requirements[item*2]] < i.requirements[item*2+1]){
						ready = false;
						break;
					}
				}
				if (i.autocomplete&&ready){
					CompleteQuest(quests[num].item);
				}
				quests[num].item.SetButtonDisabled(0,0,!ready);
			}else{
				if (i.completeable){
					if (i.autocomplete){
						CompleteQuest(quests[num].item);
					}else{
						quests[num].item.SetButtonDisabled(0,0,true);
					}
				}
			}			
		}
	}

	public void AddQuest(int questID){
		questList.questStruct questStruct = questList.all[questID-1];
		questStructs.Add(questStruct);
		TreeItem newItem = CreateItem();
		newItem.AddButton(0,btnTex,-1,true);
		questItem newQuestItem = new questItem(newItem,newItem.GetButtonCount(0)-1);
		quests.Add(newQuestItem);

		quests[quests.Count-1].item.SetText(0,questStruct.name);
		TreeItem req = CreateItem(quests[quests.Count-1].item);
		req.SetText(0,"Requirements");
		TreeItem desc = CreateItem(quests[quests.Count-1].item);
		desc.SetText(0,"Description");

		for(int i = 0; i < Mathf.FloorToInt(questStruct.requirements.Length/2);i++){//propagate requirements
			TreeItem reqitem = CreateItem(req);
			reqitem.SetText(0,typeof(game.RESOURCES).GetEnumName(questStruct.requirements[i*2])+" : "+questStruct.requirements[i*2+1]);
		}

		for(int i = 0; i < questStruct.desc.Length;i++){//propagate description
			TreeItem descitem = CreateItem(desc);
			descitem.SetText(0,questStruct.desc[i]);
		}

		
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
			if (quests[i].item==item){
				//do all the things
				questList.questStruct questStruct = questStructs[i];
				if (questStruct.rewards.Length>1){
					gameRef.addResources(questStruct.rewards);
				}
				if (questStruct.nextQuest!=0){
					AddQuest(questStruct.nextQuest);
				}
				if(questStruct.completeFunction!=""){
					typeof(game).GetMethod(questStruct.completeFunction).Invoke(gameRef, null);
				}
				if (questStruct.consume){
					for(int r = 0; r < Mathf.FloorToInt(questStruct.requirements.Length/2);r++){
						gameRef.resources[questStruct.requirements[r*2]]-=questStruct.requirements[r*2+1];
					}
				}
				TreeItem current = item;
				questStructs.Remove(questStructs[i]);
				quest.RemoveChild(current);
				quests.Remove(quests[i]);  
				break;
			}
		}
	}
	private void _on_questTree_button_pressed(object item, int column, int id)
	{
		for(int i = 0; i < quests.Count;i++){
			if (quests[i].item==item){
				//do all the things
				questList.questStruct questStruct = questStructs[i];
				if (questStruct.rewards.Length>1){
					gameRef.addResources(questStruct.rewards);
				}
				if (questStruct.nextQuest!=0){
					AddQuest(questStruct.nextQuest);
				}
				if(questStruct.completeFunction!=""){
					typeof(game).GetMethod(questStruct.completeFunction).Invoke(gameRef, null);
				}
				if (questStruct.consume){
					for(int r = 0; r < Mathf.FloorToInt(questStruct.requirements.Length/2);r++){
						gameRef.resources[questStruct.requirements[r*2]]-=questStruct.requirements[r*2+1];
					}
				}
				TreeItem current = quests[i].item;
				questStructs.Remove(questStructs[i]);
				quest.RemoveChild(current);
				quests.Remove(quests[i]);  
				break;
			}
		}
	}
}



