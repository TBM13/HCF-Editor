﻿using HCF_Editor.Samsung;

namespace HCF_Editor.Common
{
    public class Events
    {
        public delegate void Event();
        public delegate void ObjectEvent(object obj);
        public delegate void StringEvent(string arg1);

        public delegate void MIBEntryEvent(MIBEntry entry);
    }
}
