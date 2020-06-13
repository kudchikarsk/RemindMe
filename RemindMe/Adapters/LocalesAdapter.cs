using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace RemindMe.Adapters
{
    public class LocalesAdapter : RecyclerView.Adapter
    {

        private readonly List<Locale> locales;
        private LocalesAdapterViewHolder selectable;
        private string SelectedLocale => Preferences.Get("locale","");

        public event Action<int, LocalesAdapterViewHolder> ItemClick = delegate { };

        public LocalesAdapter(IEnumerable<Locale> locales)
        {
            this.locales = locales.ToList();
        }


        public override RecyclerView.ViewHolder
        OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.select_voice_item, parent, false);
            ItemClick += OnItemClick;
            LocalesAdapterViewHolder vh = new LocalesAdapterViewHolder(itemView, ItemClick);
            return vh;
        }

        private void OnItemClick(int postiton, LocalesAdapterViewHolder selectable)
        {
            if (this.selectable != null && this.selectable != selectable)
            {
                this.selectable.Deselect();
            }                
            this.selectable = selectable;
        }

        public override void
            OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {            
            LocalesAdapterViewHolder vh = holder as LocalesAdapterViewHolder;
            var locale = locales.ElementAt(position);            
            vh.Title.Text = locale.Name;
            vh.Description.Text = locale.Language;
            if (SelectedLocale != locale.Name)
                vh.HideTick();
            else
            {
                vh.ShowTick();
                this.selectable = vh;
            }
                
        }

        public override int ItemCount
        {
            get { return locales.Count(); }
        }

    }

    public class LocalesAdapterViewHolder : RecyclerView.ViewHolder
    {       
        public TextView Title { get; set; }
        public TextView Description { get; set; }
        public ImageView TickImage { get; set; }

        public LocalesAdapterViewHolder(View view, Action<int, LocalesAdapterViewHolder> listener):base(view)
        {
            Title = view.FindViewById<TextView>(Resource.Id.textView1);
            Description = view.FindViewById<TextView>(Resource.Id.textView2);
            TickImage = view.FindViewById<ImageView>(Resource.Id.tickImage);
            view.Click += (s,e)=> Select();
            view.Click += (s, e) => listener(base.LayoutPosition, this);
        }

        public void Select()
        {
            var translateX = ObjectAnimator.OfFloat(TickImage, "translationX", 0);
            var fadeIn = ObjectAnimator.OfFloat(TickImage, "alpha", 0f, 1f);
            var set = new AnimatorSet();
            set.PlayTogether(translateX, fadeIn);
            set.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            set.SetDuration(200);
            set.Start();
        }

        public void Deselect()
        {
            var translateX = ObjectAnimator.OfFloat(TickImage, "translationX", -128);
            var fadeIn = ObjectAnimator.OfFloat(TickImage, "alpha", 1f, 0f);
            var set = new AnimatorSet();
            set.PlayTogether(translateX, fadeIn);
            set.SetInterpolator(new Android.Views.Animations.LinearInterpolator());
            set.SetDuration(200);
            set.Start();
        }

        public void HideTick()
        {
            TickImage.TranslationX = -128;
        }

        public void ShowTick()
        {
            TickImage.TranslationX = 0;
        }
    }
}