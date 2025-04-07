using System;
using GfxMan.Services.Model;

namespace GfxManWpf;

public class GameInfoEventArgs(GameInfo info) : EventArgs
{
    public GameInfo GameInfo { get; } = info;
}