﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KodAdıAfacanlar.Views
{
    public class LessonView : UserControl
    {
        public LessonView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}