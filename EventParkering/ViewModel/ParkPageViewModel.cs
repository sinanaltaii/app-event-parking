﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using EventParkering.Model;
using EventParkering.Services;
using Plugin.Geolocator;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms.GoogleMaps;
using System.Collections.Generic;
using System.Linq;
using Prism.Services;
using Plugin.ExternalMaps;

namespace EventParkering.ViewModel
{
    public class ParkPageViewModel : BaseViewModel 
    {

        private readonly Dictionary<Pin, ParkItem> _pinVsParkItems = new Dictionary<Pin, ParkItem>();
        public DelegateCommand GoBack { get; set; }
        public DelegateCommand NavigateMe { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public Map Map { get; private set; }
        IPageDialogService _pageDialogService;
        ParkService _parkService;

        private double _minutes;
        public double Minutes
        {
            get
            {
                return _minutes;
            }
            set
            {
                _minutes = value;
                RaisePropertyChanged(nameof(Minutes));
            }
        }

        private bool _isPinVisible;
        public bool IsPinVisible 
        {
            get
            {
                return _isPinVisible;
            }
            set
            {
                _isPinVisible = value;
                RaisePropertyChanged(nameof(IsPinVisible));
            }
        }

        private string _newaddress;
        public string NewAddress
        {
            get
            {
                return _newaddress;
            }
            set
            {
                _newaddress = value;
                RaisePropertyChanged(nameof(NewAddress));
            }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private int _id;
        public int ID
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _lat;
        public string Lat
        {
            get { return _lat; }
            set { SetProperty(ref _lat, value); }
        }

        private string _lon;
        public string Lon
        {
            get { return _lon; }
            set { SetProperty(ref _lon, value); }
        }



        private EventItem _eventItem;
        public EventItem eventItem
        {
            get => _eventItem;
            set
            {
                _eventItem = value;
                Title = value.name;
                var address = value.streetAddress;
                Address = address + ", Helsingborg";
                ID = value.id;
                Lat = value.lat;
                Lon = value.lon;
            }
        }

        private ParkItem _selectedParkItem;
        public ParkItem SelectedParkItem
        {
            get => _selectedParkItem;
            set
            {
                _selectedParkItem = value;

                IsPinVisible = value != null; //Show/Hide Button
                if (value != null)
                {
                    NewAddress = value.name;
                    var distance = Convert.ToDouble(value?.dist);
                    var Sec = distance / 4.5;
                    var Min = Sec / 60;
                    if(Min < 1)
                    {
                        Minutes = 1;
                    }
                    else
                    {
                        double example = Min;
                        double output = Convert.ToDouble(example.ToString("N0"));
                        Minutes = output;
                    }
                }
            }
        }

        public ParkPageViewModel(INavigationService navigationService, ParkService parkService, IPageDialogService pageDialogService)
        : base(navigationService)
        {
            _parkService = parkService;
            _pageDialogService = pageDialogService;
            GoBack = new DelegateCommand(() =>
            {
                _navigationService.GoBackAsync();
            });

            Map = new Map();
            Map.InitialCameraUpdate = CameraUpdateFactory.NewCameraPosition(new CameraPosition(new Position(56.04673, 12.69437), 15d));
            Map.MyLocationEnabled = true;


            NavigateMe = new DelegateCommand(() =>
            {
                if (SelectedParkItem != null)
                {
                    //Open the map based on Selected ParkItem
                    CrossExternalMaps.Current.NavigateTo(SelectedParkItem.name, SelectedParkItem.lat, SelectedParkItem.lon);
                }
            });
        }

        public async Task GetParkingSpot()
        {
            GetMapStyle();
            await GetCurrentLocation();
            var parkDataAsync = await _parkService.ParkDataAsync(eventItem.id, "99999999999");

            try
            {
                var eventlat = Convert.ToDouble(Lat, System.Globalization.CultureInfo.InvariantCulture);
                var eventlon = Convert.ToDouble(Lon, System.Globalization.CultureInfo.InvariantCulture);

                var eventPin = new Pin
                {
                    Type = PinType.Place,
                    Position = new Position(eventlat, eventlon),
                    Label = Title
                };
                
                Map.Pins.Add(eventPin);

                Map.PinClicked += (sender, e) =>
                {
                    //Set Selected Park Item if Clicked in Park Item(Get the Park Item for the Pin from Saved dict)
                    SelectedParkItem = _pinVsParkItems.ContainsKey(e.Pin) ? _pinVsParkItems[e.Pin] : null;

                    //Hide Info Window if ParkItem Clicked
                    if (_pinVsParkItems.ContainsKey(e.Pin))
                    {
                        Map.Pins.Remove(e.Pin);
                        Map.SelectedPin = null;
                        Map.Pins.Add(e.Pin);
                    }
                };

                Map.MapClicked += (sender, args) =>
                {
                    SelectedParkItem = null;//Remove Selected Park Item When Click Outside
                };

                parkDataAsync.OrderBy(x => x.dist);

                int loopIndex = 0;
                _pinVsParkItems.Clear();
                foreach (var i in parkDataAsync)
                {
                    if (loopIndex > 4)
                        return;

                    loopIndex++;

                    BitmapDescriptor SetPinIconStream(string embeddedResource)
                    {
                        var assembly = typeof(ParkPageViewModel).GetTypeInfo().Assembly;
                        var stream = assembly.GetManifestResourceStream($"" + embeddedResource);
                        return BitmapDescriptorFactory.FromStream(stream);
                    }

                    var parkPin = new Pin
                    {
                        Type = PinType.Place,
                        Position = new Position(i.lat, i.lon),
                        Label = "",
                        Icon = SetPinIconStream("EventParkering.parkingSpot.png")
                    };
                    
                    Map.Pins.Add(parkPin);
                    _pinVsParkItems.Add(parkPin, i);

                    double R = 6371.0; // Earth's radius
                    var dLat = (Math.PI / 180) * (i.lat - eventlat);
                    var dLon = (Math.PI / 180) * (i.lon - eventlon);
                    var lat1 = (Math.PI / 180) * eventlat;
                    var lat2 = (Math.PI / 180) * i.lat;

                    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var d = R * c; // distance in Km.

                    Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(eventlat, eventlon), Distance.FromKilometers(d)));
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error: {0}", err);
            }
        }       

        public void GetMapStyle()
        {
            var assembly = typeof(ParkPageViewModel).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("EventParkering.Services.SilverJsonStyle.json");
            string Json = "";
            using (var reader = new StreamReader(stream))
            {
                Json = reader.ReadToEnd();
            }
            Map.MapStyle = MapStyle.FromJson(Json);
        }

        private async Task<bool> GetCurrentLocation()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 100;
            if (!CrossGeolocator.IsSupported)
                return false;
            if (!CrossGeolocator.Current.IsGeolocationAvailable)
                return false;
            try
            {
                var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10));

                latitude = position.Latitude;
                longitude = position.Longitude;

                return true;
            }
            catch (Exception e)
            {
                string reason = e.Message;
                return false;
            }
        }

        public async override void OnNavigatingTo(INavigationParameters parameters)
        {
            eventItem = (EventItem)parameters["Event"];
            await GetParkingSpot();
        }
    }
}
