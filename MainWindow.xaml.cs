using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ThinNeo;

namespace NeoSpider
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<Transaction> txList = new List<Transaction>();
        List<Transaction> txShow = new List<Transaction>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.IsEnabled = false;
            var address = AddressTextBox.Text;
            try
            {
                Helper_NEO.GetScriptHash_FromAddress(address);
            }
            catch (Exception)
            {
                MessageBox.Show("Address Error");
                return;
            }
            finally
            {
                btn.IsEnabled = true;
            }
            StateTextBlock.Text = $"Starting…";
            DoEvents();
            var totalEntries = 0;
            using (var web = new WebClient())
            {
                var json = web.DownloadJson(address, 1);
                var totalPages = (int)json["total_pages"];
                totalEntries = (int)json["total_entries"];
                var pageSize = (int)json["page_size"];

                txList.Clear();
                for (int i = 1; i <= totalPages; i++)
                {
                    json = web.DownloadJson(address, i);
                    foreach (var item in json["entries"])
                    {
                        var tx = Transaction.FromJson(item);
                        if (tx != null)
                            txList.Add(tx);
                    }
                    StateTextBlock.Text = $"{Math.Min(i * pageSize, totalEntries)}/{totalEntries}";
                    DoEvents();
                }
            }
            StateTextBlock.Text = $"Calculating…";
            DoEvents();
            ProcessData();
            StateTextBlock.Text = $"Completed.";
            btn.IsEnabled = true;
        }
        public void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            try
            {
                Dispatcher.PushFrame(frame);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private object ExitFrames(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }


        private void AssetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessData();
        }

        private void ProcessData()
        {
            if (txList.Count == 0) return;
            var networkFees = new List<Transaction>();
            txList.ForEach(p => networkFees.Add(p));
            txList.RemoveAll(p => p.AddressFrom == "network_fees");

            networkFees = networkFees.Where(p => p.AddressFrom == "network_fees").GroupBy(p => p.Time.Year + p.Time.Month)
                .Select(g => new Transaction
                {
                    TxId = "",
                    Time = new DateTime(g.First().Time.Year, g.First().Time.Month, 1),
                    Asset = g.First().Asset,
                    Amount = g.Sum(p => p.Amount),
                    AddressFrom = g.First().AddressFrom,
                    AddressTo = g.First().AddressTo
                }).ToList();
            txList = txList.Concat(networkFees).ToList();

            var asset = (AssetComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (asset != "All")
                txShow = txList.Where(p => p.Asset == asset).ToList();
            else
                txShow = txList;
            TxDataGrid.ItemsSource = txShow;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
