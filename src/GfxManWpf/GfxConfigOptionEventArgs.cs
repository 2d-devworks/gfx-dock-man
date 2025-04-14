using System;
using GfxMan.Services.Model;

namespace GfxManWpf;

public class GfxConfigOptionEventArgs(GfxConfigOption option, string oldName): EventArgs
{
    public GfxConfigOption ConfigOption { get; } = option;
    public string OldName { get; } = oldName;
}