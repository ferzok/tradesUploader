using System.Collections.Generic;
using System.Configuration;

namespace WindowsFormsApplication1
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
            this.TradesParser = new System.Windows.Forms.Button();
            this.TradesParserStatus = new System.Windows.Forms.Label();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Fortsnextday = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.ABNDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.atonrecstartbutton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ADSS = new System.Windows.Forms.Button();
            this.FASTMATCH = new System.Windows.Forms.Button();
            this.MultyTradesCheckBox = new System.Windows.Forms.CheckBox();
            this.CliffCheckBox = new System.Windows.Forms.CheckBox();
            this.noparsingCheckbox = new System.Windows.Forms.CheckBox();
            this.SkipspreadcheckBox = new System.Windows.Forms.CheckBox();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxEnviroment = new System.Windows.Forms.ComboBox();
            this.UpdateABN = new System.Windows.Forms.Button();
            this.VMOSL = new System.Windows.Forms.Button();
            this.Mac = new System.Windows.Forms.Button();
            this.LEK = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.PostToRecon = new System.Windows.Forms.Button();
            this.VMAton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.procedures = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNPositionParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNFTParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bOFTUploadingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cpCostToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadFTBOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BrockerComboBox = new System.Windows.Forms.ComboBox();
            this.checkBoxAllDates = new System.Windows.Forms.CheckBox();
            this.OSL = new System.Windows.Forms.Button();
            this.RJOButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TradesParser
            // 
            this.TradesParser.Location = new System.Drawing.Point(188, 63);
            this.TradesParser.Name = "TradesParser";
            this.TradesParser.Size = new System.Drawing.Size(173, 29);
            this.TradesParser.TabIndex = 0;
            this.TradesParser.Text = "Trades parser";
            this.TradesParser.UseVisualStyleBackColor = true;
            this.TradesParser.Click += new System.EventHandler(this.TradesParser_Click);
            // 
            // TradesParserStatus
            // 
            this.TradesParserStatus.AutoSize = true;
            this.TradesParserStatus.Location = new System.Drawing.Point(67, 221);
            this.TradesParserStatus.Name = "TradesParserStatus";
            this.TradesParserStatus.Size = new System.Drawing.Size(59, 13);
            this.TradesParserStatus.TabIndex = 1;
            this.TradesParserStatus.Text = "Not started";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            this.openFileDialog2.Multiselect = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 221);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Status:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(183, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "FORTS Next day:";
            // 
            // Fortsnextday
            // 
            this.Fortsnextday.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.Fortsnextday.Location = new System.Drawing.Point(276, 95);
            this.Fortsnextday.Name = "Fortsnextday";
            this.Fortsnextday.Size = new System.Drawing.Size(85, 20);
            this.Fortsnextday.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 63);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(164, 26);
            this.button1.TabIndex = 6;
            this.button1.Text = "ABNParser";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ABNReconButtonClick);
            // 
            // ABNDate
            // 
            this.ABNDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.ABNDate.Location = new System.Drawing.Point(104, 95);
            this.ABNDate.Name = "ABNDate";
            this.ABNDate.Size = new System.Drawing.Size(72, 20);
            this.ABNDate.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "ABN Report Date";
            // 
            // atonrecstartbutton
            // 
            this.atonrecstartbutton.Location = new System.Drawing.Point(370, 121);
            this.atonrecstartbutton.Name = "atonrecstartbutton";
            this.atonrecstartbutton.Size = new System.Drawing.Size(89, 23);
            this.atonrecstartbutton.TabIndex = 11;
            this.atonrecstartbutton.Text = "Aton";
            this.atonrecstartbutton.UseVisualStyleBackColor = true;
            this.atonrecstartbutton.Click += new System.EventHandler(this.atonrecstartbutton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // ADSS
            // 
            this.ADSS.Location = new System.Drawing.Point(271, 150);
            this.ADSS.Name = "ADSS";
            this.ADSS.Size = new System.Drawing.Size(90, 25);
            this.ADSS.TabIndex = 14;
            this.ADSS.Text = "ADSS";
            this.ADSS.UseVisualStyleBackColor = true;
            this.ADSS.Click += new System.EventHandler(this.button3_Click);
            // 
            // FASTMATCH
            // 
            this.FASTMATCH.Location = new System.Drawing.Point(370, 179);
            this.FASTMATCH.Name = "FASTMATCH";
            this.FASTMATCH.Size = new System.Drawing.Size(89, 23);
            this.FASTMATCH.TabIndex = 17;
            this.FASTMATCH.Text = "FASTMATCH";
            this.FASTMATCH.UseVisualStyleBackColor = true;
            this.FASTMATCH.Click += new System.EventHandler(this.button4_Click);
            // 
            // MultyTradesCheckBox
            // 
            this.MultyTradesCheckBox.AutoSize = true;
            this.MultyTradesCheckBox.Checked = true;
            this.MultyTradesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MultyTradesCheckBox.Location = new System.Drawing.Point(12, 117);
            this.MultyTradesCheckBox.Name = "MultyTradesCheckBox";
            this.MultyTradesCheckBox.Size = new System.Drawing.Size(80, 17);
            this.MultyTradesCheckBox.TabIndex = 18;
            this.MultyTradesCheckBox.Text = "Multytrades";
            this.MultyTradesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CliffCheckBox
            // 
            this.CliffCheckBox.AutoSize = true;
            this.CliffCheckBox.Checked = true;
            this.CliffCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CliffCheckBox.Location = new System.Drawing.Point(12, 140);
            this.CliffCheckBox.Name = "CliffCheckBox";
            this.CliffCheckBox.Size = new System.Drawing.Size(43, 17);
            this.CliffCheckBox.TabIndex = 19;
            this.CliffCheckBox.Text = "Cliff";
            this.CliffCheckBox.UseVisualStyleBackColor = true;
            // 
            // noparsingCheckbox
            // 
            this.noparsingCheckbox.AutoSize = true;
            this.noparsingCheckbox.Location = new System.Drawing.Point(12, 158);
            this.noparsingCheckbox.Name = "noparsingCheckbox";
            this.noparsingCheckbox.Size = new System.Drawing.Size(97, 17);
            this.noparsingCheckbox.TabIndex = 20;
            this.noparsingCheckbox.Text = "without parsing";
            this.noparsingCheckbox.UseVisualStyleBackColor = true;
            // 
            // SkipspreadcheckBox
            // 
            this.SkipspreadcheckBox.AutoSize = true;
            this.SkipspreadcheckBox.Location = new System.Drawing.Point(13, 175);
            this.SkipspreadcheckBox.Name = "SkipspreadcheckBox";
            this.SkipspreadcheckBox.Size = new System.Drawing.Size(87, 17);
            this.SkipspreadcheckBox.TabIndex = 21;
            this.SkipspreadcheckBox.Text = "Skip spreads";
            this.SkipspreadcheckBox.UseVisualStyleBackColor = true;
            // 
            // LogTextBox
            // 
            this.LogTextBox.Location = new System.Drawing.Point(12, 289);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.Size = new System.Drawing.Size(447, 93);
            this.LogTextBox.TabIndex = 22;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Enviroment";
            // 
            // comboBoxEnviroment
            // 
            this.comboBoxEnviroment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxEnviroment.FormattingEnabled = true;
            this.comboBoxEnviroment.Location = new System.Drawing.Point(79, 38);
            this.comboBoxEnviroment.Name = "comboBoxEnviroment";
            this.comboBoxEnviroment.Size = new System.Drawing.Size(174, 21);
            this.comboBoxEnviroment.TabIndex = 24;
            this.comboBoxEnviroment.SelectedIndexChanged += new System.EventHandler(this.comboBoxEnviroment_SelectedIndexChanged);
            this.comboBoxEnviroment.TextChanged += new System.EventHandler(this.comboBoxEnviroment_TextChanged);
            // 
            // UpdateABN
            // 
            this.UpdateABN.Location = new System.Drawing.Point(149, 121);
            this.UpdateABN.Name = "UpdateABN";
            this.UpdateABN.Size = new System.Drawing.Size(104, 27);
            this.UpdateABN.TabIndex = 25;
            this.UpdateABN.Text = "UpdateABNsheet";
            this.UpdateABN.UseVisualStyleBackColor = true;
            this.UpdateABN.Click += new System.EventHandler(this.UpdatungViewCpTrades);
            // 
            // VMOSL
            // 
            this.VMOSL.Location = new System.Drawing.Point(149, 156);
            this.VMOSL.Name = "VMOSL";
            this.VMOSL.Size = new System.Drawing.Size(56, 28);
            this.VMOSL.TabIndex = 27;
            this.VMOSL.Text = "VM OSL";
            this.VMOSL.UseVisualStyleBackColor = true;
            this.VMOSL.Click += new System.EventHandler(this.button6_Click);
            // 
            // Mac
            // 
            this.Mac.Location = new System.Drawing.Point(149, 248);
            this.Mac.Name = "Mac";
            this.Mac.Size = new System.Drawing.Size(38, 27);
            this.Mac.TabIndex = 28;
            this.Mac.Text = "Mac";
            this.Mac.UseVisualStyleBackColor = true;
            this.Mac.Click += new System.EventHandler(this.button7_Click);
            // 
            // LEK
            // 
            this.LEK.Location = new System.Drawing.Point(271, 121);
            this.LEK.Name = "LEK";
            this.LEK.Size = new System.Drawing.Size(90, 27);
            this.LEK.TabIndex = 29;
            this.LEK.Text = "Lek";
            this.LEK.UseVisualStyleBackColor = true;
            this.LEK.Click += new System.EventHandler(this.button8_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(424, 97);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(35, 20);
            this.numericUpDown1.TabIndex = 33;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // PostToRecon
            // 
            this.PostToRecon.Location = new System.Drawing.Point(371, 63);
            this.PostToRecon.Name = "PostToRecon";
            this.PostToRecon.Size = new System.Drawing.Size(88, 29);
            this.PostToRecon.TabIndex = 34;
            this.PostToRecon.Text = "Post";
            this.PostToRecon.UseVisualStyleBackColor = true;
            this.PostToRecon.Click += new System.EventHandler(this.button11_Click);
            // 
            // VMAton
            // 
            this.VMAton.Location = new System.Drawing.Point(371, 148);
            this.VMAton.Name = "VMAton";
            this.VMAton.Size = new System.Drawing.Size(89, 25);
            this.VMAton.TabIndex = 35;
            this.VMAton.Text = "VM Aton";
            this.VMAton.UseVisualStyleBackColor = true;
            this.VMAton.Click += new System.EventHandler(this.button12_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.procedures});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(587, 24);
            this.menuStrip1.TabIndex = 36;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // procedures
            // 
            this.procedures.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aBNPositionParsingToolStripMenuItem,
            this.aBNFTParsingToolStripMenuItem,
            this.bOFTUploadingToolStripMenuItem,
            this.cpCostToolStripMenuItem,
            this.updateOpenToolStripMenuItem,
            this.uploadFTBOToolStripMenuItem});
            this.procedures.Name = "procedures";
            this.procedures.Size = new System.Drawing.Size(45, 20);
            this.procedures.Text = "Func";
            // 
            // aBNPositionParsingToolStripMenuItem
            // 
            this.aBNPositionParsingToolStripMenuItem.Name = "aBNPositionParsingToolStripMenuItem";
            this.aBNPositionParsingToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aBNPositionParsingToolStripMenuItem.Text = "ABNPositionParsing";
            this.aBNPositionParsingToolStripMenuItem.Click += new System.EventHandler(this.aBNPositionParsingToolStripMenuItem_Click);
            // 
            // aBNFTParsingToolStripMenuItem
            // 
            this.aBNFTParsingToolStripMenuItem.Name = "aBNFTParsingToolStripMenuItem";
            this.aBNFTParsingToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aBNFTParsingToolStripMenuItem.Text = "ABNFTParsing";
            this.aBNFTParsingToolStripMenuItem.Click += new System.EventHandler(this.aBNFTParsingToolStripMenuItem_Click);
            // 
            // bOFTUploadingToolStripMenuItem
            // 
            this.bOFTUploadingToolStripMenuItem.Name = "bOFTUploadingToolStripMenuItem";
            this.bOFTUploadingToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.bOFTUploadingToolStripMenuItem.Text = "BOFTUploading";
            this.bOFTUploadingToolStripMenuItem.Click += new System.EventHandler(this.bOFTUploadingToolStripMenuItem_Click);
            // 
            // cpCostToolStripMenuItem
            // 
            this.cpCostToolStripMenuItem.Name = "cpCostToolStripMenuItem";
            this.cpCostToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cpCostToolStripMenuItem.Text = "CpCost";
            this.cpCostToolStripMenuItem.Click += new System.EventHandler(this.cpCostToolStripMenuItem_Click);
            // 
            // updateOpenToolStripMenuItem
            // 
            this.updateOpenToolStripMenuItem.Name = "updateOpenToolStripMenuItem";
            this.updateOpenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.updateOpenToolStripMenuItem.Text = "Update Open";
            this.updateOpenToolStripMenuItem.Click += new System.EventHandler(this.updateOpenToolStripMenuItem_Click);
            // 
            // uploadFTBOToolStripMenuItem
            // 
            this.uploadFTBOToolStripMenuItem.Name = "uploadFTBOToolStripMenuItem";
            this.uploadFTBOToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.uploadFTBOToolStripMenuItem.Text = "Upload FT BO";
            this.uploadFTBOToolStripMenuItem.Click += new System.EventHandler(this.uploadFTBOToolStripMenuItem_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(375, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 37;
            this.label5.Text = "Qty days";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(269, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 38;
            this.label6.Text = "Brocker";
            // 
            // BrockerComboBox
            // 
            this.BrockerComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BrockerComboBox.FormattingEnabled = true;
            this.BrockerComboBox.Location = new System.Drawing.Point(317, 38);
            this.BrockerComboBox.Name = "BrockerComboBox";
            this.BrockerComboBox.Size = new System.Drawing.Size(143, 21);
            this.BrockerComboBox.TabIndex = 39;
            this.BrockerComboBox.TextChanged += new System.EventHandler(this.BrockerComboBox_TextChanged);
            // 
            // checkBoxAllDates
            // 
            this.checkBoxAllDates.AutoSize = true;
            this.checkBoxAllDates.Location = new System.Drawing.Point(13, 195);
            this.checkBoxAllDates.Name = "checkBoxAllDates";
            this.checkBoxAllDates.Size = new System.Drawing.Size(66, 17);
            this.checkBoxAllDates.TabIndex = 40;
            this.checkBoxAllDates.Text = "All dates";
            this.checkBoxAllDates.UseVisualStyleBackColor = true;
            // 
            // OSL
            // 
            this.OSL.Location = new System.Drawing.Point(149, 189);
            this.OSL.Name = "OSL";
            this.OSL.Size = new System.Drawing.Size(38, 23);
            this.OSL.TabIndex = 41;
            this.OSL.Text = "OSL";
            this.OSL.UseVisualStyleBackColor = true;
            this.OSL.Click += new System.EventHandler(this.OSL_Click);
            // 
            // RJOButton
            // 
            this.RJOButton.Location = new System.Drawing.Point(149, 218);
            this.RJOButton.Name = "RJOButton";
            this.RJOButton.Size = new System.Drawing.Size(42, 23);
            this.RJOButton.TabIndex = 42;
            this.RJOButton.Text = "RJO";
            this.RJOButton.UseVisualStyleBackColor = true;
            this.RJOButton.Click += new System.EventHandler(this.RjoClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(212, 155);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(41, 29);
            this.button2.TabIndex = 43;
            this.button2.Text = "Send";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(271, 179);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 25);
            this.button3.TabIndex = 44;
            this.button3.Text = "ADSS Balance";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(193, 248);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(65, 26);
            this.button4.TabIndex = 45;
            this.button4.Text = "Mac bal";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(193, 188);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(65, 24);
            this.button5.TabIndex = 46;
            this.button5.Text = "OSL Fees";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(193, 218);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(65, 23);
            this.button7.TabIndex = 48;
            this.button7.Text = "OSL Bal";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.GetOslBalance);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(271, 211);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(42, 23);
            this.button8.TabIndex = 49;
            this.button8.Text = "CFH";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click_1);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(271, 248);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(52, 23);
            this.button9.TabIndex = 50;
            this.button9.Text = "bloomberg";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(314, 210);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(54, 23);
            this.button11.TabIndex = 51;
            this.button11.Text = "CFH bal";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click_1);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(370, 210);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(89, 23);
            this.button6.TabIndex = 52;
            this.button6.Text = "Nissan";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.NissanButtonClick);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(329, 248);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 53;
            this.button10.Text = "Mac position";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click_1);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(410, 248);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(48, 23);
            this.button12.TabIndex = 54;
            this.button12.Text = "IS-PRIME";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click_2);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(466, 195);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 76);
            this.button13.TabIndex = 55;
            this.button13.Text = "Mac";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(466, 84);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 33);
            this.button14.TabIndex = 56;
            this.button14.Text = "Mac Emir";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(466, 124);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(75, 33);
            this.button15.TabIndex = 57;
            this.button15.Text = "Mac Emir";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(587, 413);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.RJOButton);
            this.Controls.Add(this.OSL);
            this.Controls.Add(this.checkBoxAllDates);
            this.Controls.Add(this.BrockerComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.VMAton);
            this.Controls.Add(this.PostToRecon);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.LEK);
            this.Controls.Add(this.Mac);
            this.Controls.Add(this.VMOSL);
            this.Controls.Add(this.UpdateABN);
            this.Controls.Add(this.comboBoxEnviroment);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.SkipspreadcheckBox);
            this.Controls.Add(this.noparsingCheckbox);
            this.Controls.Add(this.CliffCheckBox);
            this.Controls.Add(this.MultyTradesCheckBox);
            this.Controls.Add(this.FASTMATCH);
            this.Controls.Add(this.ADSS);
            this.Controls.Add(this.atonrecstartbutton);
            this.Controls.Add(this.ABNDate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Fortsnextday);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TradesParserStatus);
            this.Controls.Add(this.TradesParser);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Tools";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

       
        private System.Windows.Forms.Button TradesParser;
        private System.Windows.Forms.Label TradesParserStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker Fortsnextday;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DateTimePicker ABNDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button atonrecstartbutton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button ADSS;
        private System.Windows.Forms.Button FASTMATCH;
        private System.Windows.Forms.CheckBox MultyTradesCheckBox;
        private System.Windows.Forms.CheckBox CliffCheckBox;
        private System.Windows.Forms.CheckBox noparsingCheckbox;
        private System.Windows.Forms.CheckBox SkipspreadcheckBox;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxEnviroment;
        private System.Windows.Forms.Button UpdateABN;
        private System.Windows.Forms.Button VMOSL;
        private System.Windows.Forms.Button Mac;
        private System.Windows.Forms.Button LEK;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button PostToRecon;
        private System.Windows.Forms.Button VMAton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem procedures;
        private System.Windows.Forms.ToolStripMenuItem aBNPositionParsingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aBNFTParsingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bOFTUploadingToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox BrockerComboBox;
        private System.Windows.Forms.CheckBox checkBoxAllDates;
        private System.Windows.Forms.Button OSL;
        private System.Windows.Forms.Button RJOButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.ToolStripMenuItem cpCostToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadFTBOToolStripMenuItem;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
    }
}

