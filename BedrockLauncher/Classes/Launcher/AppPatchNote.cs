﻿using BedrockLauncher.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BedrockLauncher.Classes.Launcher
{
    public class AppPatchNote : UpdateNote
    {
        public bool isLatest { get; set; } = false;
        public SolidColorBrush Title_Foreground
        {
            get
            {
                if (isBeta) return Brushes.DarkOrange;
                else return Brushes.Gray;
            }
        }
        public Visibility CurrentBox_Visibility => isLatest ? Visibility.Visible : Visibility.Collapsed;

        public AppPatchNote(UpdateNote original) : base(original)
        {

        }

        public AppPatchNote()
        {

        }
    }
}
