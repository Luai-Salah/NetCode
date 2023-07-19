using System.Diagnostics;
using AsteroidsDamage;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Xedrial.Physics.b2D.Components;

namespace Xedrial.NetCode.Server.Systems
{
    //Asteroid spawning will occur on the server
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AsteroidSpawnSystem : SystemBase
    {
        //This will be our query for Asteroids
        private EntityQuery m_AsteroidQuery;

        //This will be our query to find GameSettingsComponent data to know how many and where to spawn Asteroids
        private EntityQuery m_GameSettingsQuery;

        //This will save our Asteroid prefab to be used to spawn Asteroids
        private Entity m_Prefab;

        //This is the query for checking network connections with clients
        private EntityQuery m_ConnectionGroup;

        private BeginSimulationEntityCommandBufferSystem m_BeginEcb;
        
        protected override void OnCreate()
        {
            m_BeginEcb = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
            
            //This is an EntityQuery for our Asteroids, they must have an AsteroidTag
            m_AsteroidQuery = GetEntityQuery(ComponentType.ReadWrite<AsteroidTag>());

            //This will grab the BeginSimulationEntityCommandBuffer system to be used in OnUpdate
            World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //This is an EntityQuery for the GameSettingsComponent which will drive how many Asteroids we spawn
            m_GameSettingsQuery = GetEntityQuery(ComponentType.ReadWrite<GameSettings>());

            //This says "do not go to the OnUpdate method until an entity exists that meets this query"
            //We are using GameObjectConversion to create our GameSettingsComponent so we need to make sure 
            //The conversion process is complete before continuing
            RequireForUpdate(m_GameSettingsQuery);

            //This will be used to check how many connected clients there are
            //If there are no connected clients the server will not spawn asteroids to save CPU
            m_ConnectionGroup = GetEntityQuery(ComponentType.ReadWrite<NetworkStreamConnection>());
        }

        protected override void OnUpdate()
        {
            //Here we check the amount of connected clients
            if (m_ConnectionGroup.IsEmptyIgnoreFilter)
            {
                // No connected players, just destroy all asteroids to save CPU
                EntityManager.DestroyEntity(m_AsteroidQuery);
                return;
            }

            //Here we set the prefab we will use
            if (m_Prefab == Entity.Null)
            {
                //We grab the converted PrefabCollection Entity's AsteroidAuthoringComponent
                //and set m_Prefab to its Prefab value
                m_Prefab = SystemAPI.GetSingleton<AsteroidAuthoringComponent>().Prefab;
                //we must "return" after setting this prefab because if we were to continue into the Job
                //we would run into errors because the variable was JUST set (ECS funny business)
                //comment out return and see the error
                return;
            }

            // Because of how ECS works we must declare local variables that will be used within the job
            //You cannot "GetSingleton<GameSettingsComponent>()" from within the job, must be declared outside
            var settings = SystemAPI.GetSingleton<GameSettings>();

            //This provides the current amount of Asteroids in the EntityQuery
            int count = m_AsteroidQuery.CalculateEntityCountWithoutFiltering();

            //We must declare our prefab as a local variable (ECS funny business)
            Entity asteroidPrefab = m_Prefab;

            //We will use this to generate random positions
            var rand = new Random((uint)Stopwatch.GetTimestamp());
            EntityCommandBuffer ecb = m_BeginEcb.CreateCommandBuffer();
            
            Job.WithCode(() =>
            {
                for (int i = count; i < settings.NumberOfAsteroids; ++i)
                {
                    // this is how much within perimeter asteroids start
                    const float padding = 0.1f;

                    // we are going to have the asteroids start on the perimeter of the level
                    // choose the x, y, z coordinate of perimeter
                    // so the x value must be from negative levelWidth/2 to positive levelWidth/2 (within padding)
                    float xPosition = rand.NextFloat(-1f * (settings.LevelWidth / 2f - padding),
                        settings.LevelWidth / 2f - padding);
                    // so the y value must be from negative levelHeight/2 to positive levelHeight/2 (within padding)
                    float yPosition = rand.NextFloat(-1f * (settings.LevelHeight / 2f - padding),
                        settings.LevelHeight / 2f - padding);

                    //We now have xPosition, yPosition, zPosition in the necessary range
                    //With "chooseFace" we will decide which face of the cube the Asteroid will spawn on
                    float chooseFace = rand.NextFloat(0, 4);

                    switch (chooseFace)
                    {
                        //Based on what face was chosen, we x, y or z to a perimeter value
                        //(not important to learn ECS, just a way to make an interesting pre-spawned shape)
                        case 0:
                            xPosition = -1 * (settings.LevelWidth / 2f - padding);
                            break;
                        case 1:
                            xPosition = settings.LevelWidth / 2f - padding;
                            break;
                        case 2:
                            yPosition = -1 * (settings.LevelHeight / 2f - padding);
                            break;
                        case 3:
                            yPosition = settings.LevelHeight / 2f - padding;
                            break;
                    }

                    //we then create a new translation component with the randomly generated x, y, and z values                
                    var transform = LocalTransform.Identity;
                    transform.Position = new float3(xPosition, yPosition, 0.0f);
                    
                    //on our command buffer we record creating an entity from our Asteroid prefab
                    Entity e = ecb.Instantiate(asteroidPrefab);

                    //we then set the Translation component of the Asteroid prefab equal to our new translation component
                    ecb.SetComponent(e, transform);

                    //We will now set the PhysicsVelocity of our asteroids
                    //here we generate a random Vector3 with x, y and z between -1 and 1
                    var randomVel = new float2(rand.NextFloat(-1f, 1f), rand.NextFloat(-1f, 1f));
                    //next we normalize it so it has a magnitude of 1
                    randomVel = math.normalize(randomVel);
                    //now we set the magnitude equal to the game settings
                    randomVel *= settings.AsteroidVelocity;
                    //here we create a new VelocityComponent with the velocity data
                    ecb.SetComponent(e, new PhysicsVelocity2D
                    {
                        Linear = randomVel
                    });
                }
            }).Schedule();
            
            m_BeginEcb.AddJobHandleForProducer(Dependency);
        }
    }
}
