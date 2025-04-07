using System;
using System.ComponentModel;
using System.Linq;
using GfxMan.Services.Model;

namespace GfxManWpf;

public class GameInfoVm(GameInfo info)
{
    public Guid Id { get; set; } = info.Id;
    public string Name { get; set; } = info.Name;

    public BindingList<string> SettingsFiles { get; set; } = new(info.SettingsFiles);

    public GameInfo ToGameInfo()
    {
        return new GameInfo()
        {
            Id = Id,
            Name = Name,
            SettingsFiles = SettingsFiles.ToList(),
        };
    }
}