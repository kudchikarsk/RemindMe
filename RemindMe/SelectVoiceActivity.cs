using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using RemindMe.Adapters;
using Xamarin.Essentials;

namespace RemindMe
{
    [Activity(Label = "SelectVoice")]
    public class SelectVoiceActivity : AppCompatActivity
    {
        private RecyclerView recyclerView;
        private LocalesAdapter adapter;
        private IEnumerable<Locale> locales;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            var themeId = Preferences.Get("theme", 0);
            if (themeId > 0) SetTheme(themeId);

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_select_voice);

            locales = await TextToSpeech.GetLocalesAsync();
            adapter = new LocalesAdapter(locales);
            adapter.ItemClick += Adapter_ItemClick;

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            var layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);

            recyclerView.SetAdapter(adapter);

        }

        private void Adapter_ItemClick(int positon, object selectable)
        {
            Preferences.Set("locale", locales.ElementAt(positon).Name);            
        }
    }
}