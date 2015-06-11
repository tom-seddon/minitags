namespace QuickFind
{
    partial class QuickFindDialog
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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._tagsListView = new QuickFind.TagsBrowserListView();
            this._editBox = new QuickFind.TagsBrowserTextBox();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(228, 219);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(228, 248);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
            // 
            // _tagsListView
            // 
            this._tagsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tagsListView.FullRowSelect = true;
            this._tagsListView.HideSelection = false;
            this._tagsListView.Location = new System.Drawing.Point(12, 12);
            this._tagsListView.MultiSelect = false;
            this._tagsListView.Name = "_tagsListView";
            this._tagsListView.Size = new System.Drawing.Size(291, 201);
            this._tagsListView.TabIndex = 4;
            this._tagsListView.UseCompatibleStateImageBehavior = false;
            this._tagsListView.View = System.Windows.Forms.View.Details;
            this._tagsListView.ItemActivate += new System.EventHandler(this._tagsListView_ItemActivate);
            // 
            // _editBox
            // 
            this._editBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._editBox.Location = new System.Drawing.Point(12, 219);
            this._editBox.Name = "_editBox";
            this._editBox.Size = new System.Drawing.Size(210, 20);
            this._editBox.TabIndex = 0;
            this._editBox.TextChanged += new System.EventHandler(this._editBox_TextChanged);
            // 
            // QuickFindDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(315, 283);
            this.Controls.Add(this._tagsListView);
            this.Controls.Add(this._editBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Name = "QuickFindDialog";
            this.Text = "TagsBrowser";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QuickFindDialog_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickFindDialog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private TagsBrowserTextBox _editBox;
        private TagsBrowserListView _tagsListView;
    }
}