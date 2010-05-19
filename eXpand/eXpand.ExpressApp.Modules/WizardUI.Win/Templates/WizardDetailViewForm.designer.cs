// <autogenerated />
namespace eXpand.ExpressApp.WizardUI.Win.Templates
{
    partial class WizardDetailViewForm
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
            this.wizardControl = new DevExpress.XtraWizard.WizardControl();
            this.completionWizardPage1 = new DevExpress.XtraWizard.CompletionWizardPage();
            this.showRecordAfterCompletion = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl)).BeginInit();
            this.wizardControl.SuspendLayout();
            this.completionWizardPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.showRecordAfterCompletion.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // wizardControl
            // 
            this.wizardControl.Controls.Add(this.completionWizardPage1);
            this.wizardControl.Image = global::eXpand.ExpressApp.WizardUI.Win.Properties.Resources.wizard_image;
            this.wizardControl.Name = "wizardControl";
            this.wizardControl.Pages.AddRange(new DevExpress.XtraWizard.BaseWizardPage[] {
            this.completionWizardPage1});
            this.wizardControl.UseAcceptButton = false;
            // 
            // completionWizardPage1
            // 
            this.completionWizardPage1.Controls.Add(this.showRecordAfterCompletion);
            this.completionWizardPage1.Name = "completionWizardPage1";
            this.completionWizardPage1.Size = new System.Drawing.Size(575, 433);
            // 
            // showRecordAfterCompletion
            // 
            this.showRecordAfterCompletion.Location = new System.Drawing.Point(3, 95);
            this.showRecordAfterCompletion.Name = "showRecordAfterCompletion";
            this.showRecordAfterCompletion.Properties.Caption = "Show record after completing the wizard";
            this.showRecordAfterCompletion.Size = new System.Drawing.Size(219, 19);
            this.showRecordAfterCompletion.TabIndex = 7;
            // 
            // WizardDetailViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.wizardControl);
            this.Name = "WizardDetailViewForm";
            this.Text = "SimpleForm";
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl)).EndInit();
            this.wizardControl.ResumeLayout(false);
            this.completionWizardPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.showRecordAfterCompletion.Properties)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private DevExpress.XtraWizard.WizardControl wizardControl;
        private DevExpress.XtraEditors.CheckEdit showRecordAfterCompletion;
        private DevExpress.XtraWizard.CompletionWizardPage completionWizardPage1;
    }
}