using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http.Filters;
using System.Xml.Linq;
using System.ServiceModel.Channels;
using Windows.Storage;
using Windows.Media.Protection.PlayReady;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KalkulatorWalutUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //Wymaga pakietu nuget System.Text.Encoding!
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var serwerNBP = new HttpClient();
            string dane = "";

            try
            {
                dane = await serwerNBP.GetStringAsync(new Uri(daneNBP));
            }
            catch (Exception)
            {

            }


            if (kursyAktualne.Count != 0)
            {
                kursyAktualne.Clear();
            }

            XDocument daneKursowe = XDocument.Parse(dane);

            StorageFolder folderLokalny = ApplicationData.Current.LocalFolder;
            StorageFile plikLokalny = await folderLokalny.TryGetItemAsync("Plik.xml") as StorageFile;
            if (plikLokalny != null)
            {
            }
            else
            {
                var odpowiedz = await serwerNBP.GetAsync(daneNBP);
                var stworzPlik = await ApplicationData.Current.LocalFolder.CreateFileAsync("Plik.xml", CreationCollisionOption.ReplaceExisting);
                var odpowiedzStream = await odpowiedz.Content.ReadAsStreamAsync();
                using (var Stream = await stworzPlik.OpenStreamForWriteAsync())
                {
                    await odpowiedzStream.CopyToAsync(Stream);
                }
            }
            XDocument DokumentXML = XDocument.Load($"{folderLokalny.Path}/Plik.xml");







            DateTimeOffset dataAktualizacjiNBP = DateTimeOffset.ParseExact((string)daneKursowe.Root.Element("data_publikacji").Value, "yyyy-MM-dd", null);
            DateTimeOffset dataAktualizacjiPlik = DateTimeOffset.ParseExact(DokumentXML.Root.Element("data_publikacji").Value, "yyyy-MM-dd", null);
            int porownanieDat = 0;
            porownanieDat = DateTimeOffset.Compare(dataAktualizacjiPlik, dataAktualizacjiNBP);

            if (porownanieDat >= 0)
            {
                kursyAktualne = (from item in DokumentXML.Descendants("pozycja")
                                 select new PozycjaTabeliA()
                                 {
                                     przelicznik = item.Element("przelicznik").Value,
                                     kod_waluty = item.Element("kod_waluty").Value,
                                     kurs_sredni = item.Element("kurs_sredni").Value
                                 }).ToList();

            }
            else
            {
                kursyAktualne = (from item in daneKursowe.Descendants("pozycja")
                                 select new PozycjaTabeliA()
                                 {
                                     przelicznik = item.Element("przelicznik").Value,
                                     kod_waluty = item.Element("kod_waluty").Value,
                                     kurs_sredni = item.Element("kurs_sredni").Value
                                 }).ToList();

                var odpowiedz = await serwerNBP.GetAsync(daneNBP);
                var stworzPlik = await ApplicationData.Current.LocalFolder.CreateFileAsync("Plik.xml", CreationCollisionOption.ReplaceExisting);
                var odpowiedzStream = await odpowiedz.Content.ReadAsStreamAsync();
                using (var Stream = await stworzPlik.OpenStreamForWriteAsync())
                {
                    await odpowiedzStream.CopyToAsync(Stream);
                }
            }
            lbxZWaluty.ItemsSource = kursyAktualne;
            lbxNaWalute.ItemsSource = kursyAktualne;
            PozycjaTabeliA PLN = new PozycjaTabeliA() { kurs_sredni = "1,0", kod_waluty = "PLN", przelicznik = "1" };
            kursyAktualne.Insert(0, PLN);










            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("lbxZwaluty"))
            {
                lbxZWaluty.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["lbxZWaluty"];
                KodZWaluty.Text = kursyAktualne[lbxZWaluty.SelectedIndex].kod_waluty;
            }
            else
            {
                lbxZWaluty.SelectedIndex = 0;
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("lbxNaWalute"))
            {
                lbxNaWalute.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["lbxNaWalute"];
                KodNaWalute.Text = kursyAktualne[lbxNaWalute.SelectedIndex].kod_waluty;
            }
            else
            {
                lbxZWaluty.SelectedIndex = 0;
            }



            txtAktualizacja.Text = "Ostatnia aktualizacja: " + (string)daneKursowe.Root.Element("data_publikacji").Value;
        }

        string daneNBP = "http://www.nbp.pl/kursy/xml/LastA.xml";
        List<PozycjaTabeliA> kursyAktualne = new List<PozycjaTabeliA>();

        private void txtKwota_TextChanged(object sender, TextChangedEventArgs e)
        {
            double kwota;
            Double.TryParse(txtKwota.Text, out kwota);
            double kursWalutyWyjsciowej;
            double kursWalutyDocelowej;

            int pozZWaluty = lbxZWaluty.SelectedIndex;
            int pozNaWalute = lbxNaWalute.SelectedIndex;
            PozycjaTabeliA zWaluty = kursyAktualne[pozZWaluty];
            Double.TryParse(zWaluty.kurs_sredni.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator), out kursWalutyWyjsciowej);
            PozycjaTabeliA naWalute = kursyAktualne[pozNaWalute];
            Double.TryParse(naWalute.kurs_sredni.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator), out kursWalutyDocelowej);
            double kwotaPLN = kwota * kursWalutyWyjsciowej;
            double kwotaDocelowa = kwotaPLN / kursWalutyDocelowej;

            tbprzeliczona.Text = kwotaDocelowa.ToString();
        }

        private void lbxZWaluty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndexZ = lbxZWaluty.SelectedIndex;

            ApplicationData.Current.LocalSettings.Values["lbxZWaluty"] = selectedIndexZ;

            KodZWaluty.Text = kursyAktualne[lbxZWaluty.SelectedIndex].kod_waluty;
        }

        private void lbxNaWalute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndexNA = lbxNaWalute.SelectedIndex;

            ApplicationData.Current.LocalSettings.Values["lbxNaWalute"] = selectedIndexNA;
            KodNaWalute.Text = kursyAktualne[lbxNaWalute.SelectedIndex].kod_waluty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OProgramie));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {



            string kodWaluty = lbxZWaluty.SelectedIndex.ToString();
            var kod = kursyAktualne[lbxZWaluty.SelectedIndex].kod_waluty;
            this.Frame.Navigate(typeof(Pomoc), new { Param1 = kod, Param2 = daneNBP });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                daneNBP = (string)e.Parameter;
            }
        }


    }

        public class PozycjaTabeliA
        {
            public string przelicznik { get; set; }
            public string kod_waluty { get; set; }
            public string kurs_sredni { get; set; }
        }



}