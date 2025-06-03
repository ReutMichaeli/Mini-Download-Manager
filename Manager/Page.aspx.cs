using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Management;


namespace Manager
{
    public class FileItem
    {
        public string Title { get; set; }
        public string ImageURL { get; set; }
        public string FileURL { get; set; }
        public int Score { get; set; }
        public Dictionary<string, string> Validators { get; set; }

    }

    public static class SystemChecks
    {
        public static ulong GetAvailableRamMB()
        {
            ulong availableRam = 0;
            var searcher = new System.Management.ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");

            foreach (var obj in searcher.Get())
            {
                availableRam = (ulong)obj["FreePhysicalMemory"];
                break;
            }
            return availableRam / 1024; 
        }
    }
    public partial class Page : System.Web.UI.Page
    {
        private const string JsonUrl = "https://4qgz7zu7l5um367pzultcpbhmm0thhhg.lambda-url.us-west-2.on.aws/";
        private const string DownloadedFilesKey = "DownloadedFiles";
        private const string CurrentIndexKey = "CurrentIndex";
        private const string FilesKey = "Files";


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadFiles();
                ShowCurrentFile();
            }
        }

        private void LoadFiles()
        {
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(JsonUrl);
                List<FileItem> files = JsonConvert.DeserializeObject<List<FileItem>>(json);
                List<FileItem> compatibleFiles = files
                    .Where(IsCompatible)
                    .OrderByDescending(f => f.Score)
                    .ToList();

                Session[FilesKey] = compatibleFiles;
                Session[CurrentIndexKey] = 0;
                Session[DownloadedFilesKey] = new HashSet<string>();
            }
        }

        private void ShowCurrentFile()
        {
            var files = Session[FilesKey] as List<FileItem>;
            int index = (int)Session[CurrentIndexKey];
            if (files != null && files.Count > index)
            {
                FileItem file = files[index];
                lblTitle.Text = file.Title;
                imgIcon.ImageUrl = file.ImageURL;
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            var files = Session[FilesKey] as List<FileItem>;
            int index = (int)Session[CurrentIndexKey];
            var downloaded = Session[DownloadedFilesKey] as HashSet<string>;

            if (files == null || index >= files.Count)
            {
                lblStatus.Text = "No file to download.";
                return;
            }

            int percent = 100;

            // יצירת HTML של פס ההתקדמות
            string progressHtml = $@"
                <div class='progress-bar bg-success progress-bar-striped progress-bar-animated' 
                     role='progressbar' 
                     style='width: {percent}%; height: 100%;'>
                    {percent}%
                </div>";

            // הזרקת ה-HTML ל-Literal
            ltProgressBar.Text = progressHtml;

            // טקסט סטטוס
            lblStatus.Text = "Download Completed!";

            FileItem file = files[index];

            if (downloaded.Contains(file.FileURL))
            {
                lblStatus.Text = "Already downloaded this file before.";
                return;
            }
           
            try
            {
                string fileUrl = file.FileURL;
                string fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);

                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/5.0");

                byte[] fileData = client.DownloadData(fileUrl);

                downloaded.Add(fileUrl);
                Session[DownloadedFilesKey] = downloaded;


                Response.Clear();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
                Response.BinaryWrite(fileData);
                Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest(); 
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error during download: " + ex.Message;
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
                var files = Session[FilesKey] as List<FileItem>;
                int index = (int)Session[CurrentIndexKey];

                if (files != null && index + 1 < files.Count)
                {
                    Session[CurrentIndexKey] = index + 1;
                    ShowCurrentFile();
                    lblStatus.Text = "Showing next file.";
                }
                else
                {
                    lblStatus.Text = "No more files to display.";
                }
        }

        private bool IsCompatible(FileItem file)
        {
            var validators = file.Validators;
            if (validators == null) return true;

            if (validators.ContainsKey("ram"))
            {
                if (!ulong.TryParse(validators["ram"], out ulong requiredRam)) return false;
                ulong availableRam = SystemChecks.GetAvailableRamMB();
                if (availableRam < requiredRam) return false;
            }

            if (validators.ContainsKey("os"))
            {
                Version requiredVersion = new Version(validators["os"]);
                Version currentVersion = Environment.OSVersion.Version;
                if (currentVersion < requiredVersion) return false;
            }

            if (validators.ContainsKey("disk"))
            {
                if (!long.TryParse(validators["disk"], out long requiredDisk)) return false;
                string drive = Path.GetPathRoot(Environment.SystemDirectory);
                long availableDisk = new DriveInfo(drive).AvailableFreeSpace / (1024 * 1024);
                if (availableDisk < requiredDisk) return false;
            }

            return true;
        }
    }
}
