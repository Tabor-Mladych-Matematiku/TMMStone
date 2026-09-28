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
            rootVisualElement.Clear();
            nameToId.Clear();
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
                ShowCard(box, item.newValue);
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

        private void ShowCard(Box box, string name)
        {
            box.Clear();
            int id = nameToId[name];
            CardData.CardData data = CardDatabase[id];
            box.Add(new Image()
            {
                image = Resources.Load<Texture2D>("CardData/" + CDJsonUtils.expansionMapping[data.expansion] + "/" + name)
            });

            if (data.scripts.Count == 0)
            {
                box.Add(new Label("Does not have a script"));
            }
            else
            {
                box.Add(new Label("Attached scripts"));
                foreach (MonoScript script in LoadScripts(data.scripts))
                {
                    box.Add(new Label("Script name: " + script.name));
                    Button revealScriptButton = new(() =>
                    {
                        string assetPath = AssetDatabase.GetAssetPath(script);
                        EditorUtility.RevealInFinder(Path.GetFullPath(assetPath));
                    })
                    {
                        text = "Reveal in File Explorer"
                    };
                    box.Add(revealScriptButton);
                    box.Add(new Label(script.text));
                }
            }

            Button addScriptButton = new(() => StartCreatingScript(id, data, name, false))
            {
                text = "Add Script"
            };
            box.Add(addScriptButton);

            Button addTargetedScriptButton = new(() => StartCreatingScript(id, data, name, true))
            {
                text = "Add Targeted Script"
            };
            box.Add(addTargetedScriptButton);

            Button attachBlankButton = new(() =>
            {
                if (AttachResourcePath(id, "CardData/Scripts/Blank")) ShowCard(box, name);
            })
            {
                text = "Attach Blank.cs"
            };
            box.Add(attachBlankButton);

            ObjectField scriptField = new("Existing script")
            {
                objectType = typeof(MonoScript),
                allowSceneObjects = false
            };
            box.Add(scriptField);

            Button attachExistingButton = new(() =>
            {
                MonoScript script = scriptField.value as MonoScript;
                if (script == null)
                {
                    Debug.LogError("Select a script to attach first.");
                    return;
                }

                string assetPath = AssetDatabase.GetAssetPath(script).Replace('\\', '/');
                const string resourcesPrefix = "Assets/Resources/";
                if (!assetPath.StartsWith(resourcesPrefix, StringComparison.Ordinal) || !assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("Attached card scripts must be C# files inside Assets/Resources.");
                    return;
                }

                string resourcePath = assetPath[resourcesPrefix.Length..^3];
                if (AttachResourcePath(id, resourcePath)) ShowCard(box, name);
            })
            {
                text = "Attach Selected Script"
            };
            box.Add(attachExistingButton);
        }

        private static void StartCreatingScript(int id, CardData.CardData data, string name, bool targeted)
        {
            var endNameEditHandler = CreateInstance<EndNameEditHandler>();
            endNameEditHandler.Init(id, targeted);
            string path = "Assets/Resources/CardData/Scripts/" + CDJsonUtils.expansionMapping[data.expansion] + "/" + name + ".cs";
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endNameEditHandler, path, null, null);
        }

        private static bool AttachResourcePath(int id, string resourcePath)
        {
            if (!CDJsonUtils.Scriptpaths.TryGetValue(id, out List<string> paths))
            {
                paths = new List<string>();
                CDJsonUtils.Scriptpaths.Add(id, paths);
            }

            if (paths.Contains(resourcePath))
            {
                Debug.LogWarning($"Card {id} already has script {resourcePath} attached.");
                return false;
            }

            paths.Add(resourcePath);
            List<string> displayedPaths = CardDatabase[id].scripts;
            if (!ReferenceEquals(paths, displayedPaths) && !displayedPaths.Contains(resourcePath))
                displayedPaths.Add(resourcePath);

            SaveScriptPaths();
            return true;
        }

        private static void SaveScriptPaths()
        {
            string json = MiniJson.JsonEncode(CDJsonUtils.Scriptpaths);
            File.WriteAllText(AssetDatabase.GUIDToAssetPath(jsonGUID), json);
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(jsonGUID));
        }

        class EndNameEditHandler : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            int id;
            bool targeted;
            public void Init(int id, bool targeted)
            {
                this.id = id;
                this.targeted = targeted;
            }
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                const string templatePath = "Assets/EditorExt/CardScriptTemplate.txt";
                const string targetedAddonPath = "Assets/EditorExt/TargettedCardScriptTemplateAddon.txt";
                string templateContent = File.ReadAllText(templatePath);
                if (templateContent.Length == 0)
                {
                    Debug.LogError("Empty template recieved");
                    return;
                }
                var classname = CDJsonUtils.SanitizeToClassName(Path.GetFileNameWithoutExtension(pathName));
                templateContent = templateContent.Replace("#NAME#", classname);
                templateContent = templateContent.Replace("#BASE_CLASS#", targeted ? "TargetableCardScriptBase" : "CardScriptBase");
                string targetedSection = targeted ? File.ReadAllText(targetedAddonPath).TrimEnd() : string.Empty;
                templateContent = templateContent.Replace("#TARGETED_SECTION#", targetedSection);
                File.WriteAllText(pathName, templateContent);
                AssetDatabase.ImportAsset(pathName);
                MonoScript newScript = AssetDatabase.LoadAssetAtPath<MonoScript>(pathName);
                string Resourcepath = pathName.Replace("Assets/Resources/", "");
                Resourcepath = Resourcepath.Replace(".cs", "");
                AttachResourcePath(id, Resourcepath);
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
