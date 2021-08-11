using Godot;
using System;
using System.Collections.Generic;

public class questList : Node
{
    //int questID, int nextQuestID, Array rewards, Array requirements
    public static List<questStruct> all;
    public struct questStruct{
        public int id;
        public int nextQuest;//, 0 means no follow up quests after this
        public int[] requirements;
        public int[] rewards;
        public string name;
        public string[] desc;
        public bool consume;
        public string completeFunction;
        public bool autocomplete;
        public bool completeable;
        public questStruct( string questName, string[] description, int next, int[] require, int[] reward, bool consumeRequirements, string completeFunctionName, int thisid){
            nextQuest = next;
            requirements=require;
            rewards=reward;
            desc = description;
            name = questName;
            consume = consumeRequirements;
            completeFunction = completeFunctionName;
            id = thisid;
            autocomplete = false;
            completeable = false;
        }
        public questStruct( string questName, string[] description, int[] require, int[] reward, int thisid){
            nextQuest = thisid+1;
            requirements=require;
            rewards=reward;
            desc = description;
            name = questName;
            consume = true;
            completeFunction="";
            id = thisid;
            autocomplete = false;
            completeable = false;
        }
    }

    public int[] ToArray(params int[] input){
        return input;
    }

    public string[] ToArray(params string[] input){
        return input;
    }

    public void generateStoredQuest(string questName, string[] description, int next, int[] require, int[] reward, bool consumeRequirements, string completeFunctionName, ref int thisid){
        all.Add( new questStruct(questName,  description, next, require, reward, consumeRequirements, completeFunctionName, thisid));
        thisid++;
    }
    public void generateStoredQuest(string questName, string[] description, int[] require, int[] reward, ref int thisid){
        all.Add( new questStruct(questName, description, require, reward, thisid));
        thisid++;
    }
//when recieving quest rewards, if array length < 1, ignore

    public override void _Ready()
    {
        all = new List<questStruct>();
        int currentId = 1;
        /*
        in order:
        name
        description
        next id
        requirements (new int[0] or int[1] if none)
        rewards (new int[0] or int[1] if none)
        consume requirements?
        function to run in game ("" if none)
        currentId
        */
        generateStoredQuest(
            "REPAIR MALFUNCTIONING OBSERVATION SYSTEM",
            ToArray("SCRAP METAL CAN BE HARVESTED FROM","THE LOCAL REGION IN ORDER TO BE","COMBINED INTO CHUNKS TO REPLACE", "OPTICAL SYSTEMS."),
            currentId+1,
            ToArray((int)game.RESOURCES.Scrap,5),
            ToArray((int)game.RESOURCES.Metal_Chunk,1),
            true,
            "enableCrafting",
            ref currentId
        );

        generateStoredQuest(
            "COMPLETE MAKESHIFT LIMBS",
            ToArray("USE CONSTRUCTED METAL TO CREATE","TEMPORARY APENDAGES."),
            ToArray((int)game.RESOURCES.Makeshift_Apendage,3),
            new int[0],
            ref currentId
        );

        generateStoredQuest(
            "EXPLORE SUROUNDING AREA",
            ToArray("FIND OTHER SURVIVORS","OR EXPAND HORIZONS","TO COLLECT STRONGER MATERIALS"),
            new int[0],
            new int[0],
            ref currentId
        );
    }
}
