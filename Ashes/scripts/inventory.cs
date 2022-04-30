using Godot;
using System;
using System.Collections.Generic;
public class inventory : Tree
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public game gameRef;
    TreeItem inven;
    SortedDictionary<string, TreeItem> resources;//id, item
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        inven = CreateItem();
        inven.SetText(0,"Inventory");
        gameRef = (game)GetTree().Root.GetNode("Game");
        resources = new SortedDictionary<string, TreeItem>();
    }

    public void UpdateTree(){
        GD.Print("updating tree");
        foreach (string key in gameRef.Res.Keys){
            float amt = gameRef.Res[key];
            GameResource data = gameRef.ResData[key];
            TreeItem item;
            if (amt>0 && !data.hidden){
                if(!resources.TryGetValue(key,out item)){
                    resources.Add(key,CreateItem());
                    item = resources[key];
                    GD.Print("adding new item with key: ",key);
                }
                item.SetText(0,data.name+" "+amt);
            }else{
                if(resources.TryGetValue(key,out item)){
                    if(item.GetParent()!=null)
                        inven.RemoveChild(item);
                }
            }
        }
    }
}
