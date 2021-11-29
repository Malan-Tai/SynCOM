using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem 
{
    //a save function that translates the data to binary and saves it as lobby.save
    //must now determine what needs to be saved
    public static void saveFunction()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/lobby.save";
        SaveData data = new SaveData();
        //updating the saveData before saving it
        
        data.allyCharacters = GlobalGameManager.Instance.currentSquad;
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("data saved");

    }

    public static void loadFunction()
    {
        //checking if there is a save file
        if (File.Exists(Application.persistentDataPath+ "/lobby.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file =
                       File.Open(Application.persistentDataPath
                       + "/lobby.save", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();
            //loading the previously saved currentsquad into the GlobalGameManager
            for (int i=0; i<4; i++)
            {
                GlobalGameManager.Instance.SetSquadUnit(i, data.allyCharacters[i]);
            }
            BetweenMissionsGameManager.Instance.InitMissionRecapUnits();
            
            Debug.Log("Game data loaded!");
            //making sure that the save and load functions work as intended
            Debug.Log(GlobalGameManager.Instance.currentSquad[1].CharacterClass);
        }
        else
            Debug.LogError("There is no save data!");
    
    }
}
