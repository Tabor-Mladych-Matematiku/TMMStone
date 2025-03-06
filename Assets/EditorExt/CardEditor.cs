#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CardData;
using UnityEditor.UIElements;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine.Purchasing;

namespace CardEditor
{
    public class CardEditor : EditorWindow
    {
        static Dictionary<int, CardData.CardData> CardDatabase;
        readonly Dictionary<string, int> nameToId = new();
        static string jsonGUID;
        //static Dictionary<int, string[]> scriptpaths;
        List<string> choices;
        DropdownField cardList;
        [MenuItem("Window/CardScriptEditor")]
        public static void OpenCardEditor()
        {
            CardEditor wnd = GetWindow<CardEditor>();
            wnd.titleContent = new GUIContent("CardScriptEditor");
        }
        public void CreateGUI()
        {
            string[] jsonGUIDs = AssetDatabase.FindAssets($"{"cardScriptsPaths"} t:TextAsset", new[] { "Assets/Resources/CardData/Scripts" });
            if (jsonGUIDs.Length != 1)
            {
                Debug.LogError($"Looking for json resulted in: {jsonGUIDs.Length} GUIDs found");
            }
            jsonGUID = jsonGUIDs[0];
            //Debug.Log(Application.dataPath+ "/../Build/TMMstone_Data/Managed/");
            CardDatabase = CDJsonUtils.LoadCardDatabase();//Loads appropriate data
            VisualElement root = rootVisualElement;
            //root.Add(new Label("Behold Card Script editor!"));

            var box = new Box();
            cardList = new("Cards", new List<string>(), 0);
            foreach (var item in CardDatabase)
            {
                cardList.choices.Add(item.Value.name);
                nameToId.Add(item.Value.name, item.Key);
            }
            choices = cardList.choices;
            cardList.RegisterValueChangedCallback((item) =>
            {
                box.Clear();
                string name = item.newValue;
                int id = nameToId[name];
                CardData.CardData data = CardDatabase[id];
                //box.Add(new Label(name + "    " + data.cost));
                box.Add(new Image()
                {
                    image = Resources.Load<Texture2D>("CardData/" + CDJsonUtils.expansionMapping[data.expansion] + "/" + name)//;sprite =  Resources.Load<Sprite>("CardData/" + data.expansion + "/" + name)
                });
                //box.Add(new Label(data.type));
                //MonoScript script = GetScript(name); 
                if (data.scripts.Count == 0)
                {
                    box.Add(new Label("Does not have script"));
                    Button button = new()//can't use the consturctor action cuz it won't let me use button
                    {
                        text = "Add Script"
                    };
                    button.clicked += () =>
                    {
                        button.SetEnabled(false);
                        var endNameEditHandler = CreateInstance<EndNameEditHandler>();
                        endNameEditHandler.Init(/*button,*/ id);
                        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endNameEditHandler, "Assets/Resources/CardData/Scripts/" + CDJsonUtils.expansionMapping[data.expansion] + "/" + name + ".cs", null, null);



                    };


                    box.Add(button);
                }
                else
                {
                    box.Add(new Label("Has script"));
                    foreach (MonoScript script in LoadScripts(data.scripts))
                    {
                        box.Add(new Label("Script name: " + script.name));
                        box.Add(new Label(script.text));
                    }
                }
            });
            root.Add(cardList);
            Toggle t = new("Show unfinished only");
            Toggle t2 = new("Show finished only");
            t.RegisterValueChangedCallback(value =>
            {
                if (value.newValue) t2.value = false;
                if (!t2.value) FilterCardlist(value.newValue);
            });
            t2.RegisterValueChangedCallback(value =>
            {
                if (value.newValue) t.value = false;
                if (!t.value) FilterCardlistRev(value.newValue);
            });
            root.Add(t);
            root.Add(t2);
            root.Add(box);
            /*var toolbarMenu = new ToolbarMenu() { text = "Menu Text" };
            toolbarMenu.menu.AppendAction("Menu item 1", (a) => { Debug.Log("Menu item 1 clicked"); });
            toolbarMenu.menu.AppendAction("Menu item 2", (a) => { Debug.Log("Menu item 2 clicked"); });
            toolbarMenu.menu.AppendAction("Menu item 3", (a) => { Debug.Log("Menu item 3 clicked"); });
            root.Add(toolbarMenu);*/
            //Show
            //Selectable list of cards from the folder with card images.
            //TODO: Store a json of finished cards somewhere - add an option to filter finished cards.
            //Has an option to create/assing script
            //Scripts should end up in some special assembly.
        }
        class EndNameEditHandler : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            //Button button;
            int id;
            public void Init(/*Button button,*/ int id)
            {
                //this.button = button;
                this.id = id;
            }
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                //button.SetEnabled(true);
                string[] templateGUIDs = AssetDatabase.FindAssets($"{"CardScriptTemplate"} t:TextAsset", new[] { "Assets/EditorExt" });
                if (templateGUIDs.Length != 1)
                {
                    Debug.LogError($"Did not find template at \"Assets/EditorExt/CardScriptTemplate.txt\" {templateGUIDs.Length} GUIDs found");
                    return;
                }
                string templateContent = File.ReadAllText(AssetDatabase.GUIDToAssetPath(templateGUIDs[0]));
                if (templateContent.Length == 0)
                {
                    Debug.LogError("Empty template recieved");
                    return;
                }
                var classname = CDJsonUtils.SanitizeToClassName(Path.GetFileNameWithoutExtension(pathName));
                templateContent = templateContent.Replace("#NAME#", classname);
                File.WriteAllText(pathName, templateContent);
                AssetDatabase.ImportAsset(pathName);
                MonoScript newScript = AssetDatabase.LoadAssetAtPath<MonoScript>(pathName);
                string Resourcepath = pathName.Replace("Assets/Resources/", "");
                Resourcepath = Resourcepath.Replace(".cs", "");
                CDJsonUtils.Scriptpaths.Add(id, new() { Resourcepath });

                string json = MiniJson.JsonEncode(CDJsonUtils.Scriptpaths);
                //Debug.Log("Encoding: " + json);
                File.WriteAllText(AssetDatabase.GUIDToAssetPath(jsonGUID), json);

                CardDatabase[id].scripts.Add(Resourcepath);//This is only so that I don't need to reload the whole database after every change

                //TODO: add to script JSON
                ProjectWindowUtil.ShowCreatedAsset(newScript);
            }
            
        }
        /*
        private MonoScript GetScript(string name)
        {
            CardData.CardData data = CardDatabase[nameToId[name]];
            return Resources.Load<MonoScript>("CardData/Scripts/" + CDJsonUtils.expansionMapping[data.expansion] + "/" + name);
        }*/

        private void FilterCardlist(bool filter)
        {
            cardList.choices = (from choice in choices where !filter || CheckFilter(choice) select choice).ToList();
        }
        private void FilterCardlistRev(bool filter)
        {
            cardList.choices = (from choice in choices where !filter || !CheckFilter(choice) select choice).ToList();
        }
        bool CheckFilter(string card)
        {
            if (CardDatabase[nameToId[card]].scripts.Count != 0) return false;
            return true;
        }
        public static List<MonoScript> LoadScripts(List<string> scriptpaths)
        {
            List<MonoScript> scripts = new();
            foreach (string path in scriptpaths)
            {
                MonoScript s = Resources.Load<MonoScript>(path);//I ponder whether this is not too much of an overkill - load all card scripts at once. But... well I don't want to handle the ondemand stuff so...
                if (s != null) scripts.Add(s);
                else Debug.LogError("Could not add script");
            }
            return scripts;
        }
    }
}
#endif