using Innouvous.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BooksDB
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {

        public bool Cancelled { get; private set; }
        
        public OptionsWindow()
        {
            InitializeComponent();
            Cancelled = true;

            DBPathTextbox.Text = MainWindow.Configs.DBPath;
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = DBPathTextbox.Text;

                FileInfo fi = new FileInfo(path);

                if (!fi.Directory.Exists)
                    throw new Exception("Directory invalid");

                MainWindow.Configs.DBPath = path;
                MainWindow.Configs.Save();

                Cancelled = false;

                this.Close();
                
            }
            catch (Exception ex)
            {
                MessageBoxFactory.ShowError(ex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancelled = true;
            this.Close();
        }

        private void SelectPathButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = DialogsUtility.CreateSaveFileDialog();
            DialogsUtility.AddExtension(sfd, "Books Database", "*.bdb");
            
            sfd.ShowDialog();

            if (!String.IsNullOrEmpty(sfd.FileName))
            {
                DBPathTextbox.Text = sfd.FileName;
            }
        }

    }
}
