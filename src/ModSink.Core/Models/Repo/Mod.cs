﻿using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public class Mod
    {
        public IDictionary<string, HashValue> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}