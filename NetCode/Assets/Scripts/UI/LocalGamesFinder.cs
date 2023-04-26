using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xedrial.NetCode.UI
{
    public class LocalGamesFinder : MonoBehaviour
    {
        //We will be pulling in our SourceAsset from TitleScreenUI GameObject so we can reference Visual Elements
        [SerializeField] private UIDocument m_TitleUIDocument;

        //When we grab the rootVisualElement of our UIDocument we will be able to query the TitleScreenManager Visual Element
        private VisualElement m_TitleScreenManagerVe;

        //We will query for our TitleScreenManager cVE by its name "TitleScreenManager"
        private TitleScreenManager m_TitleScreenManagerClass;

        //Within TitleScreenManager (which is everything) we will query for our list-view by name
        //We don't have to query for the TitleScreen THEN list-view because it is one big tree of elements
        //We can call any child from the parent, very convenient! But you must be mindful about being diligent about
        //creating unique names or else you can get back several elements (which at times is the point of sharing a name)
        private ListView m_ListView;

        //Although this variable name doesn't make sense for this use case it will in the Multiplayer section
        //Here we will store our discovered GameObjects
        //This will be used as our itemSource
        private GameObject[] m_DiscoveredServerInfoObjects;

        //This is our ListItem uxml that we will drag to the public field
        //We need a reference to the uxml so we can build it in makeItem
        [SerializeField] private VisualTreeAsset m_LocalGameListItemAsset;

        //These variables are used in Update() to pace how often we check for GameObjects
        [SerializeField] private float m_PerSecond = 1.0f;
        private float m_NextTime;

        private void OnEnable()
        {
            //Here we grab the SourceAsset rootVisualElement
            //This is a MAJOR KEY, really couldn't find this key step in information online
            //If you want to reference your active UI in a script make a public UIDocument variable and 
            //then call rootVisualElement on it, from there you can query the Visual Element tree by names
            //or element types
            m_TitleScreenManagerVe = m_TitleUIDocument.rootVisualElement;
            //Here we grab the TitleScreenManager by querying by name
            m_TitleScreenManagerClass = m_TitleScreenManagerVe
                .Q<TitleScreenManager>("TitleScreenManager");
            //From within TitleScreenManager we query local-games-list by name
            m_ListView = m_TitleScreenManagerVe.Q<ListView>("local-games-list");

        }

        private void Start()
        {
            //We start by looking for any GameObjects with a LocalGame tag
            m_DiscoveredServerInfoObjects = GameObject.FindGameObjectsWithTag("LocalGame");

            // The three spells you must cast to conjure a list view
            m_ListView.makeItem = MakeItem;
            m_ListView.bindItem = BindItem;
            m_ListView.itemsSource = m_DiscoveredServerInfoObjects;
            m_ListView.onSelectedIndicesChange += ints =>
            {
                int[] selection = ints.ToArray();
                if (selection is { Length: <= 0 })
                    return;
                
                ClickedJoinGame(m_DiscoveredServerInfoObjects[selection![0]]);
                m_ListView.ClearSelection();
            };
        }

        private VisualElement MakeItem()
        {
            //Here we take the uxml and make a VisualElement
            VisualElement listItem = m_LocalGameListItemAsset.CloneTree();
            return listItem;
        }

        private void BindItem(VisualElement e, int index)
        {
            //We add the game name to the label of the list item
            e.Q<Label>("game-name").text = m_DiscoveredServerInfoObjects[index].name;
        }

        private void ClickedJoinGame(GameObject localGame)
        {
            if (!localGame)
                return;
            
            //We query our JoinGameScreen cVE and call a new function LoadJoinScreenForSelectedServer and pass our GameObject
            //This is an example of clicking a list item and passing through data to a new function with that click
            //You will see in our JoinGameScreen cVE that we use this data to fill labels in the view
            m_TitleScreenManagerClass
                .Q<JoinGameScreen>("JoinGameScreen")
                .LoadJoinScreenForSelectedServer(localGame);

            //We then call EnableJoinScreen on our TitleScreenManager cVE (which displays JoinGameScreen)
            m_TitleScreenManagerClass.EnableJoinScreen();
        }
  
        // Update is called once per frame
        private void Update()
        {
            if (m_NextTime < Time.time)
                return;
            
            //We check for GameObjects with a localGame tag
            m_DiscoveredServerInfoObjects = GameObject.FindGameObjectsWithTag("LocalGame");

            //We again set our itemsSource to our array (if the array changes it must be reset)
            m_ListView.itemsSource = m_DiscoveredServerInfoObjects;
            //We then must refresh the listView on this new data source
            //(don't worry it doesn't make the list jump, ListView is cool like that)
            m_ListView.Rebuild();

            //We increment
            m_NextTime += 1 / m_PerSecond;
        }

    }
}