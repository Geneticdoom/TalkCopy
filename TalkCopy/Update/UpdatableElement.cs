using Dalamud.Plugin.Services;
using TalkCopy.Core.Handlers;
using TalkCopy.Update.Interfaces;

namespace TalkCopy.Update;

internal abstract class UpdatableElement : IUpdatableElement
{
    public UpdatableElement() => PluginHandlers.Framework.Update += Update;
    ~UpdatableElement() => PluginHandlers.Framework.Update -= Update;

    public abstract void Update(IFramework framework);
}
