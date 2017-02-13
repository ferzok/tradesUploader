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
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.Fortsnextday = new System.Windows.Forms.DateTimePicker();
            this.InputDate = new System.Windows.Forms.DateTimePicker();
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
            this.VMOSL = new System.Windows.Forms.Button();
            this.Mac = new System.Windows.Forms.Button();
            this.LEK = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.PostToRecon = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.procedures = new System.Windows.Forms.ToolStripMenuItem();
            this.bOFTUploadingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cpCostToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadFTBOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fastmatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cFHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cFHReconciliationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cFHBalanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.atonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vMAtonToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.atonReconciliationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNParserToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNPositionParsingToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aBNFTParsingToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.updateABNSheetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oSLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oSLParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oSLFeesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oSLACIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oSLBalanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oSLDEXParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BrockerComboBox = new System.Windows.Forms.ComboBox();
            this.checkBoxAllDates = new System.Windows.Forms.CheckBox();
            this.RJOButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.button22 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button28 = new System.Windows.Forms.Button();
            this.button29 = new System.Windows.Forms.Button();
            this.checkBoxMalta = new System.Windows.Forms.CheckBox();
            this.button30 = new System.Windows.Forms.Button();
            this.button31 = new System.Windows.Forms.Button();
            this.button32 = new System.Windows.Forms.Button();
            this.button33 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button27 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TradesParser
            // 
            this.TradesParser.Location = new System.Drawing.Point(197, 100);
            this.TradesParser.Name = "TradesParser";
            this.TradesParser.Size = new System.Drawing.Size(86, 27);
            this.TradesParser.TabIndex = 0;
            this.TradesParser.Text = "Trades parser";
            this.TradesParser.UseVisualStyleBackColor = true;
            this.TradesParser.Click += new System.EventHandler(this.TradesParser_Click);
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            this.openFileDialog2.Multiselect = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "FORTS Next day:";
            // 
            // Fortsnextday
            // 
            this.Fortsnextday.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.Fortsnextday.Location = new System.Drawing.Point(105, 84);
            this.Fortsnextday.Name = "Fortsnextday";
            this.Fortsnextday.Size = new System.Drawing.Size(72, 20);
            this.Fortsnextday.TabIndex = 5;
            // 
            // InputDate
            // 
            this.InputDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.InputDate.Location = new System.Drawing.Point(105, 63);
            this.InputDate.Name = "InputDate";
            this.InputDate.Size = new System.Drawing.Size(72, 20);
            this.InputDate.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Report Date";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // ADSS
            // 
            this.ADSS.Location = new System.Drawing.Point(289, 160);
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
            this.MultyTradesCheckBox.Location = new System.Drawing.Point(11, 125);
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
            this.CliffCheckBox.Location = new System.Drawing.Point(11, 144);
            this.CliffCheckBox.Name = "CliffCheckBox";
            this.CliffCheckBox.Size = new System.Drawing.Size(43, 17);
            this.CliffCheckBox.TabIndex = 19;
            this.CliffCheckBox.Text = "Cliff";
            this.CliffCheckBox.UseVisualStyleBackColor = true;
            // 
            // noparsingCheckbox
            // 
            this.noparsingCheckbox.AutoSize = true;
            this.noparsingCheckbox.Location = new System.Drawing.Point(11, 167);
            this.noparsingCheckbox.Name = "noparsingCheckbox";
            this.noparsingCheckbox.Size = new System.Drawing.Size(97, 17);
            this.noparsingCheckbox.TabIndex = 20;
            this.noparsingCheckbox.Text = "without parsing";
            this.noparsingCheckbox.UseVisualStyleBackColor = true;
            // 
            // SkipspreadcheckBox
            // 
            this.SkipspreadcheckBox.AutoSize = true;
            this.SkipspreadcheckBox.Location = new System.Drawing.Point(11, 189);
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
            // VMOSL
            // 
            this.VMOSL.Location = new System.Drawing.Point(127, 160);
            this.VMOSL.Name = "VMOSL";
            this.VMOSL.Size = new System.Drawing.Size(75, 24);
            this.VMOSL.TabIndex = 27;
            this.VMOSL.Text = "VM";
            this.VMOSL.UseVisualStyleBackColor = true;
            this.VMOSL.Click += new System.EventHandler(this.VmClick);
            // 
            // Mac
            // 
            this.Mac.Location = new System.Drawing.Point(289, 248);
            this.Mac.Name = "Mac";
            this.Mac.Size = new System.Drawing.Size(90, 23);
            this.Mac.TabIndex = 28;
            this.Mac.Text = "Mac";
            this.Mac.UseVisualStyleBackColor = true;
            this.Mac.Click += new System.EventHandler(this.button7_Click);
            // 
            // LEK
            // 
            this.LEK.Location = new System.Drawing.Point(127, 133);
            this.LEK.Name = "LEK";
            this.LEK.Size = new System.Drawing.Size(75, 21);
            this.LEK.TabIndex = 29;
            this.LEK.Text = "Lek";
            this.LEK.UseVisualStyleBackColor = true;
            this.LEK.Click += new System.EventHandler(this.button8_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(105, 110);
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
            this.PostToRecon.Location = new System.Drawing.Point(384, 100);
            this.PostToRecon.Name = "PostToRecon";
            this.PostToRecon.Size = new System.Drawing.Size(75, 25);
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
            this.menuStrip1.Size = new System.Drawing.Size(649, 24);
            this.menuStrip1.TabIndex = 36;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // procedures
            // 
            this.procedures.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bOFTUploadingToolStripMenuItem,
            this.cpCostToolStripMenuItem,
            this.updateOpenToolStripMenuItem,
            this.uploadFTBOToolStripMenuItem,
            this.fastmatchToolStripMenuItem,
            this.cFHToolStripMenuItem,
            this.atonToolStripMenuItem,
            this.aBNToolStripMenuItem,
            this.oSLToolStripMenuItem});
            this.procedures.Name = "procedures";
            this.procedures.Size = new System.Drawing.Size(45, 20);
            this.procedures.Text = "Func";
            this.procedures.Click += new System.EventHandler(this.procedures_Click);
            // 
            // bOFTUploadingToolStripMenuItem
            // 
            this.bOFTUploadingToolStripMenuItem.Name = "bOFTUploadingToolStripMenuItem";
            this.bOFTUploadingToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.bOFTUploadingToolStripMenuItem.Text = "BOFTUploading";
            this.bOFTUploadingToolStripMenuItem.Click += new System.EventHandler(this.bOFTUploadingToolStripMenuItem_Click);
            // 
            // cpCostToolStripMenuItem
            // 
            this.cpCostToolStripMenuItem.Name = "cpCostToolStripMenuItem";
            this.cpCostToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.cpCostToolStripMenuItem.Text = "CpCost";
            this.cpCostToolStripMenuItem.Click += new System.EventHandler(this.cpCostToolStripMenuItem_Click);
            // 
            // updateOpenToolStripMenuItem
            // 
            this.updateOpenToolStripMenuItem.Name = "updateOpenToolStripMenuItem";
            this.updateOpenToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.updateOpenToolStripMenuItem.Text = "Update Open";
            this.updateOpenToolStripMenuItem.Click += new System.EventHandler(this.updateOpenToolStripMenuItem_Click);
            // 
            // uploadFTBOToolStripMenuItem
            // 
            this.uploadFTBOToolStripMenuItem.Name = "uploadFTBOToolStripMenuItem";
            this.uploadFTBOToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.uploadFTBOToolStripMenuItem.Text = "Upload FT BO";
            this.uploadFTBOToolStripMenuItem.Click += new System.EventHandler(this.uploadFTBOToolStripMenuItem_Click);
            // 
            // fastmatchToolStripMenuItem
            // 
            this.fastmatchToolStripMenuItem.Name = "fastmatchToolStripMenuItem";
            this.fastmatchToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.fastmatchToolStripMenuItem.Text = "Fastmatch";
            this.fastmatchToolStripMenuItem.Click += new System.EventHandler(this.fastmatchToolStripMenuItem_Click);
            // 
            // cFHToolStripMenuItem
            // 
            this.cFHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cFHReconciliationToolStripMenuItem,
            this.cFHBalanceToolStripMenuItem});
            this.cFHToolStripMenuItem.Name = "cFHToolStripMenuItem";
            this.cFHToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
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
            this.atonToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
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
            // aBNToolStripMenuItem
            // 
            this.aBNToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aBNParserToolStripMenuItem1,
            this.aBNPositionParsingToolStripMenuItem1,
            this.aBNFTParsingToolStripMenuItem1,
            this.updateABNSheetToolStripMenuItem});
            this.aBNToolStripMenuItem.Name = "aBNToolStripMenuItem";
            this.aBNToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.aBNToolStripMenuItem.Text = "ABN";
            // 
            // aBNParserToolStripMenuItem1
            // 
            this.aBNParserToolStripMenuItem1.Name = "aBNParserToolStripMenuItem1";
            this.aBNParserToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.aBNParserToolStripMenuItem1.Text = "ABNParser";
            this.aBNParserToolStripMenuItem1.Click += new System.EventHandler(this.aBNParserToolStripMenuItem1_Click);
            // 
            // aBNPositionParsingToolStripMenuItem1
            // 
            this.aBNPositionParsingToolStripMenuItem1.Name = "aBNPositionParsingToolStripMenuItem1";
            this.aBNPositionParsingToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.aBNPositionParsingToolStripMenuItem1.Text = "ABNPositionParsing";
            this.aBNPositionParsingToolStripMenuItem1.Click += new System.EventHandler(this.aBNPositionParsingToolStripMenuItem1_Click);
            // 
            // aBNFTParsingToolStripMenuItem1
            // 
            this.aBNFTParsingToolStripMenuItem1.Name = "aBNFTParsingToolStripMenuItem1";
            this.aBNFTParsingToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.aBNFTParsingToolStripMenuItem1.Text = "ABNFTParsing";
            this.aBNFTParsingToolStripMenuItem1.Click += new System.EventHandler(this.aBNFTParsingToolStripMenuItem1_Click);
            // 
            // updateABNSheetToolStripMenuItem
            // 
            this.updateABNSheetToolStripMenuItem.Name = "updateABNSheetToolStripMenuItem";
            this.updateABNSheetToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.updateABNSheetToolStripMenuItem.Text = "UpdateABNSheet";
            this.updateABNSheetToolStripMenuItem.Click += new System.EventHandler(this.updateABNSheetToolStripMenuItem_Click);
            // 
            // oSLToolStripMenuItem
            // 
            this.oSLToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oSLParsingToolStripMenuItem,
            this.oSLFeesToolStripMenuItem,
            this.oSLACIToolStripMenuItem,
            this.oSLBalanceToolStripMenuItem,
            this.oSLDEXParsingToolStripMenuItem});
            this.oSLToolStripMenuItem.Name = "oSLToolStripMenuItem";
            this.oSLToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.oSLToolStripMenuItem.Text = "OSL";
            // 
            // oSLParsingToolStripMenuItem
            // 
            this.oSLParsingToolStripMenuItem.Name = "oSLParsingToolStripMenuItem";
            this.oSLParsingToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.oSLParsingToolStripMenuItem.Text = "OSL Parsing";
            this.oSLParsingToolStripMenuItem.Click += new System.EventHandler(this.oSLParsingToolStripMenuItem_Click);
            // 
            // oSLFeesToolStripMenuItem
            // 
            this.oSLFeesToolStripMenuItem.Name = "oSLFeesToolStripMenuItem";
            this.oSLFeesToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.oSLFeesToolStripMenuItem.Text = "OSL Post Fees";
            this.oSLFeesToolStripMenuItem.Click += new System.EventHandler(this.oSLFeesToolStripMenuItem_Click);
            // 
            // oSLACIToolStripMenuItem
            // 
            this.oSLACIToolStripMenuItem.Name = "oSLACIToolStripMenuItem";
            this.oSLACIToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.oSLACIToolStripMenuItem.Text = "OSL Post ACI";
            this.oSLACIToolStripMenuItem.Click += new System.EventHandler(this.oSLACIToolStripMenuItem_Click);
            // 
            // oSLBalanceToolStripMenuItem
            // 
            this.oSLBalanceToolStripMenuItem.Name = "oSLBalanceToolStripMenuItem";
            this.oSLBalanceToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.oSLBalanceToolStripMenuItem.Text = "OSL Balance";
            this.oSLBalanceToolStripMenuItem.Click += new System.EventHandler(this.oSLBalanceToolStripMenuItem_Click);
            // 
            // oSLDEXParsingToolStripMenuItem
            // 
            this.oSLDEXParsingToolStripMenuItem.Name = "oSLDEXParsingToolStripMenuItem";
            this.oSLDEXParsingToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.oSLDEXParsingToolStripMenuItem.Text = "OSL DEX Parsing";
            this.oSLDEXParsingToolStripMenuItem.Click += new System.EventHandler(this.oSLDEXParsingToolStripMenuItem_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 109);
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
            this.checkBoxAllDates.Location = new System.Drawing.Point(11, 209);
            this.checkBoxAllDates.Name = "checkBoxAllDates";
            this.checkBoxAllDates.Size = new System.Drawing.Size(66, 17);
            this.checkBoxAllDates.TabIndex = 40;
            this.checkBoxAllDates.Text = "All dates";
            this.checkBoxAllDates.UseVisualStyleBackColor = true;
            // 
            // RJOButton
            // 
            this.RJOButton.Location = new System.Drawing.Point(384, 131);
            this.RJOButton.Name = "RJOButton";
            this.RJOButton.Size = new System.Drawing.Size(75, 23);
            this.RJOButton.TabIndex = 42;
            this.RJOButton.Text = "RJO";
            this.RJOButton.UseVisualStyleBackColor = true;
            this.RJOButton.Click += new System.EventHandler(this.RjoClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(208, 160);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 25);
            this.button2.TabIndex = 43;
            this.button2.Text = "Send VM";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(289, 189);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 25);
            this.button3.TabIndex = 44;
            this.button3.Text = "ADSS Balance";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(208, 248);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 45;
            this.button4.Text = "Mac bal";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(464, 316);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 50;
            this.button9.Text = "bloomberg";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(208, 131);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 52;
            this.button6.Text = "Nissan";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.NissanButtonClick);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(127, 248);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 53;
            this.button10.Text = "Mac position";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click_1);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(385, 248);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(74, 23);
            this.button12.TabIndex = 54;
            this.button12.Text = "IS-PRIME";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click_2);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(466, 115);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 23);
            this.button13.TabIndex = 55;
            this.button13.Text = "Mac";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(466, 144);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 33);
            this.button14.TabIndex = 56;
            this.button14.Text = "Mac Emir";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(548, 346);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(73, 25);
            this.button17.TabIndex = 59;
            this.button17.Text = "MT4";
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(466, 224);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(75, 25);
            this.button18.TabIndex = 60;
            this.button18.Text = "MT send";
            this.button18.UseVisualStyleBackColor = true;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // button20
            // 
            this.button20.Location = new System.Drawing.Point(384, 160);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(75, 25);
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
            this.button22.Location = new System.Drawing.Point(127, 190);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(75, 23);
            this.button22.TabIndex = 64;
            this.button22.Text = "Renesource";
            this.button22.UseVisualStyleBackColor = true;
            this.button22.Click += new System.EventHandler(this.button22_Click);
            // 
            // button23
            // 
            this.button23.Location = new System.Drawing.Point(554, 191);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(84, 23);
            this.button23.TabIndex = 65;
            this.button23.Text = "Updating links";
            this.button23.UseVisualStyleBackColor = true;
            this.button23.Click += new System.EventHandler(this.button23_Click);
            // 
            // button24
            // 
            this.button24.Location = new System.Drawing.Point(208, 219);
            this.button24.Name = "button24";
            this.button24.Size = new System.Drawing.Size(75, 23);
            this.button24.TabIndex = 66;
            this.button24.Text = "Rene RUEQ";
            this.button24.UseVisualStyleBackColor = true;
            this.button24.Click += new System.EventHandler(this.button24_Click);
            // 
            // button25
            // 
            this.button25.Location = new System.Drawing.Point(384, 219);
            this.button25.Name = "button25";
            this.button25.Size = new System.Drawing.Size(75, 23);
            this.button25.TabIndex = 67;
            this.button25.Text = "IB Belarta";
            this.button25.UseVisualStyleBackColor = true;
            this.button25.Click += new System.EventHandler(this.button25_Click);
            // 
            // button26
            // 
            this.button26.Location = new System.Drawing.Point(208, 191);
            this.button26.Name = "button26";
            this.button26.Size = new System.Drawing.Size(75, 21);
            this.button26.TabIndex = 68;
            this.button26.Text = "Rene GLF";
            this.button26.UseVisualStyleBackColor = true;
            this.button26.Click += new System.EventHandler(this.button26_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(289, 219);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(90, 23);
            this.button8.TabIndex = 70;
            this.button8.Text = "ItInvest";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click_1);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(384, 189);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(75, 23);
            this.button11.TabIndex = 71;
            this.button11.Text = "Axi";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.AxiButtonClick);
            // 
            // button28
            // 
            this.button28.Location = new System.Drawing.Point(466, 189);
            this.button28.Name = "button28";
            this.button28.Size = new System.Drawing.Size(82, 23);
            this.button28.TabIndex = 72;
            this.button28.Text = "Belarta LMAX";
            this.button28.UseVisualStyleBackColor = true;
            this.button28.Click += new System.EventHandler(this.button28_Click);
            // 
            // button29
            // 
            this.button29.Location = new System.Drawing.Point(554, 144);
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
            this.checkBoxMalta.Location = new System.Drawing.Point(12, 232);
            this.checkBoxMalta.Name = "checkBoxMalta";
            this.checkBoxMalta.Size = new System.Drawing.Size(52, 17);
            this.checkBoxMalta.TabIndex = 74;
            this.checkBoxMalta.Text = "Malta";
            this.checkBoxMalta.UseVisualStyleBackColor = true;
            // 
            // button30
            // 
            this.button30.Location = new System.Drawing.Point(127, 219);
            this.button30.Name = "button30";
            this.button30.Size = new System.Drawing.Size(75, 23);
            this.button30.TabIndex = 75;
            this.button30.Text = "Rene UMA";
            this.button30.UseVisualStyleBackColor = true;
            this.button30.Click += new System.EventHandler(this.button30_Click);
            // 
            // button31
            // 
            this.button31.Location = new System.Drawing.Point(554, 226);
            this.button31.Name = "button31";
            this.button31.Size = new System.Drawing.Size(75, 23);
            this.button31.TabIndex = 76;
            this.button31.Text = "Expiration";
            this.button31.UseVisualStyleBackColor = true;
            this.button31.Click += new System.EventHandler(this.button31_Click);
            // 
            // button32
            // 
            this.button32.Location = new System.Drawing.Point(289, 100);
            this.button32.Name = "button32";
            this.button32.Size = new System.Drawing.Size(90, 25);
            this.button32.TabIndex = 77;
            this.button32.Text = "AutoUpdate";
            this.button32.UseVisualStyleBackColor = true;
            this.button32.Click += new System.EventHandler(this.button32_Click);
            // 
            // button33
            // 
            this.button33.Location = new System.Drawing.Point(548, 289);
            this.button33.Name = "button33";
            this.button33.Size = new System.Drawing.Size(75, 23);
            this.button33.TabIndex = 78;
            this.button33.Text = "Belarta BCS";
            this.button33.UseVisualStyleBackColor = true;
            this.button33.Click += new System.EventHandler(this.button33_Click);
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(548, 317);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(75, 23);
            this.button15.TabIndex = 79;
            this.button15.Text = "Post PerFee";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click_1);
            // 
            // button27
            // 
            this.button27.Location = new System.Drawing.Point(289, 131);
            this.button27.Name = "button27";
            this.button27.Size = new System.Drawing.Size(90, 23);
            this.button27.TabIndex = 80;
            this.button27.Text = "RJO belarta";
            this.button27.UseVisualStyleBackColor = true;
            this.button27.Click += new System.EventHandler(this.RJO_belarta_click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(649, 413);
            this.Controls.Add(this.button27);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.button33);
            this.Controls.Add(this.button32);
            this.Controls.Add(this.button31);
            this.Controls.Add(this.button30);
            this.Controls.Add(this.checkBoxMalta);
            this.Controls.Add(this.button29);
            this.Controls.Add(this.button28);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button26);
            this.Controls.Add(this.button25);
            this.Controls.Add(this.button24);
            this.Controls.Add(this.button23);
            this.Controls.Add(this.button22);
            this.Controls.Add(this.button21);
            this.Controls.Add(this.button20);
            this.Controls.Add(this.button18);
            this.Controls.Add(this.button17);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.RJOButton);
            this.Controls.Add(this.checkBoxAllDates);
            this.Controls.Add(this.BrockerComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.PostToRecon);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.LEK);
            this.Controls.Add(this.Mac);
            this.Controls.Add(this.VMOSL);
            this.Controls.Add(this.comboBoxEnviroment);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.SkipspreadcheckBox);
            this.Controls.Add(this.noparsingCheckbox);
            this.Controls.Add(this.CliffCheckBox);
            this.Controls.Add(this.MultyTradesCheckBox);
            this.Controls.Add(this.ADSS);
            this.Controls.Add(this.InputDate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Fortsnextday);
            this.Controls.Add(this.label2);
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
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker Fortsnextday;
        private System.Windows.Forms.DateTimePicker InputDate;
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
        private System.Windows.Forms.Button VMOSL;
        private System.Windows.Forms.Button Mac;
        private System.Windows.Forms.Button LEK;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button PostToRecon;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem procedures;
        private System.Windows.Forms.ToolStripMenuItem bOFTUploadingToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox BrockerComboBox;
        private System.Windows.Forms.CheckBox checkBoxAllDates;
        private System.Windows.Forms.Button RJOButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.ToolStripMenuItem cpCostToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadFTBOToolStripMenuItem;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.ToolStripMenuItem fastmatchToolStripMenuItem;
        private System.Windows.Forms.Button button23;
        private System.Windows.Forms.Button button24;
        private System.Windows.Forms.Button button25;
        private System.Windows.Forms.Button button26;
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
        private System.Windows.Forms.Button button31;
        private System.Windows.Forms.Button button32;
        private System.Windows.Forms.Button button33;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button27;
        private System.Windows.Forms.ToolStripMenuItem aBNToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aBNParserToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aBNPositionParsingToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aBNFTParsingToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem updateABNSheetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSLParsingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSLFeesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSLACIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSLBalanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSLDEXParsingToolStripMenuItem;
    }
}

