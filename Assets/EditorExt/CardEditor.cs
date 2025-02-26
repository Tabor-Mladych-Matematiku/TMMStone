#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CardData;
using UnityEditor.UIElements;
using System.Linq;

namespace CardEditor
{
    /*struct DropdownSlot
    {
        int id;
        string name;
        public DropdownSlot(int i, string s)
        {
            id = i;
            name = s;
        }
        public override string ToString()
        {
            return name + " "+id;
        }
        public static implicit operator string(DropdownSlot x)
        {
            return x.ToString();
        }
    }*/
    public class CardEditor : EditorWindow
    {
        Dictionary<int, CardData.CardData> CardDatabase;
        Dictionary<string, int> nameToId = new();
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
                MonoScript script = GetScript(name); 
                if(script == null)
                {
                    box.Add(new Label("Does not have script"));
                    box.Add(new Button(() => { Debug.Log("TODO: Create Script"); })
                    {
                        text = "Add Script"
                    });
                }
                else
                {
                    box.Add(new Label("Has script"));
                }
            });
            root.Add(cardList);
            Toggle t = new("Filter?");
            t.RegisterValueChangedCallback(value => FilterCardlist(value.newValue));
            root.Add(t);
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

        private MonoScript GetScript(string name)
        {
            CardData.CardData data = CardDatabase[nameToId[name]];
            return Resources.Load<MonoScript>("CardData/Scripts/" + CDJsonUtils.expansionMapping[data.expansion] + "/" + name);
        }

        private void FilterCardlist(bool filter)
        {
            cardList.choices = (from choice in choices where !filter || CheckFilter(choice) select choice).ToList();
        }
        bool CheckFilter(string card)
        {
            if (GetScript(card) != null) return false;
            return true;
        }
    }
}
#endif