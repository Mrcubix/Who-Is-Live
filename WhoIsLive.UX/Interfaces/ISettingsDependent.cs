using WhoIsLive.UX.Entities;

namespace WhoIsLive.Lib.Interfaces;

public interface ISettingsDependent
{
    Settings Settings { get; set; }
}