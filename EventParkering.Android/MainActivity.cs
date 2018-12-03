﻿using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FFImageLoading.Forms.Droid;
using Prism;
using Prism.Ioc;
using EventParkering.ViewModel;
using Xamarin.Forms;
using EventParkering.Services;

namespace EventParkering.Droid
{
    [Activity(Label = "EventParkering", Icon = "@mipmap/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.Window.RequestFeature(WindowFeatures.ActionBar);
            base.SetTheme(Resource.Style.MainTheme);

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CachedImageRenderer.Init();
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            LoadApplication(new App(new AndroidInitializer()));
        }

        public class AndroidInitializer : IPlatformInitializer
        {
            public void RegisterTypes(IContainerRegistry containerRegistry)
            {
                containerRegistry.RegisterForNavigation<NavigationPage>();
                containerRegistry.RegisterForNavigation<View.MainPage, MainPageViewModel>();
                containerRegistry.RegisterForNavigation<View.EventPage, EventPageViewModel>();
                containerRegistry.RegisterForNavigation<View.ParkPage, ParkPageViewModel>();
                containerRegistry.Register<IRestService, RestService>();
            }
        }
    }
}