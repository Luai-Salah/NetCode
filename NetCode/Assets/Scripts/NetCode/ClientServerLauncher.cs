using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using Unity.Entities;
using Unity.NetCode;

using Xedrial.NetCode.UI;

namespace Xedrial.NetCode
{
    public class ClientServerLauncher : MonoBehaviour
    {
        // These are the variables that will get us access to the UI views 
        // This is how we can grab active UI into a script
        // If this is confusing checkout the "Making a List" page in the gitbook
    
        // This is the UI Document from the Hierarchy in NavigationScene
        [SerializeField] private UIDocument m_TitleUIDocument;
        
        private VisualElement m_TitleScreenManagerVe;
        // These variables we will set by querying the parent UI Document
        private HostGameScreen m_HostGameScreen;
        private JoinGameScreen m_JoinGameScreen;
        private ManualConnectScreen m_ManualConnectScreen;

        private void OnEnable()
        {
            // Here we set our variables for our different views so we can then add call backs to their buttons
            m_TitleScreenManagerVe = m_TitleUIDocument.rootVisualElement;
            m_HostGameScreen = m_TitleScreenManagerVe.Q<HostGameScreen>("HostGameScreen");
            m_JoinGameScreen = m_TitleScreenManagerVe.Q<JoinGameScreen>("JoinGameScreen");
            m_ManualConnectScreen = m_TitleScreenManagerVe.Q<ManualConnectScreen>("ManualConnectScreen");

            // Host Game Screen callback
            m_HostGameScreen.Q("launch-host-game")?
                .RegisterCallback<ClickEvent>(OnClickedHostGame);
            
            // Join Game Screen callback
            m_JoinGameScreen.Q("launch-join-game")?
                .RegisterCallback<ClickEvent>(OnClickedJoinGame);
            
            // Manual Connect Screen callback
            m_ManualConnectScreen.Q("launch-connect-game")?
                .RegisterCallback<ClickEvent>(OnClickedJoinGame);
        }
        
        private void OnClickedHostGame(ClickEvent _)
        {
            //When we click "Host Game" that means we want to be both a server and a client
            //So we will trigger both functions for the server and client
            ServerLauncher();
            ClientLauncher();

            //This function will trigger the MainScene
            StartGameScene();
        }

        private void OnClickedJoinGame(ClickEvent _)
        {
            //When we click 'Join Game" that means we want to only be a client
            //So we do not trigger ServerLauncher
            ClientLauncher();

            //This function triggers the MainScene
            StartGameScene();
        }
        
        private static void ServerLauncher()
        {
            //CreateServerWorld is a method provided by ClientServerBootstrap for precisely this reason
            //Manual creation of worlds

            //We must grab the DefaultGameObjectInjectionWorld first as it is needed to create our ServerWorld
            var world = World.DefaultGameObjectInjectionWorld;
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
            ClientServerBootstrap.CreateServerWorld(world, "ServerWorld");
#endif
        }

        private static void ClientLauncher()
        {
            // First we grab the DefaultGameObjectInjectionWorld because it is needed to create ClientWorld
            var world = World.DefaultGameObjectInjectionWorld;

            // We have to account for the fact that we may be in the Editor and using ThinClients
            // We initially start with 1 client world which will not change if not in the editor
            const int numClientWorlds = 1;
            int totalNumClients = numClientWorlds;

            // If in the editor we grab the amount of ThinClients from ClientServerBootstrap class (it is a static variable)
            // We add that to the total amount of worlds we must create
#if UNITY_EDITOR
            int numThinClients = ClientServerBootstrap.RequestedNumThinClients;
            totalNumClients += numThinClients;
#endif
            // We create the necessary number of worlds and append the number to the end
            for (int i = 0; i < numClientWorlds; ++i)
                ClientServerBootstrap.CreateClientWorld(world, "ClientWorld" + i);
#if UNITY_EDITOR
            for (int i = numClientWorlds; i < totalNumClients; ++i)
            {
                World clientWorld = ClientServerBootstrap.CreateClientWorld(world, "ClientWorld" + i);
                clientWorld.EntityManager.CreateEntity(typeof(ThinClientComponent));
            }
#endif
        }

        private static void StartGameScene()
        {
            //Here we trigger MainScene
#if UNITY_EDITOR
            if(Application.isPlaying)
#endif
                SceneManager.LoadSceneAsync("MainScene");
#if UNITY_EDITOR
            else Debug.Log("Loading: " + "MainScene");
#endif
        }
    }
}