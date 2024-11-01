using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

namespace TalkCopy;

internal unsafe class Utils
{
    public unsafe bool FindNodeOfType(int type, Action<nint> callback, bool breakUponCorrect, AtkResNode* node, bool printSiblings = false)
    {
        if (node == null) return false;

        bool passed = false;

        if (type < 1000 && (int)node->Type < 1000) { passed = (int)node->Type == type; }
        else if(type >= 1000 && (int)node->Type >= 1000)
        {
            AtkComponentNode* compNode = (AtkComponentNode*)node;
            AtkUldManager componentInfo = compNode->Component->UldManager;
            AtkUldComponentInfo* objectInfo = (AtkUldComponentInfo*)componentInfo.Objects;
            if (objectInfo != null)
            {
                int compType = (int)objectInfo->ComponentType;
                compType += 1000;
                passed = type == compType;
            }
        }

        if (passed)
        {
            callback.Invoke((nint)node);
            if (breakUponCorrect)
            {
                return true;
            }
        }


        AtkResNode* prevNode = node;
        while ((prevNode = prevNode->PrevSiblingNode) != null)
        {
            if (printSiblings) 
            { 
                if (FindNodeOfType(type, callback, breakUponCorrect, prevNode) && breakUponCorrect)
                {
                    return true;
                }
            }
        }

        AtkResNode* nextNode = node;
        while ((nextNode = nextNode->NextSiblingNode) != null)
        {
            if (printSiblings)
            {
                if (FindNodeOfType(type, callback, breakUponCorrect, nextNode) && breakUponCorrect)
                {
                    return true;
                }
            }
        }

        if ((int)node->Type < 1000)
        {
            return FindNodeOfType(type, callback, breakUponCorrect, node->ChildNode, true);
        }
        else
        {
            AtkComponentNode* compNode = (AtkComponentNode*)node;
            AtkUldManager componentInfo = compNode->Component->UldManager;
            return FindNodeOfType(type, callback, breakUponCorrect, componentInfo.RootNode, true);
        }
    }
}
