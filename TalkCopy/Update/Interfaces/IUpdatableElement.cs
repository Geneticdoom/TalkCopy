using Dalamud.Plugin.Services;

namespace TalkCopy.Update.Interfaces;

internal interface IUpdatableElement
{
    void Update(IFramework framework);
}
