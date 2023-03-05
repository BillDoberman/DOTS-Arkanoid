﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup), OrderFirst = true)]
public partial class BallSpawnCheatInputProcessingSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
        RequireForUpdate<GameData>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        var gameData = SystemAPI.GetSingleton<GameData>();
        var randomSeed = (uint)System.Environment.TickCount;
        
        Entities
            .WithNone<LaserPaddleTag>()
            .ForEach((Entity paddle, ref PaddleInputData inputData, in LocalTransform transform, in OwnerPlayerId ownerPlayerId) =>
            {
                if (inputData.Action == InputActionType.SpawnBallCheat)
                {
                    ecb.AddSingleFrameComponent(new BallSpawnRequest
                    {
                        Position = transform.Position + new float3(0, 1, 0),
                        OwnerPaddle = paddle,
                        OwnerPlayer = ownerPlayerId.Value,
                        StuckToPaddle = false,
                        Velocity = BallsHelper.GetRandomDirection(new Random(randomSeed)) * gameData.BallSpeed
                    });
                }
            }).Schedule();

        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}