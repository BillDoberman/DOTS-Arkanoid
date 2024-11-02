using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(GameStateSystemGroup))]
public partial struct ContinuePromptSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateData>();
        state.RequireForUpdate<GameData>();
        state.RequireForUpdate<ContinuePromptState>();
    }
    
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {        
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        gameState.StateTimer = 5.0f;
        SystemAPI.SetSingleton(gameState);

        foreach (var (playerData, player) in SystemAPI.Query<RefRW<PlayerData>>().WithEntityAccess())
        {
            playerData.ValueRW.Lives = 3;
        }

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        ecb.AddSingleFrameComponent(new LevelDespawnRequest());
        ecb.DestroyEntity(SystemAPI.QueryBuilder().WithAll<PlayerData>().Build(), EntityQueryCaptureMode.AtPlayback);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameState = SystemAPI.GetSingleton<GameStateData>();

        if (gameState.StateTimer > 0)
        {
            gameState.StateTimer -= SystemAPI.Time.DeltaTime;
            SystemAPI.SetSingleton(gameState);
        }
        else
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            ecb.AddSingleFrameComponent(ChangeStateCommand.Create<GameOverState>());
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}