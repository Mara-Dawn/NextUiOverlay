using System;

#nullable disable
namespace NextUIOverlay.Data.Input;

[Flags]
public enum InputModifier
{
    None = 0,
    Shift = 1,
    Control = 2,
    Alt = 4,
    MouseLeft = 8,
    MouseRight = 16, // 0x00000010
    MouseMiddle = 32, // 0x00000020
}