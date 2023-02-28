using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KalkulatorWalutUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Pomoc : Page
    {
        public Pomoc()
        {
            this.InitializeComponent();
        }
        string param1;
        string param2;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var Parametry = (dynamic)e.Parameter;
            param1 = Parametry.Param1;
            param2 = Parametry.Param2;

            if (param1 is string && !string.IsNullOrWhiteSpace(param1))
            {
                PomocTekst.Text = $"-- Twoja aktualnie wybrana waluta poczatkowa to: {param1}";
            }
            txtBoxSerwerNBP.Text = param2;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage),param2);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBoxSerwerNBP.Text))
            {
                param2 = txtBoxSerwerNBP.Text;
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                try
                {
                    StorageFile file = await localFolder.GetFileAsync("Plik.xml");
                    await file.DeleteAsync();
                }
                catch (Exception ex)
                {
                }
            }

        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var serwerNBP = new HttpClient();
            string dane = "";

            try
            {
                dane = await serwerNBP.GetStringAsync(new Uri(param2));
            }
            catch (Exception)
            {

            }

            XDocument daneKursowe = XDocument.Parse(dane);

            StorageFolder folderLokalny = ApplicationData.Current.LocalFolder;
            var odpowiedz = await serwerNBP.GetAsync(param2);
            var stworzPlik = await ApplicationData.Current.LocalFolder.CreateFileAsync("Plik.xml", CreationCollisionOption.ReplaceExisting);
            var odpowiedzStream = await odpowiedz.Content.ReadAsStreamAsync();
            using (var Stream = await stworzPlik.OpenStreamForWriteAsync())
            {
                await odpowiedzStream.CopyToAsync(Stream);
            }
        }
    }
}
