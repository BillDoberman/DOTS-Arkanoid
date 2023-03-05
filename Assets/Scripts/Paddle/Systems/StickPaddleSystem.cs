﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(BallBlockPaddleSystemGroup))]
public partial class StickPaddleSystem : SystemBase
{
    private const float StuckTimeLimit = 3.0f;
    
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities
            .WithAll<StickPaddleTag>()
            .ForEach((Entity paddle, in LocalTransform paddleTransform,
                in HitByBallEvent hitByBallEvent, in DynamicBuffer<BallLink> ballsBuffer) =>
            {
                var ballData = SystemAPI.GetComponent<BallData>(hitByBallEvent.Ball);
                if (ballData.OwnerPaddle == paddle)
                {
                    bool hasAny = false;
                    for (int i = 0; i < ballsBuffer.Length; i++)
                        hasAny |= SystemAPI.HasComponent<BallStuckToPaddle>(ballsBuffer[i].Ball);
                    if (!hasAny)
                    {
                        var ballTransform = SystemAPI.GetComponent<LocalTransform>(hitByBallEvent.Ball);
                        var paddleData = SystemAPI.GetComponent<PaddleData>(paddle);
                        float stickOffset = ballTransform.Position.x - paddleTransform.Position.x;
                        float stickSide = paddleData.Size.x * 0.7f;
                        ecb.AddComponent(hitByBallEvent.Ball, new BallStuckToPaddle {
                            StuckTime = StuckTimeLimit,
                            Offset = math.clamp(stickOffset, -stickSide / 2, stickSide / 2)
                        });
                    }
                }
            }).Schedule();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}