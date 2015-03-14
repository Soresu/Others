namespace CombolistGenerator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.combos = new System.Windows.Forms.RichTextBox();
            this.Start = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.ShiffleClick = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button6 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "txt files|*.txt";
            this.openFileDialog1.Title = "Select list";
            // 
            // combos
            // 
            this.combos.Location = new System.Drawing.Point(12, 12);
            this.combos.Name = "combos";
            this.combos.Size = new System.Drawing.Size(212, 314);
            this.combos.TabIndex = 0;
            this.combos.TabStop = false;
            this.combos.Text = "";
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(233, 10);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(156, 23);
            this.Start.TabIndex = 2;
            this.Start.Text = "Generate passworlds";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(233, 69);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(156, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Remove passworlds";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.RemoveClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(233, 98);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(156, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Email to username";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.EmalToUserClick);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(77, 333);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 1;
            this.button3.Text = "Open file";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OpenFileClick);
            // 
            // ShiffleClick
            // 
            this.ShiffleClick.Location = new System.Drawing.Point(233, 128);
            this.ShiffleClick.Name = "ShiffleClick";
            this.ShiffleClick.Size = new System.Drawing.Size(156, 23);
            this.ShiffleClick.TabIndex = 6;
            this.ShiffleClick.Text = "Shuffle rows";
            this.ShiffleClick.UseVisualStyleBackColor = true;
            this.ShiffleClick.Click += new System.EventHandler(this.ShiffleClick_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged_1);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(233, 158);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(156, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "Remove multiple rows";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.RemoveMultipleClick);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(233, 188);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(156, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Expand user list";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.ExpandClick);
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.SystemColors.Control;
            this.progressBar1.Location = new System.Drawing.Point(0, 379);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(399, 10);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 9;
            // 
            // button6
            // 
            this.button6.Enabled = false;
            this.button6.Location = new System.Drawing.Point(269, 313);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(93, 43);
            this.button6.TabIndex = 10;
            this.button6.Text = "Cancel";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileName = "ComboList";
            this.saveFileDialog1.Filter = "txt files|*.txt";
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(269, 270);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(93, 23);
            this.button7.TabIndex = 9;
            this.button7.Text = "Save to file";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(233, 39);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(156, 23);
            this.button8.TabIndex = 3;
            this.button8.Text = "Add the username to pw";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(233, 218);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(156, 23);
            this.button9.TabIndex = 11;
            this.button9.Text = "Clear list";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 389);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.ShiffleClick);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.combos);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Combolist Generator by Soresu";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RichTextBox combos;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button ShiffleClick;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
    }
}

