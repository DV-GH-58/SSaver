using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Formats.Asn1;
using System.Numerics;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SSaver.Views;

public sealed partial class MainWindow : Window
{
    string SSText = DateTime.Now.ToString("t");
    string SSText_NL = "";
    private IList<SyndicationItem> Items;
    private string FeedText = DateTime.Now.ToString("t");
    private float X_Counter = 0;
    private float Y_Counter = 0;
    private int FeedListIdx = 0;
    private readonly Starfield _starfield;

    public async Task RefreshAsync()
    {
        try
        {
            Items = await LoadRssAsync("https://www.nu.nl/rss");
            if (Items == null || Items.Count == 0)
            {
                FeedText = "No RSS feed items found.";
            }
            else
            {
                FeedText = Items.Count.ToString();
            }
        }
        catch (Exception)
        {
            FeedText = "Failed to load RSS feed.";
        }
        finally
        {
            if (Items != null)
            {
                FeedListIdx = 0;
            }
            SSText_NL = DateTime.Now.ToString("ddd") + " " + DateTime.Now.ToString("d");
        }
    }

    public async Task<IList<SyndicationItem>> LoadRssAsync(string url)
    {
        using var client = new HttpClient();
        var stream = await client.GetStreamAsync(url);

        using var reader = XmlReader.Create(stream);
        var feed = SyndicationFeed.Load(reader);

        return feed?.Items?.ToList() ?? new List<SyndicationItem>();
    }

    public MainWindow()
    {
        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;

        _starfield = new Starfield();

        Canvas.Draw += Canvas_Draw;
        Canvas.CreateResources += Canvas_CreateResources;
        Canvas.PointerMoved += CloseOnInput;
        Canvas.PointerPressed += CloseOnInput;
        Canvas.KeyDown += CloseOnInput;
    }

    private void CloseOnInput(object sender, object e)
    {
        Close();
    }


    private CanvasBitmap _image;

    private async Task LoadImageAsync(CanvasAnimatedControl sender)
    {
        _image = await CanvasBitmap.LoadAsync(sender, "Assets/background.jpg");

        await RefreshAsync();

        //if (Items != null)
        //{
        //    if (Items.Count == 0)
        //        return;
        //    var i = Items[FeedListIdx++];
        //    if (i != null)
        //    {
        //        var _tmp = i.Title.Text;
        //        FeedText = _tmp;
        //    }
        //}
        //else
        //{
        //    FeedText = "No RSS feed items found.";
        //}
    }

    private void Canvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
    {
        args.TrackAsyncAction(LoadImageAsync(sender).AsAsyncAction());

        _starfield.Initialize(sender);
    }

    private void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        var pos = DateTime.Now.Minute % 2;

        var textFormat = new CanvasTextFormat()
        {
            FontSize = 200,
            FontFamily = "Segoe UI",
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Bottom
        };
        var textDayOfWeekFormat = new CanvasTextFormat()
        {
            FontSize = 80,
            FontFamily = "Segoe UI",
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Bottom
        };
        var textFormatRSS = new CanvasTextFormat()
        {
            FontSize = 50,
            FontFamily = "Segoe UI",
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Bottom
        };

        if (_image != null)
        {
            args.DrawingSession.DrawImage(_image, 0, 0, new Windows.Foundation.Rect(0, 0, (float)sender.Size.Width, (float)sender.Size.Height), opacity: 0.2f); // draw at X_Counter=100, Y_Counter=100 }
        }

        // update to current time\
        SSText = DateTime.Now.ToShortTimeString();

        _starfield.Update(sender);
        _starfield.Draw(sender, args.DrawingSession);

        args.DrawingSession.DrawText(SSText, (float)sender.Size.Width/2, (float)sender.Size.Height / 3,
            Windows.UI.Color.FromArgb((byte)(100 - DateTime.Now.Second), 0xff, 0xff, 0xff),
            textFormat);

        args.DrawingSession.DrawText(SSText_NL, (float)sender.Size.Width / 2, (float)sender.Size.Height / 4 * (2 + pos),
            Windows.UI.Color.FromArgb((byte)(100 + DateTime.Now.Second), 0xff, 0xff, 0xff), textDayOfWeekFormat);

        args.DrawingSession.DrawText(FeedText, (float)sender.Size.Width / 2, (float)sender.Size.Height,
           Colors.DarkGray, textFormatRSS);

        Canvas.Invalidate(); // continuous animation
    }

    private void Canvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
    {
        var delta = (float)args.Timing.ElapsedTime.TotalSeconds;

        X_Counter += 500 * delta; // pixels per second * Direction
        Y_Counter += 2 * delta; // pixels per second * Direction

        if (X_Counter > sender.Size.Width)
        {
            X_Counter = 0;
        }

        if (Y_Counter > 2)
        {
            Y_Counter = 0;

            if (Items != null)
            {
                if (FeedListIdx >= Items.Count)
                    FeedListIdx = 0;

                var i = Items[FeedListIdx++];

                if (i != null)
                {
                    var _tmp = i.Title.Text;
                    FeedText = _tmp;
                }
            }
        }
    }

}



