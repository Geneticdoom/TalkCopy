using System;
using System.Reflection;
using TalkCopy.Attributes;
using TalkCopy.Automation.Interfaces;

namespace TalkCopy.Automation;

internal class Creator : ICreator
{
    public void Create()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        if (assembly == null) return;
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(ActiveAttribute), false).Length == 0) continue;
            Activator.CreateInstance(type);
        }
    }
}
