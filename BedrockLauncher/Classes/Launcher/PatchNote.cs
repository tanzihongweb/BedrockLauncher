﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockLauncher.Classes.Launcher
{
    public class PatchNote
    {

        public string FallbackImage
        {
            get
            {
                if (isBeta) return Constants.PATCHNOTE_BETA_IMG;
                else return Constants.INSTALLATIONS_LATEST_RELEASE_ICONPATH;
            }
        }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Tag { get => (isBeta ? "Beta" : "Release"); }
        public string Url { get; set; }
        public System.Version Version { get; set; }
        public bool isBeta { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public string PublishingDateString { get; set; } = string.Empty;

        public PatchNote()
        {

        }

    }
}
