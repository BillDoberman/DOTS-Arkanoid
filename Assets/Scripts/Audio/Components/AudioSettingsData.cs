using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public enum AudioClipKeys
{
    BallLoss,
    PowerUpReceived,
    BlockDamage,
    BlockDestroy,
    GameOver,
    PaddleHit,
    PaddleCatch,
    RoundCleared,
    RoundStart,
    LaserShot
}

[Serializable]
public struct AudioClipEntry
{
    public AudioClipKeys Key;
    public AudioClip Clip;
}

public class AudioSettingsData : IComponentData
{
    public Dictionary<AudioClipKeys, AudioClip> Clips;
}