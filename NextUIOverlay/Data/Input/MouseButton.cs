using System;

#nullable disable
namespace NextUIOverlay.Data.Input;

[Flags]
public enum MouseButton
{
    None = 0,
    Primary = 1,
    Secondary = 2,
    Tertiary = 4,
    Fourth = 8,
    Fifth = 16, // 0x00000010
}