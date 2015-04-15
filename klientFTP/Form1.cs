using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KlientFTP;
using klientFTP;
using System.Collections;
using System.IO;

namespace klientFTP
{
    public partial class Form1 : Form
    {

        string PlikLokalny = "";
        
        void klient_DCompleted(object sender,AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                MessageBox.Show("Błąd: " + e.Error.Message);
            else
                MessageBox.Show("Plik pobrany");
            klient.DownloadCompleted = true;
            button3.Enabled = true;
            button4.Enabled = true;
        }
        void klient_UpCompleted(object sender,System.Net.UploadFileCompletedEventArgs e)
        {
            if(e.Cancelled || e.Error != null)
            {
                MessageBox.Show("Błąd : " + e.Error.Message);
                klient.UploadCompleted = true;
                button3.Enabled = true;
                button4.Enabled = true;
                return;
            }
            klient.UploadCompleted = true;
            button3.Enabled = true;
            button4.Enabled = true;
            MessageBox.Show("Wysłano plik");
            try
            {
                ListujZawartosc(klient.GetDirectories());

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
            }

        }

        void klient_UpProgressChanged(object sender,System.Net.UploadProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Wysłano : " + (e.BytesSent / (double)1024).ToString() + " kB";
        }
        void klient_ProgressChanged(object sender,System.Net.DownloadProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Pobrano: " + (e.BytesReceived / (double)1024).ToString() + " kB";
        }

        public Form1()
        {
            InitializeComponent();
            klient.UpProgressChanged += new FTPClient.UpProgressChangedEventHandler(klient_UpProgressChanged);
            klient.UpCompleted += new FTPClient.UpCompletedEventHandler(klient_UpCompleted);
            klient.DProgressChanged += new FTPClient.DProgressChangedEventHandler(klient_ProgressChanged);
            klient.DCompleted += new FTPClient.DCompletedEventHandler(klient_DCompleted);
        }

        FTPClient klient = new FTPClient();
       

        private void ListujZawartosc(ArrayList ListaKatalogow)
        {
            listBox1.Items.Clear();
            listBox1.Items.Add("[..]");
            ListaKatalogow.Sort();
            textBox3.Text = klient.CurrentDirectory;
            foreach(string nazwa in ListaKatalogow)
            {
                string pozycja = nazwa.Substring(nazwa.LastIndexOf(' ') + 1, nazwa.Length - nazwa.LastIndexOf(' ') - 1);
                if(pozycja != ".." && pozycja !=".")
                    switch(nazwa[0])
                    {
                        case 'd':
                            listBox1.Items.Add("[" + pozycja + "]");
                            break;
                        case 'l':
                            listBox1.Items.Add("->" + pozycja);
                            break;
                        default:
                            listBox1.Items.Add(pozycja);
                            break;
                    }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.Text!=string.Empty&comboBox1.Text.Trim()!=String.Empty)
                try
                {
                    klient = new FTPClient(comboBox1.Text, textBox1.Text, maskedTextBox1.Text);
                    ListujZawartosc(klient.GetDirectories());
                    toolStripStatusLabel1.Text = "Serwer : ftp://" + klient.Host;
                    button1.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button6.Enabled = true;
                }
            catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else
            {
                MessageBox.Show("Wprowadź nazwe serwera FTP", "Błąd");
                comboBox1.Text = string.Empty;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int indeks = listBox1.SelectedIndex;
            if(listBox1.Items[indeks].ToString()[0]!='[')
            {
                //if(MessageBox.Show("Czy pobrać plik?","Pobieranie pliku",MessageBoxButtons.OKCancel,MessageBoxIcon.Question)==DialogResult.OK)
                {
                    try
                    {
                        PlikLokalny = listBox1.Items[indeks].ToString();
                        FileInfo fi = new FileInfo(PlikLokalny);
                        if (fi.Exists == false)
                        {
                            klient.DownloadFileAsync(listBox1.Items[indeks].ToString(), PlikLokalny);
                            //  button3.Enabled=false;
                            // button4.Enabled=false;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(2000);
                            this.button7_Click(sender, e);
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message,"Błąd");
                    }
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int indeks = listBox1.SelectedIndex;
            try
            {
                if(indeks > -1)
                {
                    if (indeks == 0)
                        ListujZawartosc(klient.ChangeDirectoryUp());
                    else
                        if (listBox1.Items[indeks].ToString()[0] == '[')
                        {
                            string directory = listBox1.Items[indeks].ToString().Substring(1, listBox1.Items[indeks].ToString().Length - 2);
                            ListujZawartosc(klient.ChangeDirectory(directory));
                        }
                        else
                            if (listBox1.Items[indeks].ToString()[0] == '-' && listBox1.Items[indeks].ToString()[2] == '.')
                            {
                                string link = listBox1.Items[indeks].ToString().Substring(5, listBox1.Items[indeks].ToString().Length - 5);
                                klient.CurrentDirectory = "ftp://" + klient.Host;
                                ListujZawartosc(klient.ChangeDirectory(link));
                            }
                            else
                                this.button3_Click(sender, e);
                    listBox1.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                this.listBox1_MouseDoubleClick(sender, null);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                try
                {
                    klient.UploadFileAsync(openFileDialog1.FileName);
                    button3.Enabled = false;
                    button4.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd");
                }
            }
        }

        private string [] OdczytajZPlikuTekstowego()
        {
            List<string> tekst = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(PlikLokalny))
                {
                    string linia;
                    while ((linia = sr.ReadLine()) != null)
                        tekst.Add(linia);
                }
                return tekst.ToArray();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Błą odczytu pliku : " + PlikLokalny + " (" + ex.Message + ")");
                return null;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBox4.Lines = OdczytajZPlikuTekstowego();
        }
    }
}
