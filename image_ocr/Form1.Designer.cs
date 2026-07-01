namespace image_ocr
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			panelTop = new DevExpress.XtraEditors.PanelControl();
			btnClear = new DevExpress.XtraEditors.SimpleButton();
			btnSave = new DevExpress.XtraEditors.SimpleButton();
			btnCopy = new DevExpress.XtraEditors.SimpleButton();
			btnBatch = new DevExpress.XtraEditors.SimpleButton();
			chkHighlight = new DevExpress.XtraEditors.CheckButton();
			chkUpscale = new DevExpress.XtraEditors.CheckButton();
			chkBinarize = new DevExpress.XtraEditors.CheckButton();
			chkContrast = new DevExpress.XtraEditors.CheckButton();
			chkGray = new DevExpress.XtraEditors.CheckButton();
			labelPre = new DevExpress.XtraEditors.LabelControl();
			btnRun = new DevExpress.XtraEditors.SimpleButton();
			comboLang = new DevExpress.XtraEditors.ComboBoxEdit();
			labelLang = new DevExpress.XtraEditors.LabelControl();
			comboEngine = new DevExpress.XtraEditors.ComboBoxEdit();
			labelEngine = new DevExpress.XtraEditors.LabelControl();
			btnPaste = new DevExpress.XtraEditors.SimpleButton();
			btnOpen = new DevExpress.XtraEditors.SimpleButton();
			splitContainer = new DevExpress.XtraEditors.SplitContainerControl();
			labelDropHint = new DevExpress.XtraEditors.LabelControl();
			memoText = new DevExpress.XtraEditors.MemoEdit();
			panelStatus = new DevExpress.XtraEditors.PanelControl();
			labelStatus = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)panelTop).BeginInit();
			panelTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)comboLang.Properties).BeginInit();
			((System.ComponentModel.ISupportInitialize)comboEngine.Properties).BeginInit();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel1).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel2).BeginInit();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)memoText.Properties).BeginInit();
			((System.ComponentModel.ISupportInitialize)panelStatus).BeginInit();
			panelStatus.SuspendLayout();
			SuspendLayout();
			// 
			// panelTop
			// 
			panelTop.Controls.Add(btnClear);
			panelTop.Controls.Add(btnSave);
			panelTop.Controls.Add(btnCopy);
			panelTop.Controls.Add(btnBatch);
			panelTop.Controls.Add(chkHighlight);
			panelTop.Controls.Add(chkUpscale);
			panelTop.Controls.Add(chkBinarize);
			panelTop.Controls.Add(chkContrast);
			panelTop.Controls.Add(chkGray);
			panelTop.Controls.Add(labelPre);
			panelTop.Controls.Add(btnRun);
			panelTop.Controls.Add(comboLang);
			panelTop.Controls.Add(labelLang);
			panelTop.Controls.Add(comboEngine);
			panelTop.Controls.Add(labelEngine);
			panelTop.Controls.Add(btnPaste);
			panelTop.Controls.Add(btnOpen);
			panelTop.Dock = DockStyle.Top;
			panelTop.Location = new Point(0, 0);
			panelTop.Name = "panelTop";
			panelTop.Size = new Size(1000, 86);
			panelTop.TabIndex = 0;
			// 
			// btnClear
			// 
			btnClear.Location = new Point(936, 49);
			btnClear.Name = "btnClear";
			btnClear.Size = new Size(52, 28);
			btnClear.TabIndex = 16;
			btnClear.Text = "지우기";
			// 
			// btnSave
			// 
			btnSave.Location = new Point(858, 49);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(74, 28);
			btnSave.TabIndex = 15;
			btnSave.Text = "저장...";
			// 
			// btnCopy
			// 
			btnCopy.Location = new Point(790, 49);
			btnCopy.Name = "btnCopy";
			btnCopy.Size = new Size(64, 28);
			btnCopy.TabIndex = 14;
			btnCopy.Text = "복사";
			// 
			// btnBatch
			// 
			btnBatch.Location = new Point(443, 49);
			btnBatch.Name = "btnBatch";
			btnBatch.Size = new Size(128, 28);
			btnBatch.TabIndex = 13;
			btnBatch.Text = "여러 이미지 일괄...";
			// 
			// chkHighlight
			// 
			chkHighlight.Location = new Point(335, 50);
			chkHighlight.Name = "chkHighlight";
			chkHighlight.Size = new Size(96, 26);
			chkHighlight.TabIndex = 12;
			chkHighlight.Text = "단어 표시";
			// 
			// chkUpscale
			// 
			chkUpscale.Location = new Point(249, 50);
			chkUpscale.Name = "chkUpscale";
			chkUpscale.Size = new Size(74, 26);
			chkUpscale.TabIndex = 11;
			chkUpscale.Text = "2x 확대";
			// 
			// chkBinarize
			// 
			chkBinarize.Location = new Point(181, 50);
			chkBinarize.Name = "chkBinarize";
			chkBinarize.Size = new Size(64, 26);
			chkBinarize.TabIndex = 10;
			chkBinarize.Text = "이진화";
			// 
			// chkContrast
			// 
			chkContrast.Location = new Point(121, 50);
			chkContrast.Name = "chkContrast";
			chkContrast.Size = new Size(56, 26);
			chkContrast.TabIndex = 9;
			chkContrast.Text = "대비";
			// 
			// chkGray
			// 
			chkGray.Location = new Point(61, 50);
			chkGray.Name = "chkGray";
			chkGray.Size = new Size(56, 26);
			chkGray.TabIndex = 8;
			chkGray.Text = "흑백";
			// 
			// labelPre
			// 
			labelPre.Location = new Point(12, 56);
			labelPre.Name = "labelPre";
			labelPre.Size = new Size(34, 14);
			labelPre.TabIndex = 7;
			labelPre.Text = "전처리:";
			// 
			// btnRun
			// 
			btnRun.Location = new Point(654, 12);
			btnRun.Name = "btnRun";
			btnRun.Size = new Size(150, 28);
			btnRun.TabIndex = 6;
			btnRun.Text = "텍스트 추출 (F5)";
			// 
			// comboLang
			// 
			comboLang.Location = new Point(504, 13);
			comboLang.Name = "comboLang";
			comboLang.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
			comboLang.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			comboLang.Size = new Size(140, 20);
			comboLang.TabIndex = 5;
			// 
			// labelLang
			// 
			labelLang.Location = new Point(467, 19);
			labelLang.Name = "labelLang";
			labelLang.Size = new Size(24, 14);
			labelLang.TabIndex = 4;
			labelLang.Text = "언어:";
			// 
			// comboEngine
			// 
			comboEngine.Location = new Point(289, 13);
			comboEngine.Name = "comboEngine";
			comboEngine.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
			comboEngine.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			comboEngine.Size = new Size(170, 20);
			comboEngine.TabIndex = 3;
			// 
			// labelEngine
			// 
			labelEngine.Location = new Point(252, 19);
			labelEngine.Name = "labelEngine";
			labelEngine.Size = new Size(24, 14);
			labelEngine.TabIndex = 2;
			labelEngine.Text = "엔진:";
			// 
			// btnPaste
			// 
			btnPaste.Location = new Point(134, 12);
			btnPaste.Name = "btnPaste";
			btnPaste.Size = new Size(104, 28);
			btnPaste.TabIndex = 1;
			btnPaste.Text = "붙여넣기 (Ctrl+V)";
			// 
			// btnOpen
			// 
			btnOpen.Location = new Point(12, 12);
			btnOpen.Name = "btnOpen";
			btnOpen.Size = new Size(118, 28);
			btnOpen.TabIndex = 0;
			btnOpen.Text = "이미지 열기 (Ctrl+O)";
			// 
			// splitContainer
			// 
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Location = new Point(0, 86);
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add(labelDropHint);
			splitContainer.Panel1.Text = "Panel1";
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.Controls.Add(memoText);
			splitContainer.Panel2.Text = "Panel2";
			splitContainer.Size = new Size(1000, 494);
			splitContainer.SplitterPosition = 500;
			splitContainer.TabIndex = 1;
			// 
			// labelDropHint
			// 
			labelDropHint.Appearance.Font = new Font("맑은 고딕", 11F);
			labelDropHint.Appearance.Options.UseFont = true;
			labelDropHint.Appearance.Options.UseTextOptions = true;
			labelDropHint.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			labelDropHint.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			labelDropHint.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			labelDropHint.Dock = DockStyle.Fill;
			labelDropHint.Location = new Point(0, 0);
			labelDropHint.Name = "labelDropHint";
			labelDropHint.Size = new Size(353, 60);
			labelDropHint.TabIndex = 0;
			labelDropHint.Text = "여기에 이미지를 드래그앤드롭 하거나\r\n[이미지 열기] 버튼을 누르세요.\r\n(Ctrl+V 로 붙여넣기 · 이미지 위 드래그로 영역 선택)";
			// 
			// memoText
			// 
			memoText.Dock = DockStyle.Fill;
			memoText.Location = new Point(0, 0);
			memoText.Name = "memoText";
			memoText.Properties.Appearance.Font = new Font("맑은 고딕", 10.5F);
			memoText.Properties.Appearance.Options.UseFont = true;
			memoText.Size = new Size(490, 494);
			memoText.TabIndex = 0;
			// 
			// panelStatus
			// 
			panelStatus.Controls.Add(labelStatus);
			panelStatus.Dock = DockStyle.Bottom;
			panelStatus.Location = new Point(0, 580);
			panelStatus.Name = "panelStatus";
			panelStatus.Size = new Size(1000, 28);
			panelStatus.TabIndex = 2;
			// 
			// labelStatus
			// 
			labelStatus.Appearance.Options.UseTextOptions = true;
			labelStatus.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			labelStatus.Dock = DockStyle.Fill;
			labelStatus.Location = new Point(2, 2);
			labelStatus.Name = "labelStatus";
			labelStatus.Padding = new Padding(8, 0, 0, 0);
			labelStatus.Size = new Size(38, 14);
			labelStatus.TabIndex = 0;
			labelStatus.Text = "준비됨";
			// 
			// Form1
			// 
			AllowDrop = true;
			AutoScaleDimensions = new SizeF(7F, 14F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1000, 608);
			Controls.Add(splitContainer);
			Controls.Add(panelStatus);
			Controls.Add(panelTop);
			IconOptions.ShowIcon = false;
			KeyPreview = true;
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "이미지 OCR — 이미지에서 텍스트 추출";
			((System.ComponentModel.ISupportInitialize)panelTop).EndInit();
			panelTop.ResumeLayout(false);
			panelTop.PerformLayout();
			((System.ComponentModel.ISupportInitialize)comboLang.Properties).EndInit();
			((System.ComponentModel.ISupportInitialize)comboEngine.Properties).EndInit();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel1).EndInit();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel2).EndInit();
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)memoText.Properties).EndInit();
			((System.ComponentModel.ISupportInitialize)panelStatus).EndInit();
			panelStatus.ResumeLayout(false);
			panelStatus.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private DevExpress.XtraEditors.PanelControl panelTop;
        private DevExpress.XtraEditors.SimpleButton btnOpen;
        private DevExpress.XtraEditors.SimpleButton btnPaste;
        private DevExpress.XtraEditors.LabelControl labelEngine;
        private DevExpress.XtraEditors.ComboBoxEdit comboEngine;
        private DevExpress.XtraEditors.LabelControl labelLang;
        private DevExpress.XtraEditors.ComboBoxEdit comboLang;
        private DevExpress.XtraEditors.SimpleButton btnRun;
        private DevExpress.XtraEditors.LabelControl labelPre;
        private DevExpress.XtraEditors.CheckButton chkGray;
        private DevExpress.XtraEditors.CheckButton chkContrast;
        private DevExpress.XtraEditors.CheckButton chkBinarize;
        private DevExpress.XtraEditors.CheckButton chkUpscale;
        private DevExpress.XtraEditors.CheckButton chkHighlight;
        private DevExpress.XtraEditors.SimpleButton btnBatch;
        private DevExpress.XtraEditors.SimpleButton btnCopy;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnClear;
        private DevExpress.XtraEditors.SplitContainerControl splitContainer;
        private image_ocr.Controls.ImageCanvas imageCanvas;
        private DevExpress.XtraEditors.LabelControl labelDropHint;
        private DevExpress.XtraEditors.MemoEdit memoText;
        private DevExpress.XtraEditors.PanelControl panelStatus;
        private DevExpress.XtraEditors.LabelControl labelStatus;
    }
}
