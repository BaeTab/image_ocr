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
			btnRun = new DevExpress.XtraEditors.SimpleButton();
			comboEngine = new DevExpress.XtraEditors.ComboBoxEdit();
			labelEngine = new DevExpress.XtraEditors.LabelControl();
			btnPaste = new DevExpress.XtraEditors.SimpleButton();
			btnOpen = new DevExpress.XtraEditors.SimpleButton();
			splitContainer = new DevExpress.XtraEditors.SplitContainerControl();
			pictureEdit = new DevExpress.XtraEditors.PictureEdit();
			labelDropHint = new DevExpress.XtraEditors.LabelControl();
			memoText = new DevExpress.XtraEditors.MemoEdit();
			panelStatus = new DevExpress.XtraEditors.PanelControl();
			labelStatus = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)panelTop).BeginInit();
			panelTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)comboEngine.Properties).BeginInit();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel1).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel2).BeginInit();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureEdit.Properties).BeginInit();
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
			panelTop.Controls.Add(btnRun);
			panelTop.Controls.Add(comboEngine);
			panelTop.Controls.Add(labelEngine);
			panelTop.Controls.Add(btnPaste);
			panelTop.Controls.Add(btnOpen);
			panelTop.Dock = DockStyle.Top;
			panelTop.Location = new Point(0, 0);
			panelTop.Name = "panelTop";
			panelTop.Size = new Size(1000, 52);
			panelTop.TabIndex = 0;
			// 
			// btnClear
			// 
			btnClear.Location = new Point(921, 12);
			btnClear.Name = "btnClear";
			btnClear.Size = new Size(66, 28);
			btnClear.TabIndex = 7;
			btnClear.Text = "지우기";
			// 
			// btnSave
			// 
			btnSave.Location = new Point(825, 12);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(90, 28);
			btnSave.TabIndex = 6;
			btnSave.Text = "저장...";
			// 
			// btnCopy
			// 
			btnCopy.Location = new Point(739, 12);
			btnCopy.Name = "btnCopy";
			btnCopy.Size = new Size(80, 28);
			btnCopy.TabIndex = 5;
			btnCopy.Text = "복사";
			// 
			// btnRun
			// 
			btnRun.Location = new Point(583, 12);
			btnRun.Name = "btnRun";
			btnRun.Size = new Size(150, 28);
			btnRun.TabIndex = 4;
			btnRun.Text = "텍스트 추출 (F5)";
			// 
			// comboEngine
			// 
			comboEngine.Location = new Point(341, 14);
			comboEngine.Name = "comboEngine";
			comboEngine.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
			comboEngine.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			comboEngine.Size = new Size(230, 20);
			comboEngine.TabIndex = 3;
			// 
			// labelEngine
			// 
			labelEngine.Location = new Point(304, 19);
			labelEngine.Name = "labelEngine";
			labelEngine.Size = new Size(24, 14);
			labelEngine.TabIndex = 2;
			labelEngine.Text = "엔진:";
			// 
			// btnPaste
			// 
			btnPaste.Location = new Point(158, 12);
			btnPaste.Name = "btnPaste";
			btnPaste.Size = new Size(130, 28);
			btnPaste.TabIndex = 1;
			btnPaste.Text = "붙여넣기 (Ctrl+V)";
			// 
			// btnOpen
			// 
			btnOpen.Location = new Point(12, 12);
			btnOpen.Name = "btnOpen";
			btnOpen.Size = new Size(140, 28);
			btnOpen.TabIndex = 0;
			btnOpen.Text = "이미지 열기 (Ctrl+O)";
			// 
			// splitContainer
			// 
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Location = new Point(0, 52);
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add(pictureEdit);
			splitContainer.Panel1.Controls.Add(labelDropHint);
			splitContainer.Panel1.Text = "Panel1";
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.Controls.Add(memoText);
			splitContainer.Panel2.Text = "Panel2";
			splitContainer.Size = new Size(1000, 528);
			splitContainer.SplitterPosition = 500;
			splitContainer.TabIndex = 1;
			// 
			// pictureEdit
			// 
			pictureEdit.Dock = DockStyle.Fill;
			pictureEdit.Location = new Point(0, 0);
			pictureEdit.Name = "pictureEdit";
			pictureEdit.Properties.Appearance.BackColor = Color.FromArgb(37, 37, 38);
			pictureEdit.Properties.Appearance.Options.UseBackColor = true;
			pictureEdit.Properties.ShowMenu = false;
			pictureEdit.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
			pictureEdit.Size = new Size(500, 528);
			pictureEdit.TabIndex = 1;
			pictureEdit.Visible = false;
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
			labelDropHint.Size = new Size(255, 60);
			labelDropHint.TabIndex = 0;
			labelDropHint.Text = "여기에 이미지를 드래그앤드롭 하거나\r\n[이미지 열기] 버튼을 누르세요.\r\n(Ctrl+V 로 붙여넣기도 됩니다)";
			// 
			// memoText
			// 
			memoText.Dock = DockStyle.Fill;
			memoText.Location = new Point(0, 0);
			memoText.Name = "memoText";
			memoText.Properties.Appearance.Font = new Font("맑은 고딕", 10.5F);
			memoText.Properties.Appearance.Options.UseFont = true;
			memoText.Size = new Size(490, 528);
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
			((System.ComponentModel.ISupportInitialize)comboEngine.Properties).EndInit();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel1).EndInit();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer.Panel2).EndInit();
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureEdit.Properties).EndInit();
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
        private DevExpress.XtraEditors.SimpleButton btnRun;
        private DevExpress.XtraEditors.SimpleButton btnCopy;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnClear;
        private DevExpress.XtraEditors.SplitContainerControl splitContainer;
        private DevExpress.XtraEditors.LabelControl labelDropHint;
        private DevExpress.XtraEditors.PictureEdit pictureEdit;
        private DevExpress.XtraEditors.MemoEdit memoText;
        private DevExpress.XtraEditors.PanelControl panelStatus;
        private DevExpress.XtraEditors.LabelControl labelStatus;
    }
}
