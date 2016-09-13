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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ADSS = new System.Windows.Forms.Button();
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.procedures = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNFTParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bOFTUploadingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cpCostToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadFTBOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fastmatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNPositionParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cFHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cFHReconciliationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cFHBalanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.atonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vMAtonToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.atonReconciliationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.button9 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.button22 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.button27 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button28 = new System.Windows.Forms.Button();
            this.button29 = new System.Windows.Forms.Button();
            this.checkBoxMalta = new System.Windows.Forms.CheckBox();
            this.button30 = new System.Windows.Forms.Button();
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
            this.TradesParserStatus.Location = new System.Drawing.Point(55, 253);
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
            this.label1.Location = new System.Drawing.Point(9, 253);
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
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.procedures});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(727, 24);
            this.menuStrip1.TabIndex = 36;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // procedures
            // 
            this.procedures.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aBNFTParsingToolStripMenuItem,
            this.bOFTUploadingToolStripMenuItem,
            this.cpCostToolStripMenuItem,
            this.updateOpenToolStripMenuItem,
            this.uploadFTBOToolStripMenuItem,
            this.fastmatchToolStripMenuItem,
            this.aBNPositionParsingToolStripMenuItem,
            this.cFHToolStripMenuItem,
            this.atonToolStripMenuItem});
            this.procedures.Name = "procedures";
            this.procedures.Size = new System.Drawing.Size(45, 20);
            this.procedures.Text = "Func";
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
            // fastmatchToolStripMenuItem
            // 
            this.fastmatchToolStripMenuItem.Name = "fastmatchToolStripMenuItem";
            this.fastmatchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.fastmatchToolStripMenuItem.Text = "Fastmatch";
            this.fastmatchToolStripMenuItem.Click += new System.EventHandler(this.fastmatchToolStripMenuItem_Click);
            // 
            // aBNPositionParsingToolStripMenuItem
            // 
            this.aBNPositionParsingToolStripMenuItem.Name = "aBNPositionParsingToolStripMenuItem";
            this.aBNPositionParsingToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aBNPositionParsingToolStripMenuItem.Text = "ABNPositionParsing";
            this.aBNPositionParsingToolStripMenuItem.Click += new System.EventHandler(this.aBNPositionParsingToolStripMenuItem_Click);
            // 
            // cFHToolStripMenuItem
            // 
            this.cFHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cFHReconciliationToolStripMenuItem,
            this.cFHBalanceToolStripMenuItem});
            this.cFHToolStripMenuItem.Name = "cFHToolStripMenuItem";
            this.cFHToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cFHToolStripMenuItem.Text = "CFH";
            // 
            // cFHReconciliationToolStripMenuItem
            // 
            this.cFHReconciliationToolStripMenuItem.Name = "cFHReconciliationToolStripMenuItem";
            this.cFHReconciliationToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.cFHReconciliationToolStripMenuItem.Text = "CFH Reconciliation";
            this.cFHReconciliationToolStripMenuItem.Click += new System.EventHandler(this.cFHReconciliationToolStripMenuItem_Click);
            // 
            // cFHBalanceToolStripMenuItem
            // 
            this.cFHBalanceToolStripMenuItem.Name = "cFHBalanceToolStripMenuItem";
            this.cFHBalanceToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.cFHBalanceToolStripMenuItem.Text = "CFH Balance";
            this.cFHBalanceToolStripMenuItem.Click += new System.EventHandler(this.cFHBalanceToolStripMenuItem_Click);
            // 
            // atonToolStripMenuItem
            // 
            this.atonToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.vMAtonToolStripMenuItem1,
            this.atonReconciliationToolStripMenuItem});
            this.atonToolStripMenuItem.Name = "atonToolStripMenuItem";
            this.atonToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.atonToolStripMenuItem.Text = "Aton";
            // 
            // vMAtonToolStripMenuItem1
            // 
            this.vMAtonToolStripMenuItem1.Name = "vMAtonToolStripMenuItem1";
            this.vMAtonToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.vMAtonToolStripMenuItem1.Text = "VM Aton";
            this.vMAtonToolStripMenuItem1.Click += new System.EventHandler(this.vMAtonToolStripMenuItem1_Click);
            // 
            // atonReconciliationToolStripMenuItem
            // 
            this.atonReconciliationToolStripMenuItem.Name = "atonReconciliationToolStripMenuItem";
            this.atonReconciliationToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.atonReconciliationToolStripMenuItem.Text = "Aton Reconciliation";
            this.atonReconciliationToolStripMenuItem.Click += new System.EventHandler(this.atonReconciliationToolStripMenuItem_Click);
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
            this.RJOButton.Location = new System.Drawing.Point(367, 121);
            this.RJOButton.Name = "RJOButton";
            this.RJOButton.Size = new System.Drawing.Size(75, 23);
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
            this.button4.Location = new System.Drawing.Point(193, 245);
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
            this.button7.Location = new System.Drawing.Point(201, 218);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(57, 23);
            this.button7.TabIndex = 48;
            this.button7.Text = "OSL Bal";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.GetOslBalance);
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
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(370, 210);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(72, 23);
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
            this.button13.Size = new System.Drawing.Size(75, 23);
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
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(138, 219);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(56, 23);
            this.button16.TabIndex = 58;
            this.button16.Text = "OSL Int";
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(466, 224);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(73, 25);
            this.button17.TabIndex = 59;
            this.button17.Text = "MT";
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(464, 255);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(75, 25);
            this.button18.TabIndex = 60;
            this.button18.Text = "MT send";
            this.button18.UseVisualStyleBackColor = true;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // button19
            // 
            this.button19.Location = new System.Drawing.Point(466, 287);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(75, 23);
            this.button19.TabIndex = 61;
            this.button19.Text = "OSL DEX";
            this.button19.UseVisualStyleBackColor = true;
            this.button19.Click += new System.EventHandler(this.DEXParsing);
            // 
            // button20
            // 
            this.button20.Location = new System.Drawing.Point(367, 152);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(75, 23);
            this.button20.TabIndex = 62;
            this.button20.Text = "RJO Cash";
            this.button20.UseVisualStyleBackColor = true;
            this.button20.Click += new System.EventHandler(this.button20_Click);
            // 
            // button21
            // 
            this.button21.Location = new System.Drawing.Point(466, 346);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(75, 23);
            this.button21.TabIndex = 63;
            this.button21.Text = "belarta";
            this.button21.UseVisualStyleBackColor = true;
            this.button21.Click += new System.EventHandler(this.BelartaClick);
            // 
            // button22
            // 
            this.button22.Location = new System.Drawing.Point(367, 179);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(75, 23);
            this.button22.TabIndex = 64;
            this.button22.Text = "Renesource";
            this.button22.UseVisualStyleBackColor = true;
            this.button22.Click += new System.EventHandler(this.button22_Click);
            // 
            // button23
            // 
            this.button23.Location = new System.Drawing.Point(547, 195);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(84, 23);
            this.button23.TabIndex = 65;
            this.button23.Text = "Updating links";
            this.button23.UseVisualStyleBackColor = true;
            this.button23.Click += new System.EventHandler(this.button23_Click);
            // 
            // button24
            // 
            this.button24.Location = new System.Drawing.Point(464, 161);
            this.button24.Name = "button24";
            this.button24.Size = new System.Drawing.Size(75, 23);
            this.button24.TabIndex = 66;
            this.button24.Text = "Rene RUEQ";
            this.button24.UseVisualStyleBackColor = true;
            this.button24.Click += new System.EventHandler(this.button24_Click);
            // 
            // button25
            // 
            this.button25.Location = new System.Drawing.Point(466, 317);
            this.button25.Name = "button25";
            this.button25.Size = new System.Drawing.Size(75, 23);
            this.button25.TabIndex = 67;
            this.button25.Text = "IB Belarta";
            this.button25.UseVisualStyleBackColor = true;
            this.button25.Click += new System.EventHandler(this.button25_Click);
            // 
            // button26
            // 
            this.button26.Location = new System.Drawing.Point(548, 161);
            this.button26.Name = "button26";
            this.button26.Size = new System.Drawing.Size(83, 23);
            this.button26.TabIndex = 68;
            this.button26.Text = "Rene GLF";
            this.button26.UseVisualStyleBackColor = true;
            this.button26.Click += new System.EventHandler(this.button26_Click);
            // 
            // button27
            // 
            this.button27.Location = new System.Drawing.Point(548, 124);
            this.button27.Name = "button27";
            this.button27.Size = new System.Drawing.Size(75, 23);
            this.button27.TabIndex = 69;
            this.button27.Text = "RJO FT";
            this.button27.UseVisualStyleBackColor = true;
            this.button27.Click += new System.EventHandler(this.button27_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(271, 211);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(90, 23);
            this.button8.TabIndex = 70;
            this.button8.Text = "ItInvest";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click_1);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(547, 225);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(84, 23);
            this.button11.TabIndex = 71;
            this.button11.Text = "Axi";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click_2);
            // 
            // button28
            // 
            this.button28.Location = new System.Drawing.Point(548, 257);
            this.button28.Name = "button28";
            this.button28.Size = new System.Drawing.Size(83, 23);
            this.button28.TabIndex = 72;
            this.button28.Text = "LMAX";
            this.button28.UseVisualStyleBackColor = true;
            this.button28.Click += new System.EventHandler(this.button28_Click);
            // 
            // button29
            // 
            this.button29.Location = new System.Drawing.Point(548, 84);
            this.button29.Name = "button29";
            this.button29.Size = new System.Drawing.Size(75, 23);
            this.button29.TabIndex = 73;
            this.button29.Text = "Post FT";
            this.button29.UseVisualStyleBackColor = true;
            this.button29.Click += new System.EventHandler(this.button29_Click);
            // 
            // checkBoxMalta
            // 
            this.checkBoxMalta.AutoSize = true;
            this.checkBoxMalta.Checked = true;
            this.checkBoxMalta.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMalta.Location = new System.Drawing.Point(12, 215);
            this.checkBoxMalta.Name = "checkBoxMalta";
            this.checkBoxMalta.Size = new System.Drawing.Size(52, 17);
            this.checkBoxMalta.TabIndex = 74;
            this.checkBoxMalta.Text = "Malta";
            this.checkBoxMalta.UseVisualStyleBackColor = true;
            // 
            // button30
            // 
            this.button30.Location = new System.Drawing.Point(638, 161);
            this.button30.Name = "button30";
            this.button30.Size = new System.Drawing.Size(71, 23);
            this.button30.TabIndex = 75;
            this.button30.Text = "Rene UMA";
            this.button30.UseVisualStyleBackColor = true;
            this.button30.Click += new System.EventHandler(this.button30_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(727, 413);
            this.Controls.Add(this.button30);
            this.Controls.Add(this.checkBoxMalta);
            this.Controls.Add(this.button29);
            this.Controls.Add(this.button28);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button27);
            this.Controls.Add(this.button26);
            this.Controls.Add(this.button25);
            this.Controls.Add(this.button24);
            this.Controls.Add(this.button23);
            this.Controls.Add(this.button22);
            this.Controls.Add(this.button21);
            this.Controls.Add(this.button20);
            this.Controls.Add(this.button19);
            this.Controls.Add(this.button18);
            this.Controls.Add(this.button17);
            this.Controls.Add(this.button16);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button9);
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
            this.Controls.Add(this.ADSS);
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
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button ADSS;
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
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.ToolStripMenuItem cpCostToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadFTBOToolStripMenuItem;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.ToolStripMenuItem fastmatchToolStripMenuItem;
        private System.Windows.Forms.Button button23;
        private System.Windows.Forms.Button button24;
        private System.Windows.Forms.Button button25;
        private System.Windows.Forms.Button button26;
        private System.Windows.Forms.Button button27;
        private System.Windows.Forms.ToolStripMenuItem cFHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cFHReconciliationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cFHBalanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem atonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vMAtonToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem atonReconciliationToolStripMenuItem;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button28;
        private System.Windows.Forms.Button button29;
        private System.Windows.Forms.CheckBox checkBoxMalta;
        private System.Windows.Forms.Button button30;
    }
}

