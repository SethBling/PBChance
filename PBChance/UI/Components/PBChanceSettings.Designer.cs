namespace PBChance.UI.Components
{
    partial class PBChanceSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RecentLabel = new System.Windows.Forms.Label();
            this.PercentOfAttempts = new System.Windows.Forms.RadioButton();
            this.FixedAttempts = new System.Windows.Forms.RadioButton();
            this.AttemptCountBox = new System.Windows.Forms.NumericUpDown();
            this.CreditsLabel = new System.Windows.Forms.Label();
            this.DisplayOddsCheckbox = new System.Windows.Forms.CheckBox();
            this.IgnoreRunCountBox = new System.Windows.Forms.CheckBox();
            this.AttemptsAfter = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.AttemptCountBox)).BeginInit();
            this.SuspendLayout();
            // 
            // RecentLabel
            // 
            this.RecentLabel.AutoSize = true;
            this.RecentLabel.Location = new System.Drawing.Point(3, 55);
            this.RecentLabel.Name = "RecentLabel";
            this.RecentLabel.Size = new System.Drawing.Size(87, 13);
            this.RecentLabel.TabIndex = 1;
            this.RecentLabel.Text = "Use most recent:";
            // 
            // PercentOfAttempts
            // 
            this.PercentOfAttempts.AutoSize = true;
            this.PercentOfAttempts.Checked = true;
            this.PercentOfAttempts.Location = new System.Drawing.Point(150, 32);
            this.PercentOfAttempts.Name = "PercentOfAttempts";
            this.PercentOfAttempts.Size = new System.Drawing.Size(118, 17);
            this.PercentOfAttempts.TabIndex = 2;
            this.PercentOfAttempts.TabStop = true;
            this.PercentOfAttempts.Text = "Percent of Attempts";
            this.PercentOfAttempts.UseVisualStyleBackColor = true;
            // 
            // FixedAttempts
            // 
            this.FixedAttempts.AutoSize = true;
            this.FixedAttempts.Location = new System.Drawing.Point(150, 55);
            this.FixedAttempts.Name = "FixedAttempts";
            this.FixedAttempts.Size = new System.Drawing.Size(66, 17);
            this.FixedAttempts.TabIndex = 3;
            this.FixedAttempts.TabStop = true;
            this.FixedAttempts.Text = "Attempts";
            this.FixedAttempts.UseVisualStyleBackColor = true;
            // 
            // AttemptCountBox
            // 
            this.AttemptCountBox.Location = new System.Drawing.Point(93, 53);
            this.AttemptCountBox.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.AttemptCountBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.AttemptCountBox.Name = "AttemptCountBox";
            this.AttemptCountBox.Size = new System.Drawing.Size(51, 20);
            this.AttemptCountBox.TabIndex = 1;
            this.AttemptCountBox.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // CreditsLabel
            // 
            this.CreditsLabel.AutoSize = true;
            this.CreditsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CreditsLabel.Location = new System.Drawing.Point(3, 13);
            this.CreditsLabel.Name = "CreditsLabel";
            this.CreditsLabel.Size = new System.Drawing.Size(141, 13);
            this.CreditsLabel.TabIndex = 4;
            this.CreditsLabel.Text = "PBChance by SethBling";
            // 
            // DisplayOddsCheckbox
            // 
            this.DisplayOddsCheckbox.AutoSize = true;
            this.DisplayOddsCheckbox.Location = new System.Drawing.Point(6, 98);
            this.DisplayOddsCheckbox.Name = "DisplayOddsCheckbox";
            this.DisplayOddsCheckbox.Size = new System.Drawing.Size(125, 17);
            this.DisplayOddsCheckbox.TabIndex = 5;
            this.DisplayOddsCheckbox.Text = "Display Odds (1 in N)";
            this.DisplayOddsCheckbox.UseVisualStyleBackColor = true;
            // 
            // IgnoreRunCountBox
            // 
            this.IgnoreRunCountBox.AutoSize = true;
            this.IgnoreRunCountBox.Location = new System.Drawing.Point(6, 121);
            this.IgnoreRunCountBox.Name = "IgnoreRunCountBox";
            this.IgnoreRunCountBox.Size = new System.Drawing.Size(205, 17);
            this.IgnoreRunCountBox.TabIndex = 6;
            this.IgnoreRunCountBox.Text = "Use All Runs (Ignore Attempt Counter)";
            this.IgnoreRunCountBox.UseVisualStyleBackColor = true;
            // 
            // AttemptsAfter
            // 
            this.AttemptsAfter.AutoSize = true;
            this.AttemptsAfter.Location = new System.Drawing.Point(150, 78);
            this.AttemptsAfter.Name = "AttemptsAfter";
            this.AttemptsAfter.Size = new System.Drawing.Size(124, 17);
            this.AttemptsAfter.TabIndex = 7;
            this.AttemptsAfter.TabStop = true;
            this.AttemptsAfter.Text = "Attempts After Run #";
            this.AttemptsAfter.UseVisualStyleBackColor = true;
            // 
            // PBChanceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AttemptsAfter);
            this.Controls.Add(this.IgnoreRunCountBox);
            this.Controls.Add(this.DisplayOddsCheckbox);
            this.Controls.Add(this.CreditsLabel);
            this.Controls.Add(this.AttemptCountBox);
            this.Controls.Add(this.FixedAttempts);
            this.Controls.Add(this.PercentOfAttempts);
            this.Controls.Add(this.RecentLabel);
            this.Name = "PBChanceSettings";
            this.Size = new System.Drawing.Size(289, 141);
            ((System.ComponentModel.ISupportInitialize)(this.AttemptCountBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label RecentLabel;
        private System.Windows.Forms.RadioButton PercentOfAttempts;
        private System.Windows.Forms.RadioButton FixedAttempts;
        private System.Windows.Forms.RadioButton AttemptsAfter;
        private System.Windows.Forms.NumericUpDown AttemptCountBox;
        private System.Windows.Forms.Label CreditsLabel;
        private System.Windows.Forms.CheckBox DisplayOddsCheckbox;
        private System.Windows.Forms.CheckBox IgnoreRunCountBox;
    }
}
