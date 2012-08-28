﻿using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using CrossBox.Core.ViewModels;

namespace CrossBox.Core
{
    public class StartNavigation
        : MvxApplicationObject,
        IMvxStartNavigation
    {
        public void Start()
        {
            RequestNavigate<MainMenuViewModel>();
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }
}
