using Godot;
using System;

public class inventory : Tree
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public game gameRef;
    TreeItem inven;
    TreeItem[] resources;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        inven = CreateItem();
        inven.SetText(0,"Inventory");
        resources = new TreeItem[typeof(game.RESOURCES).GetEnumNames().Length];
        for(int i = 0; i < typeof(game.RESOURCES).GetEnumNames().Length;i++){
            resources[i] = CreateItem();
            resources[i].SetText(0,"");
        }
        gameRef = (game)GetTree().Root.GetNode("Game");
    }

    public void UpdateTree(){
        for(int i = 0; i < typeof(game.RESOURCES).GetEnumNames().Length;i++){
            if (gameRef.resources[i]>0){
                if (resources[i] == null||resources[i].GetParent()==null){
                    resources[i] = CreateItem();
                }
                resources[i].SetText(0,game.GetEnumItemName(typeof(game.RESOURCES).GetEnumNames()[i])+" "+gameRef.resources[i]);
            }else{
                if (resources[i].GetParent()!=null){
                    GD.Print(resources[i]);
                    inven.RemoveChild(resources[i]); 
                }
            }
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
