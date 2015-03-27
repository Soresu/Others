using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MiscUtil.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CombolistGenerator
{
    public partial class Form1 : Form
    {
        public static string OutputFile = "";
        public static ToolTip Help = new ToolTip();
        private static bool[] _lookup;
        private const string MatchPattern = @"^(?=.*[^a-zA-Z])(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])\S{8,}$";
        private List<string> temp=new List<string>(); 

        public Form1()
        {
            InitializeComponent();
            GenPref.Text = Properties.Settings.Default.GenPref;
            ExpPref.Text = Properties.Settings.Default.ExpPref;
            InitLookup();
        }

        private void InitLookup()
        {
            _lookup = new bool[65536];
            for (char c = '0'; c <= '9'; c++) _lookup[c] = true;
            for (char c = 'A'; c <= 'Z'; c++) _lookup[c] = true;
            for (char c = 'a'; c <= 'z'; c++) _lookup[c] = true;
            _lookup['.'] = true;
            _lookup['_'] = true;
        }

        #region Worker

        private async void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string asd;
            string tmpstring;
            LineReader lineReader;
            List<string> tempTemp = new List<string>(); 
            int idx;
            double max;
            if (button6.InvokeRequired)
            {
                button6.Invoke(new MethodInvoker(delegate { button6.Enabled = true; }));
            }
            else
            {
                button6.Enabled = true;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            switch ((int)e.Argument)
            {
                case 1:
                    #region Generatepw

                    OutputFile = "";
                    if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }

                        return;
                    }
                    idx = 0;
                    var prefixes = GenPref.Text.Split("|".ToCharArray());
                    tempTemp.AddRange(temp);
                    temp.Clear();
                    max = tempTemp.Count();
                    foreach (var line in tempTemp)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SaveStringlist(temp);
                            return;
                        }
                        idx++;
                        Setstatus(idx, max);
                        var percent = idx / max * 100;
                        GenerateCombos(line, prefixes);
                        (sender as BackgroundWorker).ReportProgress((int)percent);
                        //SetText();
                    }
                    max = temp.Count;
                    Setstatus(temp.Count +" combo generated");
                    SaveStringlist(temp);
                    #endregion
                    break;
                case 2:

                    #region RemovePasswords

                   if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }
                        return;
                    }
                    OutputFile = "";
                    tempTemp.AddRange(temp);
                    max = tempTemp.Count();
                    idx = 0;
                    temp.Clear();
                    foreach (var line in tempTemp)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SaveStringlist(temp);
                            return;
                        }
                        if (line.Contains(":"))
                        {
                            temp.Add(line.Split(":".ToCharArray())[0]);
                        }
                        idx++;
                        Setstatus(idx, max);
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int)percent);
                    }
                    SaveStringlist(temp);
                    #endregion

                    break;
                case 3:

                    #region EmailToUser

                    if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }
                        return;
                    }
                    OutputFile = "";
                    tempTemp.AddRange(temp);
                    temp.Clear();
                    max = tempTemp.Count();
                    idx = 0;
                    temp.Clear();
                    foreach (var line in tempTemp)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SaveStringlist(temp);
                            return;
                        }
                        if (line.Contains(":"))
                        {
                            var mail = line.Split(":".ToCharArray())[0];
                            var pass = line.Split(":".ToCharArray())[1];
                            if (IsValidEmail(mail))
                            {
                                temp.Add(mail.Split("@".ToCharArray())[0] + ":" + pass);
                            }
                        }
                        else
                        {
                            if (IsValidEmail(line))
                            {
                                temp.Add(line.Split("@".ToCharArray())[0]);
                            }
                        }
                        idx++;
                        Setstatus(idx, max);
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int)percent);
                    }
                    SaveStringlist(temp);
                    #endregion

                    break;
                case 4:

                    #region Shuffle
                    var rnd = new Random();
                    var liness = temp.OrderBy(line => rnd.Next()).ToArray();
                    OutputFile = "";
                    temp = liness.ToList();
                    SaveStringlist(liness.ToList());
                    Setstatus(liness.Count() + " line shuffled");

                    #endregion

                    break;
                case 5:
                    #region Remove
                    if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }
                        return;
                    }
                    OutputFile = "";
                    tempTemp.AddRange(temp);
                    temp.Clear();
                    temp = tempTemp.Distinct().ToList();
                    SaveStringlist(temp.ToList());
                    var removed = tempTemp.Count() - temp.Count();
                    Setstatus(removed + " removed");
                    #endregion
                    break;
                case 6:
                    #region Expand

                    if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }
                        return;
                    }
                    OutputFile = "";
                    tempTemp.AddRange(temp);
                    temp.Clear();
                    max = tempTemp.Count();
                    idx = 0;
                    var prefixesExp = ExpPref.Text.Split("|".ToCharArray());
                    temp.Clear();
                    foreach (var line in tempTemp)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SaveStringlist(temp);
                            return;
                        }
                        if (line.Contains(":"))
                        {
                            GenerateUserList(line.Split(":".ToCharArray())[0], prefixesExp);
                        }
                        else
                        {
                            GenerateUserList(line, prefixesExp);
                        }
                        idx++;
                        Setstatus(idx, max);
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int)percent);
                    }
                    SaveStringlist(temp);
                    #endregion
                    break;
                case 7:
                    #region UserNameToPw

                    if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }
                        return;
                    }
                    OutputFile = "";
                    tempTemp.AddRange(temp);
                    temp.Clear();
                    max = tempTemp.Count();
                    idx = 0;
                    temp.Clear();
                    foreach (var line in tempTemp)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SaveStringlist(temp);
                            return;
                        }
                        temp.Add(line + ":" + line);
                        idx++;
                        Setstatus(idx, max);
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int)percent);
                    }
                    SaveStringlist(temp);
                    #endregion
                    break;
                case 8:
                    #region Validate


                    if (temp.Count<2)
                    {
                        if (combos.InvokeRequired)
                        {
                            combos.Invoke(
                                new MethodInvoker(delegate { combos.Text = "There is a problem with the list"; }));
                        }
                        else
                        {
                            combos.Text = "There is a problem with the list";
                        }
                        return;
                    }
                    OutputFile = "";
                    tempTemp.AddRange(temp);
                    temp.Clear();
                    max = tempTemp.Count();
                    idx = 0;
                    temp.Clear();
                    foreach (var line in tempTemp)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SaveStringlist(temp);
                            return;
                        }
                        if (!(line.Split(':').Length - 1 > 1))
                        {
                            if (line.Contains(":"))
                            {
                                var combo = line.Split(":".ToCharArray());
                                if (CheckPassword(combo[1]))
                                {
                                    temp.Add(line);
                                }
                            }
                            else
                            {
                                if (CheckPassword(line))
                                {
                                    temp.Add(line);
                                }
                            }
                        }
                        idx++;
                        Setstatus(idx, max);
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int)percent);
                    }
                    SaveStringlist(temp);
                    Setstatus(tempTemp.Count() - temp.Count + " removed");
                    #endregion
                    break;
                case 9:
                    #region proxy
                    Regex rgx = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b:\d{2,5}", RegexOptions.IgnoreCase);
                    Regex rgxip = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", RegexOptions.IgnoreCase);
                    Setstatus("Request sent, downloading.");
                    Uri uri = new Uri("http://proxylists.net/http_highanon.txt");
                    var html = await DownloadStringAsync(uri);
                    MatchCollection matches = rgx.Matches(html);
                    List<string> proxyList= (from object line in matches select line.ToString()).ToList();

                    Setstatus("Request sent, downloading..");
                    uri = new Uri("http://web.unideb.hu/aurel192/proxylist.txt");
                    html = await DownloadStringAsync(uri);
                    matches = rgx.Matches(html);
                    proxyList.AddRange(from object line in matches select line.ToString());

                    Setstatus("Request sent, downloading...");
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
                        client.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
                        client.Encoding = Encoding.UTF8;
                        html = client.DownloadString("http://incloak.com/proxy-list/?maxtime=1000&ports=80&anon=34");
                        matches = rgxip.Matches(html);
                        proxyList.AddRange(from object line in matches select line.ToString());
                    }



                    idx = 0;
                    max = proxyList.Count;
                    SaveStringlist(proxyList.Distinct().ToList());
                    Setstatus(proxyList.Distinct().Count() + " proxy found");
                    #endregion
                    break;
                
            }
            //SetText();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        private void Setstatus(double idx, double max)
        {
            if (status.InvokeRequired)
            {
                status.Invoke(
                    new MethodInvoker(delegate { status.Text = idx + "/" + max; }));
            }
            else
            {
                status.Text = idx + "/" + max;
            }
        }
        private void Setstatus(string text)
        {
            if (status.InvokeRequired)
            {
                status.Invoke(
                    new MethodInvoker(delegate { status.Text = text; }));
            }
            else
            {
                status.Text = text;
            }
        }
        private void backgroundWorker1_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void SetProgressbar(int percent)
        {
            if (status.InvokeRequired)
            {
                status.Invoke(
                    new MethodInvoker(delegate { progressBar1.Value = percent; }));
            }
            else
            {
                progressBar1.Value = percent;
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SystemSounds.Beep.Play();
            progressBar1.Value = 100;
            button6.Enabled = false;
        }

        #endregion

        #region Buttons

        private void EmalToUserClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(3);
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(2);
        }

        private void Start_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(1);
        }

        private void OpenFileClick(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                var file = openFileDialog1.FileName;
                try
                {
                    OutputFile = File.ReadAllText(file);
                    var lineReader = new LineReader(() => new StringReader(OutputFile));
                    temp = lineReader.ToList();
                    SetText();
                    Setstatus(lineReader.Count() +" line added");
                }
                catch (IOException)
                {
                    combos.Text = "Error at loading file";
                }
            }
        }

        private void ShiffleClick_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(4);
        }

        private void RemoveMultipleClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(5);
        }

        private void ExpandClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(6);
        }
        private void StopClick(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            button6.Enabled = false;
        }

        private void SaveClick(object sender, EventArgs e)
        {
            var result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                try
                {
                    var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                    //^^^^^
                    using (var sw = new StreamWriter(saveFileDialog1.FileName, false, utf8WithoutBom))
                    {
                        sw.NewLine = "\r\n";
                        sw.WriteLine(String.Join(Environment.NewLine, temp));
                        sw.Close();
                        Setstatus("File saved!");
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Unable to save the file");
                }
            }
        }

        private void UserToPwClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(7);
        }

        private void ClearClick(object sender, EventArgs e)
        {
            combos.Text = "";
        }
        private void ValidateClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(8);
        }
        private void PwPref_MouseHover(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Show("Prefixes separated with pipe \"|\" \r\nIf you start with \"*\" the first letter will be Uppercase \r\nWith \"+\" you declare a new password", textBox, 155, 0, 10000);
        }

        private void PwPref_MouseLeave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Hide(textBox);
        }

        private void UserPref_MouseHover(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Show("Prefixes separated with pipe \"|\" \r\nIf you start with \"*\" the first letter will be Uppercase", textBox, 155, 0, 10000);
        }

        private void UserPref_MouseLeave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Hide(textBox);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ExpPref = ExpPref.Text;
            Properties.Settings.Default.GenPref = GenPref.Text;
            Properties.Settings.Default.Save();
        }

        private void ResetClick(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            button6.Enabled = false;
            GenPref.Text = "|*|0|01|1|12|123|*0|*1|*01|+password";
            ExpPref.Text = "|*|0|01|1|12";
            combos.Text = "";
        }

        private void button11_MouseHover(object sender, EventArgs e)
        {
            Button textBox = (Button)sender;
            Help.Show("Password include lowercase and uppercase alphabetic characters, numbers", textBox, 155, 1, 10000);
        }

        private void button11_MouseLeave(object sender, EventArgs e)
        {
            Button textBox = (Button)sender;
            Help.Hide(textBox);
        }

        private async void ProxyClick(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(9);

        }

        #endregion

        #region Helpers
        public bool CheckPassword(string password)
        {
            if (password != null) return Regex.IsMatch(password, MatchPattern);
            else return false;
        }
        public void SetText()
        {
            if (temp.Count > 500000)
            {
                if (combos.InvokeRequired)
                {
                    combos.Invoke(
                        new MethodInvoker(delegate
                        {
                            combos.Text = "Too much data to show here, save it to txt";
                            combos.SelectionStart = combos.Text.Length;
                            combos.ScrollToCaret();
                        }));
                }
                else
                {
                    combos.Text = "Too much data to show here, save it to txt";
                    combos.SelectionStart = combos.Text.Length;
                    combos.ScrollToCaret();
                }
                return;
            }
            if (combos.InvokeRequired)
            {
                combos.Invoke(new MethodInvoker(
                    delegate
                    {
                        combos.Text = OutputFile;
                        combos.SelectionStart = combos.Text.Length;
                        combos.ScrollToCaret();
                    }));
            }
            else
            {
                combos.Text = OutputFile;
                combos.SelectionStart = combos.Text.Length;
                combos.ScrollToCaret();
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public void SaveCombo(string combo)
        {
            if (combo.Length>0)
            {
                     OutputFile += combo;
            }
        }
        public void SaveStringlist(List<string> list)
        {
            if (list.Count>500000)
            {
                if (combos.InvokeRequired)
                {
                    combos.Invoke(
                        new MethodInvoker(delegate
                        {
                            combos.Text = "Too much data to show here, save it to txt";
                            combos.SelectionStart = combos.Text.Length;
                            combos.ScrollToCaret();
                        }));
                }
                else
                {
                    combos.Text = "Too much data to show here, save it to txt";
                    combos.SelectionStart = combos.Text.Length;
                    combos.ScrollToCaret();
                }
                return;
            }
            
                if (combos.InvokeRequired)
                {
                    combos.Invoke(
                        new MethodInvoker(delegate
                        {
                            combos.Text = String.Join(Environment.NewLine, list);
                            combos.SelectionStart = combos.Text.Length;
                            combos.ScrollToCaret();
                        }));
                }
                else
                {
                    combos.Text = String.Join(Environment.NewLine, list);
                    combos.SelectionStart = combos.Text.Length;
                    combos.ScrollToCaret();
                }

            
        }
        private static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public void GenerateCombos(string name, string[] prefixes)
        {

            try
            {
                var pw = name.Replace(" ", "").ToLower();
                foreach (var pref in prefixes)
                {
                    if (pref.Contains("*"))
                    {
                        temp.Add(name + ":" + UppercaseFirst(pw) + pref.Replace("*", ""));
                    }
                    else if (pref.Contains("+"))
                    {
                        temp.Add(name + ":" + pref.Replace("+", ""));
                    }
                    else
                    {
                        temp.Add(name + ":" + pw + pref);
                    }
                }
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e.ToString());
            }

        }

        public void GenerateUserList(string name, string[] prefixes)
        {
            try
            {
                foreach (var pref in prefixes)
                {
                    if (pref.Contains("*"))
                    {
                        temp.Add(UppercaseFirst(name) + pref.Replace("*", ""));
                    }
                    else
                    {
                        temp.Add(name + pref);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SaveSingleLine(string name)
        {
            SaveCombo(name + Environment.NewLine);
        }

        public static string RemoveSpecialCharacters(string str)
        {
            char[] buffer = new char[str.Length];
            int index = 0;
            foreach (char c in str)
            {
                if (_lookup[c])
                {
                    buffer[index] = c;
                    index++;
                }
            }
            return new string(buffer, 0, index);
        }

        public static Task<string> DownloadStringAsync(Uri url)
        {
            var tcs = new TaskCompletionSource<string>();
            var wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
            wc.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            wc.DownloadStringAsync(url);
            return tcs.Task;
        }

        #endregion


    }
}