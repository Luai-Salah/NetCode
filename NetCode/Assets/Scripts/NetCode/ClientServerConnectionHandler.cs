using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Xedrial.NetCode
{
    public class ClientServerConnectionHandler : MonoBehaviour
    {
        //this is the store of server/client info
        public ClientServerInfo m_ClientServerInfo;

        // these are the launch objects from Navigation scene that tells what to set up
        private GameObject[] m_LaunchObjects;

        //these will gets access to the UI views 
        public UIDocument m_GameUIDocument;
        private VisualElement m_GameManagerUIVe;

        private void OnEnable()
        {
            // This will put callback on "Quit Game" button
            // This triggers the clean up function (ClickedQuitGame)
            m_GameManagerUIVe = m_GameUIDocument.rootVisualElement;
            m_GameManagerUIVe.Q("quit-game")?
                .RegisterCallback<ClickEvent>(ClickedQuitGame);
        }

        private void Awake()
        {
            m_LaunchObjects = GameObject.FindGameObjectsWithTag("LaunchObject");
            foreach (GameObject launchObject in m_LaunchObjects)
            {
                // 
                // checks for server launch object
                // does set up for the server for listening to connections and player scores
                //
                if (launchObject.GetComponent<ServerLaunchObjectData>() != null)
                {
                    //sets the gameObject server data (mono)
                    m_ClientServerInfo.IsServer = true;
                
                    // sets the component server data in server world(dots)
                    // ClientServerConnectionControl (server) will run in server world
                    // it will pick up this component and use it to listen on the port
                    foreach (World world in World.All)
                    {
                        // we cycle through all the worlds, and if the world has ServerSimulationSystemGroup
                        // we move forward (because that is the server world)
                        if (world.GetExistingSystem<ServerSimulationSystemGroup>() == null)
                            continue;
                        
                        Entity serverDataEntity = world.EntityManager.CreateEntity();
                        world.EntityManager.AddComponentData(serverDataEntity, new ServerDataComponent
                        {
                            GamePort = m_ClientServerInfo.GamePort
                        });
                        // create component that allows server initialization to run
                        world.EntityManager.CreateEntity(typeof(InitializeServerComponent));
                    }
                }

                // 
                // checks for client launch object
                //  does set up for client for dots and mono
                // 
                var clientLaunchObject = launchObject.GetComponent<ClientLaunchObjectData>();
                if (!clientLaunchObject)
                    continue;
                
                //sets the gameObject data in ClientServerInfo (mono)
                //sets the gameObject data in ClientServerInfo (mono)
                m_ClientServerInfo.IsClient = true;
                m_ClientServerInfo.ConnectToServerIp = clientLaunchObject.IPAddress;                

                //sets the component client data in server world(dots)
                //ClientServerConnectionControl (client) will run in client world
                //it will pick up this component and use it connect to IP and port
                foreach (World world in World.All)
                {
                    //we cycle through all the worlds, and if the world has ClientSimulationSystemGroup
                    //we move forward (because that is the client world)
                    if (world.GetExistingSystem<ClientSimulationSystemGroup>() == null)
                        continue;
                    
                    Entity clientDataEntity = world.EntityManager.CreateEntity();
                    world.EntityManager.AddComponentData(clientDataEntity, new ClientDataComponent
                    {
                        ConnectToServerIp = m_ClientServerInfo.ConnectToServerIp,
                        GamePort = m_ClientServerInfo.GamePort
                    });
                    //create component that allows client initialization to run
                    world.EntityManager.CreateEntity(typeof(InitializeClientComponent));
                }
            }
        }
        
        //This function will navigate us to NavigationScene
        private static void ClickedQuitGame(ClickEvent _)
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
#endif
                SceneManager.LoadSceneAsync("NavigationScene");
#if UNITY_EDITOR
            else Debug.Log("Loading: " + "NavigationScene");
#endif
        }

        // When the OnDestroy method is called (because of our transition to NavigationScene) we
        // must delete all our entities and our created worlds to go back to a blank state
        // This way we can move back and forth between scenes and "start from scratch" each time
        private void OnDestroy()
        {
            // This query deletes all entities
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
            // This query deletes all worlds
            World.DisposeAllWorlds();

            // We return to our initial world that we started with, defaultWorld
            var bootstrap = new NetCodeBootstrap();
            bootstrap.Initialize("DefaultWorld");
        }
    }
}