using System;

#nullable enable
namespace NextUIOverlay.Socket;

[Serializable]
public class SocketEvent
{
    public string guid = "";
    public string type = "";
    public string message = "";
    public SocketRequest? request;
    public bool accept;
}