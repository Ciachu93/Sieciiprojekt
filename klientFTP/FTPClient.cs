using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;

namespace KlientFTP
{
    class FTPClient
    {
        #region Pola
        private string host;
        private string username;
        private string password;
        private string ftpPath;
        private bool downloadCompleted = true;
        private bool uploadCompleted = true;
        #endregion
        #region Metody
        public string Host
        {
            get { return host; }
            set { host = value; }
        }
        public string UserName
        {
            get { return username; }
            set { username = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }

        }
        public string CurrentDirectory
        {
            get
            {
                if (ftpPath.StartsWith("ftp://"))
                    return ftpPath;
                else
                    return "ftp://" + ftpPath;
            }
            set
            {
                ftpPath = value;
            }
        }
        public bool DownloadCompleted
        {
            get { return downloadCompleted; }
            set { downloadCompleted = value; }

        }
        public bool UploadCompleted
        {
            get { return uploadCompleted; }
            set { uploadCompleted = value; }

        }
        #endregion


        public ArrayList ChangeDirectory(string DirectoryName)
        {
            ftpPath += "/" + DirectoryName;
            return GetDirectories();
        }

        public FTPClient()
        {

        }
        public FTPClient(string host, string username, string password)
        {
            this.host = host;
            this.username = username;
            this.password = password;
            ftpPath = "ftp://" + this.host;
        }

        public ArrayList ChangeDirectoryUp()
        {
            if (ftpPath != "ftp://" + host)
            {
                ftpPath = ftpPath.Remove(ftpPath.LastIndexOf("/"), ftpPath.Length - ftpPath.LastIndexOf("/"));
                return GetDirectories();

            }
            else return GetDirectories();
        }

        public ArrayList GetDirectories()
        {

            ArrayList directories = new ArrayList();
            FtpWebRequest request;
            try
            {
                request = (FtpWebRequest)WebRequest.Create(ftpPath);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(this.username, this.password);
                request.KeepAlive = false;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string directory;
                        while ((directory = reader.ReadLine()) != null)
                        {
                            directories.Add(directory);
                        }

                    }
                }
                return directories;
            }

            catch
            {
                throw new Exception("Błąd: Nie mozna nawiązać połączenia z " + host);
            }
        }

       

        public void DownloadFileAsync(string FTPFileName, string FileName)
        {
            WebClient client = new WebClient();
            try
            {
                Uri uri = new Uri(ftpPath + "/" + FTPFileName);
                FileInfo file = new FileInfo(FileName);
                if (file.Exists)
                    throw new Exception("Błąd: Plik " + FileName + " istnieje");
                else
                {
                    client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);

                    client.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.Credentials = new NetworkCredential(this.username, this.password);
                    client.DownloadFileAsync(uri, FileName);
                    downloadCompleted = false;
                }
            }

            catch
            {
                client.Dispose();
                throw new Exception("Błąd: Pobieranie pliku niemożliwe");
            }
        }

        void client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            this.OnUpCompleted(sender, e);
        }

        void client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            this.OnUpProgressChanged(sender, e);
        }

        public void UploadFileAsync(string FileName)
        {
            try
            {
                System.Net.Cache.RequestCachePolicy cache = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Reload);
                WebClient client = new WebClient();
                FileInfo file = new FileInfo(FileName);
                Uri uri = new Uri((CurrentDirectory + '/' + file.Name).ToString());
                client.Credentials = new NetworkCredential(this.username, this.password);
                uploadCompleted = false;
                if (file.Exists)
                {
                    client.UploadFileCompleted += new UploadFileCompletedEventHandler(client_UploadFileCompleted);
                    client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged);
                    client.UploadFileAsync(uri, FileName);
                }
            }
            catch
            {
                throw new Exception("Błąd: Nie można wysłać pliku");
            }
        }



        public delegate void UpCompletedEventHandler(object sender, UploadFileCompletedEventArgs e);
        public event UpCompletedEventHandler UpCompleted;
        protected virtual void OnUpCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            if (UpCompleted != null) UpCompleted(sender, e);
        }


        public delegate void UpProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);
        public event UpProgressChangedEventHandler UpProgressChanged;
        protected virtual void OnUpProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (UpProgressChanged != null) UpProgressChanged(sender, e);
        }


        public delegate void DProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
        public event DProgressChangedEventHandler DProgressChanged;
        protected virtual void OnDProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DProgressChanged != null)
                DProgressChanged(sender, e);
        }

        
        public delegate void DCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
        public event DCompletedEventHandler DCompleted;
        protected virtual void OnDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DCompleted != null) DCompleted(sender, e);
        }


        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.OnDProgressChanged(sender, e);
        }

        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.OnDCompleted(sender, e);
        }


    }



}

