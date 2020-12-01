using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class SavingManager : MonoBehaviour {
    public static SavingManager instance;
    private Saver saver;
    Saver.HomeworkData homeworkData;
    UnityEvent randomizePlayerEvent;
    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }

        saver = Saver.Instance;

    }
    // Start is called before the first frame update
    void Start() {
        //Testing most of the scripts, checking local save to memory as well as saving and loading to file using the Test class
        //saver.StartTest();
        if(randomizePlayerEvent == null) {
            randomizePlayerEvent = new UnityEvent();
        }
        randomizePlayerEvent.AddListener(RandomizePlayer);
        homeworkData = (Saver.HomeworkData)saver.GetDataInstance(SaveFile.HomeworkData);
        Debug.Log("Homework Data on Start: " + saver.GetJson(SaveFile.HomeworkData));

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            randomizePlayerEvent.Invoke();
            Debug.Log(homeworkData.playerData.name);
            saver.Save(SaveFile.HomeworkData);
            Debug.Log("Homework Data after randomization: " + saver.GetJson(SaveFile.HomeworkData));
        }

    }

    private void RandomizePlayer() {
        Debug.Log("Called event!");
        homeworkData.playerData.RandomizePlayerData();
    }
}

public enum SaveFile { Test, HomeworkData };
public class Saver {
    private static Saver instance;
    object WriteReadLock = new object();


    private Dictionary<SaveFile, DataInstance> savesData = new Dictionary<SaveFile, DataInstance>();

    Saver() {
        buildBasePath = Application.dataPath;
        savesData.Add(SaveFile.Test, new Test());
        savesData.Add(SaveFile.HomeworkData, new HomeworkData());

        foreach (SaveFile saveFile in Enum.GetValues(typeof(SaveFile))) {
            Load(saveFile);
        }
    }


    private string buildBasePath;

    public static Saver Instance {
        get {
            if (instance == null) {
                instance = new Saver();
                return instance;
            }
            else {
                return instance;
            }
        }
    }



    public DataInstance GetDataInstance(SaveFile saveFile) { return savesData[saveFile]; }

    private string DirectoryPath { get => buildBasePath + "/Saves/"; }

    private string GetFilePath(SaveFile saveFile) { return DirectoryPath + saveFile.ToString() + ".txt"; }

    internal string GetJson(SaveFile saveFile) { return JsonUtility.ToJson(GetDataInstance(saveFile)); }

    private void SetFromJson(SaveFile saveFile, string json) {
        DataInstance instance = GetDataInstance(saveFile);
        JsonUtility.FromJsonOverwrite(json, instance);
    }

    public bool FileExists(SaveFile saveFile) { return File.Exists(GetFilePath(saveFile)); }

    public void Save(SaveFile saveFile) {
        string filePath = GetFilePath(saveFile);
        if (Directory.Exists(DirectoryPath)) {
            lock (WriteReadLock) {
                File.WriteAllText(filePath, GetJson(saveFile));
            }
        }
        else {
            lock (WriteReadLock) {
                Directory.CreateDirectory(DirectoryPath);
            }
            Save(saveFile);
        }
    }
    public void Load(SaveFile saveFile) {
        string filePath = GetFilePath(saveFile);
        if (FileExists(saveFile)) {
            lock (WriteReadLock) {
                SetFromJson(saveFile, File.ReadAllText(filePath));
            }
        }
    }

    public void StartTest() {
        Debug.Log("TEST");
        string json = GetJson(SaveFile.Test);
        Debug.Log("Original Json: " + json);
        SetFromJson(SaveFile.Test, json);
        json = GetJson(SaveFile.Test);
        Debug.Log("Json after local load: " + json);
        string filePath = GetFilePath(SaveFile.Test);
        Debug.Log("File save path: " + filePath);
        Load(SaveFile.Test);
        Save(SaveFile.Test);
        Load(SaveFile.Test);
        json = GetJson(SaveFile.Test);
        Debug.Log("Json after hard load/save: " + json);
    }

    public class DataInstance {
    }

    private class Test : DataInstance {
        public int speed = 5;
    }


    public class HomeworkData : DataInstance {

        public PlayerData playerData = new PlayerData();

        [Serializable]
        public class PlayerData {
            public int level;
            public string name;
            public Inventory inventory = new Inventory();

            public void RandomizePlayerData() {
                level = UnityEngine.Random.Range(1, 100);
                name = nameExamples[UnityEngine.Random.Range(0, 10)];
                inventory.item1_id = UnityEngine.Random.Range(1, 100);
                inventory.item2_id = UnityEngine.Random.Range(1, 100);
                inventory.item3_id = UnityEngine.Random.Range(1, 100);
                inventory.item1_name = nameExamples[UnityEngine.Random.Range(0, 10)];
                inventory.item2_name = nameExamples[UnityEngine.Random.Range(0, 10)];
                inventory.item3_name = nameExamples[UnityEngine.Random.Range(0, 10)];

            }
        }


        [Serializable]
        public class Inventory {
            public int item1_id;
            public string item1_name;
            public int item2_id;
            public string item2_name;
            public int item3_id;
            public string item3_name;
        }


    }
    public static string[] nameExamples = new string[10] {
        "Fire Sword!",
        "Staff Of Doom!",
        "The Cape Of Luck",
        "The Wizard Pointy Hat",
        "The Sandals of Waterwalking",
        "The Eyes Of Medusa",
        "The Axe Of Gimly",
        "The Car Of Austin Powers",
        "The Millennium Falcon",
        "The One Ring"
    };
}


