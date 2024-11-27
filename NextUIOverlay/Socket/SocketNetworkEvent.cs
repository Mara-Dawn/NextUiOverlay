using System.Runtime.InteropServices;

#nullable enable
namespace NextUIOverlay.Socket;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SocketNetworkEvent
{
    public const string CastStart = "castStart";
    public const string Gauge = "gauge";
    public const string NpcSpawn = "npcSpawn";
}