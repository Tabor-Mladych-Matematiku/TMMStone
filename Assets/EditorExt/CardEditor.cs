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
        VisualElement cardGrid;
        ScrollView cardGridScroll;
        Box cardDetails;
        Button deselectButton;
        Toggle unfinishedToggle;
        Toggle finishedToggle;
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

            cardDetails = new Box();
            choices = new List<string>();
            foreach (var item in CardDatabase)
            {
                choices.Add(item.Value.name);
                nameToId.Add(item.Value.name, item.Key);
            }

            deselectButton = new(ShowCardGrid)
            {
                text = "Deselect card"
            };
            root.Add(deselectButton);

            unfinishedToggle = new Toggle("Show unfinished only");
            finishedToggle = new Toggle("Show finished only");
            unfinishedToggle.RegisterValueChangedCallback(value =>
            {
                if (value.newValue) finishedToggle.SetValueWithoutNotify(false);
                RebuildCardGrid();
            });
            finishedToggle.RegisterValueChangedCallback(value =>
            {
                if (value.newValue) unfinishedToggle.SetValueWithoutNotify(false);
                RebuildCardGrid();
            });
            root.Add(unfinishedToggle);
            root.Add(finishedToggle);

            cardGridScroll = new ScrollView(ScrollViewMode.Vertical);
            cardGridScroll.style.flexGrow = 1;
            cardGrid = new VisualElement();
            cardGrid.style.width = Length.Percent(100);
            cardGrid.style.flexShrink = 0;
            cardGrid.style.flexDirection = FlexDirection.Row;
            cardGrid.style.flexWrap = Wrap.Wrap;
            cardGrid.style.alignContent = Align.FlexStart;
            cardGridScroll.Add(cardGrid);
            root.Add(cardGridScroll);
            root.Add(cardDetails);

            ShowCardGrid();
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

        private void ShowCardGrid()
        {
            deselectButton.style.display = DisplayStyle.None;
            unfinishedToggle.style.display = DisplayStyle.Flex;
            finishedToggle.style.display = DisplayStyle.Flex;
            cardGridScroll.style.display = DisplayStyle.Flex;
            cardDetails.style.display = DisplayStyle.None;
            RebuildCardGrid();
        }

        private void RebuildCardGrid()
        {
            cardGridScroll.scrollOffset = Vector2.zero;
            cardGrid.Clear();
            IEnumerable<string> visibleCards = choices;
            if (unfinishedToggle.value)
                visibleCards = visibleCards.Where(CheckFilter);
            else if (finishedToggle.value)
                visibleCards = visibleCards.Where(card => !CheckFilter(card));

            foreach (string cardName in visibleCards)
            {
                CardData.CardData data = CardDatabase[nameToId[cardName]];
                Texture2D cardImage = LoadCardImage(data, cardName);

                Button cardButton = new(() => SelectCard(cardName));
                cardButton.tooltip = cardName;
                cardButton.style.width = 160;
                cardButton.style.height = 230;
                cardButton.style.marginLeft = 4;
                cardButton.style.marginRight = 4;
                cardButton.style.marginTop = 4;
                cardButton.style.marginBottom = 4;
                cardButton.style.flexDirection = FlexDirection.Column;

                Image image = new()
                {
                    image = cardImage,
                    scaleMode = ScaleMode.ScaleToFit
                };
                image.style.width = 150;
                image.style.height = 195;
                cardButton.Add(image);
                cardButton.Add(new Label(cardName));
                cardGrid.Add(cardButton);
            }
        }

        private void SelectCard(string cardName)
        {
            deselectButton.style.display = DisplayStyle.Flex;
            unfinishedToggle.style.display = DisplayStyle.None;
            finishedToggle.style.display = DisplayStyle.None;
            cardGridScroll.style.display = DisplayStyle.None;
            cardDetails.style.display = DisplayStyle.Flex;
            ShowCard(cardDetails, cardName);
        }

        private void ShowCard(Box box, string name)
        {
            box.Clear();
            int id = nameToId[name];
            CardData.CardData data = CardDatabase[id];
            box.Add(new Image()
            {
                image = LoadCardImage(data, name)
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
            string assetPath = AssetDatabase.GUIDToAssetPath(jsonGUID);
            File.WriteAllText(assetPath, FormatScriptPathsJson(CDJsonUtils.Scriptpaths), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath);
        }

        private static string FormatScriptPathsJson(Dictionary<int, List<string>> scriptPaths)
        {
            var json = new StringBuilder();
            json.AppendLine("{");

            var entries = scriptPaths.OrderBy(pair => pair.Key).ToList();
            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                KeyValuePair<int, List<string>> entry = entries[entryIndex];
                json.Append("    ")
                    .Append(MiniJson.JsonEncode(entry.Key.ToString()))
                    .AppendLine(": [");

                for (int pathIndex = 0; pathIndex < entry.Value.Count; pathIndex++)
                {
                    json.Append("        ")
                        .Append(MiniJson.JsonEncode(entry.Value[pathIndex]));
                    if (pathIndex < entry.Value.Count - 1) json.Append(',');
                    json.AppendLine();
                }

                json.Append("    ]");
                if (entryIndex < entries.Count - 1) json.Append(',');
                json.AppendLine();
            }

            json.AppendLine("}");
            return json.ToString();
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

        bool CheckFilter(string card)
        {
            if (CardDatabase[nameToId[card]].scripts.Count != 0) return false;
            return true;
        }

        private static Texture2D LoadCardImage(CardData.CardData data, string cardName)
        {
            const string tokenSuffix = " (token)";
            string artName = cardName.EndsWith(tokenSuffix, StringComparison.Ordinal)
                ? cardName[..^tokenSuffix.Length]
                : cardName;
            string expansion = CDJsonUtils.expansionMapping[data.expansion];
            string folder = $"Assets/CardArt/CardFaces/{expansion}";
            if (!Directory.Exists(folder)) return null;

            string imagePath = Directory.EnumerateFiles(folder)
                .FirstOrDefault(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(Path.GetFileNameWithoutExtension(path), artName, StringComparison.Ordinal));
            return imagePath == null
                ? null
                : AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath.Replace('\\', '/'));
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
