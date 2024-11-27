using System;

#nullable enable
namespace NextUIOverlay.Socket;

[Serializable]
public class SocketRequest
{
    public uint id;
    public object? data;
    public string[]? events;
}