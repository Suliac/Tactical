using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct GridComponent : IComponentData
{
    public uint GridWidth;
    public uint GridLength;
}

