using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Xml.Serialization;

public class SavingManager : MonoBehaviour {
    public static SavingManager instance;
    private SavingMediator saver;
    HomeworkData homeworkData;
    UnityEvent randomizePlayerEvent;
    [SerializeField]
    TextMeshProUGUI UItext;
    private bool XMLmode = true;

    public void ChangeMode(bool mode) {
        XMLmode = mode;
        LoadToScreen(SaveFile.HomeworkData, !mode);
    }
    
    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }

        saver = SavingMediator.Instance;
    }
    // Start is called before the first frame update
    void Start() {
        if (randomizePlayerEvent == null) {
            randomizePlayerEvent = new UnityEvent();
        }
        randomizePlayerEvent.AddListener(RandomizePlayer);
        homeworkData = (HomeworkData)saver.GetDataInstance(SaveFile.HomeworkData);
        LoadToScreen(SaveFile.HomeworkData,false);






        //Testing most of the scripts, checking local save to memory as well as saving and loading to file using the Test class
        //saver.StartTest();
    }


    public void SaveJson(SaveFile saveFile) {
        saver.Save(saveFile,true);
    }
    public void SaveJson(string saveFile) {
        SaveJson((SaveFile)Enum.Parse(typeof(SaveFile),saveFile));
    }
    public void LoadJson(SaveFile saveFile) {
        saver.Load(saveFile, true);
        LoadToScreen(saveFile,true);
    }
    public void LoadJson(string saveFile) {
        LoadJson((SaveFile)Enum.Parse(typeof(SaveFile), saveFile));
    }

    public void SaveXML(SaveFile saveFile) {
        saver.Save(saveFile, false);
    }
    public void SaveXML(string saveFile) {
        SaveXML((SaveFile)Enum.Parse(typeof(SaveFile), saveFile));
    }
    public void LoadXML(SaveFile saveFile) {
        saver.Load(saveFile, false);
        LoadToScreen(saveFile, false);
    }
    public void LoadXML(string saveFile) {
        LoadXML((SaveFile)Enum.Parse(typeof(SaveFile), saveFile));
    }
    public void StartEvent() {
        randomizePlayerEvent.Invoke();
    }
    private void RandomizePlayer() {
        homeworkData.playerData.RandomizePlayerData();
        LoadToScreen(SaveFile.HomeworkData,!XMLmode);
    }

    private void LoadToScreen(SaveFile saveFile, bool isJson) {
        if (isJson) {
            UItext.text = saver.GetJson(saveFile);
        }
        else {
            UItext.text = saver.GetXML(saveFile);
        }
        
    }
}

public enum SaveFile { Test, HomeworkData };
public enum SaveType { Json, XML }
public class SavingMediator {
    private static SavingMediator instance;
    object WriteReadLock = new object();


    private Dictionary<SaveFile, DataInstance> savesData = new Dictionary<SaveFile, DataInstance>();

    SavingMediator() {
        buildBasePath = Application.dataPath;
        savesData.Add(SaveFile.Test, new Test());
        savesData.Add(SaveFile.HomeworkData, new HomeworkData());
    }


    private string buildBasePath;

    public static SavingMediator Instance {
        get {
            if (instance == null) {
                instance = new SavingMediator();
                return instance;
            }
            else {
                return instance;
            }
        }
    }



    public DataInstance GetDataInstance(SaveFile saveFile) { return savesData[saveFile]; }

    private string DirectoryPath { get => buildBasePath + "/Saves/"; }

    private string GetFilePath(SaveFile saveFile, bool isJson) { return DirectoryPath + saveFile.ToString() + (isJson?"Json":"XML") + ".txt"; }

    internal string GetJson(SaveFile saveFile) { return JsonUtility.ToJson(GetDataInstance(saveFile),true); }

    internal string GetXML(SaveFile saveFile) {
        DataInstance instance = GetDataInstance(saveFile);
        XmlSerializer xmlSerializer = new XmlSerializer(instance.GetType());
        using (StringWriter textWriter = new StringWriter()) {
            xmlSerializer.Serialize(textWriter, instance);
            return textWriter.ToString();
        }
    }


    public bool FileExists(SaveFile saveFile, bool isJson) { return File.Exists(GetFilePath(saveFile, isJson)); }

    public void Save(SaveFile saveFile, bool isJson) {
        string data = (isJson ? GetJson(saveFile) : GetXML(saveFile));
        string filePath = GetFilePath(saveFile,isJson);
        if (Directory.Exists(DirectoryPath)) {
            lock (WriteReadLock) {
                File.WriteAllText(filePath, data);
            }
        }
        else {
            lock (WriteReadLock) {
                Directory.CreateDirectory(DirectoryPath);
            }
            Save(saveFile,isJson);
        }
    }
    public void Load(SaveFile saveFile, bool isJson) {
        string filePath = GetFilePath(saveFile, isJson);
        if (FileExists(saveFile, isJson)) {
            lock (WriteReadLock) {
                if (isJson) {
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), GetDataInstance(saveFile));
                }
                else {
                    using (StreamReader reader = new StreamReader(filePath)) {
                        DataInstance instance = GetDataInstance(saveFile);
                        XmlSerializer xmlSerializer = new XmlSerializer(instance.GetType());
                        instance = (DataInstance)xmlSerializer.Deserialize(reader);
                        
                    }
                }
            }
        }
    }

    public void StartTest() {
        Debug.Log("TEST");
        string json = GetJson(SaveFile.Test);
        Debug.Log("Original Json: " + json);
        JsonUtility.FromJsonOverwrite(json, GetDataInstance(SaveFile.Test));
        json = GetJson(SaveFile.Test);
        Debug.Log("Json after local load: " + json);
        string filePath = GetFilePath(SaveFile.Test, true);
        Debug.Log("File save path: " + filePath);
        Load(SaveFile.Test, true);
        Save(SaveFile.Test,true);
        Load(SaveFile.Test, true);
        json = GetJson(SaveFile.Test);
        Debug.Log("Json after hard load/save: " + json);
    }
}

public class DataInstance {
}

public class Test : DataInstance {
    public int speed = 5;
}
[System.Xml.Serialization.XmlRoot("HomeworkData")]
[Serializable]
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
    private static string[] nameExamples = new string[10] {
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






