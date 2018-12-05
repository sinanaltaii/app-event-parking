﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using EventParkering.Model;
using Newtonsoft.Json;

namespace EventParkering.Services
{
    public class RestService : IRestService
    {      
        public List<EventItem> EventList { get; private set; }
        HttpClient client;

        public RestService()
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
        }

        public async Task<List<EventItem>> EventDataAsync()
        {
            EventList = new List<EventItem>();
            string EventUrl = "https://labs-api-ep-prod-webbapp.azurewebsites.net/getEvents";

            try
            {
                var response = await client.GetAsync(EventUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    EventList = JsonConvert.DeserializeObject<List<EventItem>>(content);
                    Debug.WriteLine(content);
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
            }

            return EventList;
        }

        /*public async Task<ObservableCollection<ParkItem>> ParkDataAsync(int id, string dist)
        {
            string callUrl = "https://labs-api-ep-prod-webbapp.azurewebsites.net/getEventParkingLots?id=" + id + "&dist=" + dist;
            try
            {
                var response = await client.GetAsync(callUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ObservableCollection<ParkItem> stations = JsonConvert.DeserializeObject<ObservableCollection<ParkItem>>(content);
                    Debug.WriteLine(stations);
                    return stations;
                }
                else {
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }*/
    }
}