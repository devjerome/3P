﻿#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileTagsPage.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;

namespace _3PA.MainFeatures.FilesInfoNs {
    public partial class FileTagsPage : YamuiPage {

        #region fields

        public FileTagObject LocFileTagObject;
        public string Filename;

        #endregion

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public FileTagsPage() {
            InitializeComponent();

            UpdateInfo();

            // add event handlers
            yamuiComboBox1.SelectedIndexChanged += SelectedIndexChanged;
            KeyDown += OnKeyDown;
            btok.ButtonPressed += BtokOnButtonPressed;
            btcancel.ButtonPressed += BtcancelOnButtonPressed;
            btclear.ButtonPressed += BtclearOnButtonPressed;
            btdefault.ButtonPressed += BtdefaultOnButtonPressed;
            bttoday.ButtonPressed += BttodayOnButtonPressed;
            bttoday.Click += (sender, args) => { yamuiTextBox6.Text = DateTime.Now.ToString("dd/MM/yy"); };
        }

        #endregion

        #region public

        /// <summary>
        /// Call this method to update the content of the form according to the current document
        /// </summary>
        public void UpdateInfo() {
            Filename = Npp.GetCurrentFileName();

            // populate combobox
            var lst = new List<ItemCombo> {
                new ItemCombo {DisplayText = "Last info", Nb = FileTag.LastTag},
                new ItemCombo {DisplayText = "Default info", Nb = FileTag.DefaultTag}
            };
            yamuiComboBox1.DisplayMember = "DisplayText";
            yamuiComboBox1.ValueMember = "Nb";

            if (FileTag.Contains(Filename)) {

                var lastItem = FileTag.GetLastFileTag(Filename);
                LocFileTagObject = lastItem;

                var i = 2;
                var lastItemPos = 0;
                foreach (var fileTag in FileTag.GetFileTagsList(Filename)) {
                    lst.Add(new ItemCombo { DisplayText = Filename + " #" + fileTag.Nb, Nb = fileTag.Nb });
                    if (fileTag.Nb == lastItem.Nb) lastItemPos = i;
                    i++;
                }

                yamuiComboBox1.DataSource = lst;
                yamuiComboBox1.SelectedIndex = lastItemPos;

                var itemsOfFile = FileTag.GetFileTagsList(Filename);
                lst.AddRange(itemsOfFile.Select(item => new ItemCombo { DisplayText = item.Nb, Nb = item.Nb }));
            } else {
                LocFileTagObject = FileTag.GetFileTags(Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? FileTag.DefaultTag : FileTag.LastTag, "");

                yamuiComboBox1.DataSource = lst;
                yamuiComboBox1.SelectedIndex = Config.Instance.UseDefaultValuesInsteadOfLastValuesInEditTags ? 1 : 0;
            }
            UpdateView();

            ActiveControl = yamuiComboBox1;
        }

        #endregion

        #region private event

        private void BtokOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            Save(Filename);
            Save(FileTag.LastTag);
            FileTag.Export();
            FileTag.Cloak();
        }

        private void BtcancelOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            UpdateView();
            FileTag.Cloak();
        }

        private void BtclearOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            LocFileTagObject = new FileTagObject();
            UpdateView();
        }

        private void BtdefaultOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            Save(FileTag.DefaultTag);
        }

        private void BttodayOnButtonPressed(object sender, ButtonPressedEventArgs buttonPressedEventArgs) {
            yamuiTextBox6.Text = DateTime.Now.ToString("dd/MM/yy");
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.KeyCode == Keys.Escape)
                FileTag.Cloak();
        }

        #endregion

        #region private data management

        /// <summary>
        /// Save the info
        /// </summary>
        /// <param name="filename"></param>
        private void Save(string filename) {
            UpdateModel();
            FileTag.SetFileTags(filename, LocFileTagObject.Nb, LocFileTagObject.Date, LocFileTagObject.Text, LocFileTagObject.NomAppli, LocFileTagObject.Version, LocFileTagObject.Chantier, LocFileTagObject.Jira);
        }

        private void UpdateModel() {
            LocFileTagObject.NomAppli = yamuiTextBox1.Text;
            LocFileTagObject.Version = yamuiTextBox2.Text;
            LocFileTagObject.Chantier = yamuiTextBox4.Text;
            LocFileTagObject.Jira = yamuiTextBox3.Text;
            LocFileTagObject.Nb = yamuiTextBox5.Text;
            LocFileTagObject.Text = yamuiTextBox7.Text;
            LocFileTagObject.Date = yamuiTextBox6.Text;
        }

        private void UpdateView() {
            yamuiTextBox1.Text = LocFileTagObject.NomAppli;
            yamuiTextBox2.Text = LocFileTagObject.Version;
            yamuiTextBox4.Text = LocFileTagObject.Chantier;
            yamuiTextBox3.Text = LocFileTagObject.Jira;
            yamuiTextBox5.Text = LocFileTagObject.Nb;
            yamuiTextBox7.Text = LocFileTagObject.Text;
            yamuiTextBox6.Text = LocFileTagObject.Date;
        }

        /// <summary>
        /// called when the user changes the value of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedIndexChanged(object sender, EventArgs e) {
            var val = yamuiComboBox1.SelectedValue.ToString();
            if (val == FileTag.LastTag || val == FileTag.DefaultTag)
                LocFileTagObject = FileTag.GetFileTags(val, "");
            else
                LocFileTagObject = FileTag.GetFileTags(Filename, val);
            UpdateView();
        }

        #endregion

    }

    #region item combo struct

    public struct ItemCombo {
        public string DisplayText { get; set; }
        public string Nb { get; set; }
    }

    #endregion

}