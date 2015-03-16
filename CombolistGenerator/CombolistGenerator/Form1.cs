using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Mail;
using System.Windows.Forms;
using MiscUtil.IO;

namespace CombolistGenerator
{
    public partial class Form1 : Form
    {
        public static string OutputFile = "";
        public static ToolTip Help = new ToolTip();

        public Form1()
        {
            InitializeComponent();
            GenPref.Text = Properties.Settings.Default.GenPref;
            ExpPref.Text = Properties.Settings.Default.ExpPref;
        }

        #region Worker

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string asd;
            string tmpstring;
            LineReader lineReader;
            double idx;
            double max;
            if (button6.InvokeRequired)
            {
                button6.Invoke(new MethodInvoker(delegate { button6.Enabled = true; }));
            }
            else
            {
                button6.Enabled = true;
            }

            switch ((int) e.Argument)
            {
                case 1:

                    #region Generatepw

                    OutputFile = "";
                    asd = "";
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { asd = combos.Text; }));
                    }
                    else
                    {
                        asd = combos.Text;
                    }
                    if (asd == "")
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
                    tmpstring = asd;
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { combos.Text = ""; }));
                    }
                    else
                    {
                        combos.Text = "";
                    }
                    lineReader = new LineReader(() => new StringReader(tmpstring));
                    max = lineReader.Count();
                    idx = 0;
                    var prefixes = GenPref.Text.Split("|".ToCharArray());
                    foreach (var line in lineReader)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
                            return;
                        }
                        GenerateCombos(line,prefixes);
                        idx++;
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;
                case 2:

                    #region RemovePasswords

                    asd = "";
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { asd = combos.Text; }));
                    }
                    else
                    {
                        asd = combos.Text;
                    }
                    if (asd == "" || !asd.Contains(":"))
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
                    tmpstring = asd;
                    lineReader = new LineReader(() => new StringReader(tmpstring));
                    max = lineReader.Count();
                    idx = 0;
                    foreach (var line in lineReader)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
                            return;
                        }
                        if (line.Contains(":"))
                        {
                            SaveSingleLine(line.Split(":".ToCharArray())[0]);
                        }
                        idx++;
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;
                case 3:

                    #region EmailToUser

                    asd = "";
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { asd = combos.Text; }));
                    }
                    else
                    {
                        asd = combos.Text;
                    }
                    if (asd == "" || !asd.Contains("@"))
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
                    tmpstring = asd;
                    lineReader = new LineReader(() => new StringReader(tmpstring));
                    max = lineReader.Count();
                    idx = 0;
                    foreach (var line in lineReader)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
                            return;
                        }
                        if (line.Contains(":"))
                        {
                            var mail = line.Split(":".ToCharArray())[0];
                            var pass = line.Split(":".ToCharArray())[1];
                            if (IsValidEmail(mail))
                            {
                                SaveSingleLine(mail.Split("@".ToCharArray())[0] + ":" + pass);
                            }
                        }
                        else
                        {
                            if (IsValidEmail(line))
                            {
                                SaveSingleLine(line.Split("@".ToCharArray())[0]);
                            }
                        }
                        idx++;
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;
                case 4:

                    #region Shuffle

                    var lines = new LineReader(() => new StringReader(OutputFile));
                    var rnd = new Random();
                    var liness = lines.OrderBy(line => rnd.Next()).ToArray();
                    OutputFile = "";
                    max = liness.Count();
                    idx = 0;
                    foreach (var line in liness)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
                            return;
                        }
                        OutputFile += line + "\n";
                        idx++;
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;
                case 5:

                    #region Remove

                    asd = "";
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { asd = combos.Text; }));
                    }
                    else
                    {
                        asd = combos.Text;
                    }
                    if (asd == "")
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
                    tmpstring = asd;
                    lineReader = new LineReader(() => new StringReader(tmpstring));
                    var lineReaderDistincted = lineReader.Distinct();
                    max = lineReader.Count();
                    idx = 0;
                    foreach (var line in lineReaderDistincted)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
                            return;
                        }
                        SaveSingleLine(line);
                        idx++;
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;
                case 6:

                    #region Expand

                    asd = "";
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { asd = combos.Text; }));
                    }
                    else
                    {
                        asd = combos.Text;
                    }
                    if (asd == "")
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
                    tmpstring = asd;
                    lineReader = new LineReader(() => new StringReader(tmpstring));
                    max = lineReader.Count();
                    idx = 0;
                    var prefixesExp = ExpPref.Text.Split("|".ToCharArray());
                    foreach (var line in lineReader)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
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
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;

                case 7:

                    #region UserNameToPw

                    asd = "";
                    if (combos.InvokeRequired)
                    {
                        combos.Invoke(new MethodInvoker(delegate { asd = combos.Text; }));
                    }
                    else
                    {
                        asd = combos.Text;
                    }
                    if (asd == "")
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
                    tmpstring = asd;
                    lineReader = new LineReader(() => new StringReader(tmpstring));
                    max = lineReader.Count();
                    idx = 0;
                    foreach (var line in lineReader)
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            SetText();
                            return;
                        }

                        SaveSingleLine(line + ":" + line);

                        idx++;
                        var percent = idx / max * 100;
                        (sender as BackgroundWorker).ReportProgress((int) percent);
                    }

                    #endregion

                    break;
            }
            SetText();
        }

        private void backgroundWorker1_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SystemSounds.Beep.Play();
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
                    combos.Text = OutputFile;
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

        #endregion

        #region Helpers

        public void SetText()
        {
            if (combos.InvokeRequired)
            {
                combos.Invoke(new MethodInvoker(delegate { combos.Text = OutputFile; }));
            }
            else
            {
                combos.Text = OutputFile;
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
            OutputFile += combo;
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
            var pw = name.ToLower();
            try
            {
                foreach (var pref in prefixes)
                {
                    if (pref.Contains("*"))
                    {
                        SaveCombo(name + ":" + UppercaseFirst(pw) + pref.Replace("*", "") + "\n");
                    }
                    else if (pref.Contains("+"))
                    {
                        SaveCombo(name + ":" + pref.Replace("+", "") + "\n");
                    }
                    else
                    {
                        SaveCombo(name + ":" + pw + pref + "\n");
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
                        SaveCombo( UppercaseFirst(name) + pref.Replace("*", "") + "\n");
                    }else
                    {
                        SaveCombo(name + pref + "\n");
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
            SaveCombo(name + "\n");
        }

    #endregion

        private void button6_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            button6.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var file = openFileDialog1.FileName;
                try
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    sw.WriteLine(combos.Text);
                }
                catch (IOException)
                {
                    MessageBox.Show("Unable to save the file");
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(7);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            combos.Text = "";
        }

        private void PwPref_MouseHover(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Show("Prefixes separated with pipe \"|\" \nIf you start with \"*\" the first letter will be Uppercase \nWith \"+\" you declare a new password", textBox, 155, 0, 10000);
        }

        private void PwPref_MouseLeave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Hide(textBox);
        }

        private void UserPref_MouseHover(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Help.Show("Prefixes separated with pipe \"|\" \nIf you start with \"*\" the first letter will be Uppercase", textBox, 155, 0, 10000);
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
    }
}