﻿using System;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms
{
    public partial class SAV_Trainer8 : Form
    {
        private readonly SaveFile Origin;
        private readonly SAV8SWSH SAV;

        public SAV_Trainer8(SaveFile sav)
        {
            InitializeComponent();
            WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);
            SAV = (SAV8SWSH)(Origin = sav).Clone();
            //Loading = true;
            if (Main.Unicode)
            {
                TB_OTName.Font = FontUtil.GetPKXFont();
            }

            B_MaxCash.Click += (sender, e) => MT_Money.Text = SAV.MaxMoney.ToString();

            CB_Gender.Items.Clear();
            CB_Gender.Items.AddRange(Main.GenderSymbols.Take(2).ToArray()); // m/f depending on unicode selection

            TrainerStats.LoadRecords(SAV, Records.RecordList_8);

            NUD_BP.Value = SAV.Misc.BP;
            GetComboBoxes();
            GetTextBoxes();

            TC_Editor.TabPages.Remove(Tab_BadgeMap); // needs more work

            //Loading = false;
        }

        // private readonly bool Loading;
        // private bool MapUpdated;

        private void GetComboBoxes()
        {
            CB_Language.InitializeBinding();
            CB_Language.DataSource = GameInfo.LanguageDataSource(SAV.Generation);
        }

        private void GetTextBoxes()
        {
            // Get Data
            CB_Game.SelectedIndex = SAV.Game - (int)GameVersion.SW;
            CB_Gender.SelectedIndex = SAV.Gender;

            // Display Data
            TB_OTName.Text = SAV.OT;
            trainerID1.LoadIDValues(SAV);
            MT_Money.Text = SAV.Money.ToString();
            CB_Language.SelectedValue = SAV.Language;

            //NUD_M.Value = SAV.Situation.M;
            //// Sanity Check Map Coordinates
            //try
            //{
            //    NUD_X.Value = (decimal)SAV.Situation.X;
            //    NUD_Z.Value = (decimal)SAV.Situation.Z;
            //    NUD_Y.Value = (decimal)SAV.Situation.Y;
            //    NUD_R.Value = (decimal)SAV.Situation.R;
            //}
            //catch { GB_Map.Enabled = false; }

            // Load Play Time
            MT_Hours.Text = SAV.PlayedHours.ToString();
            MT_Minutes.Text = SAV.PlayedMinutes.ToString();
            MT_Seconds.Text = SAV.PlayedSeconds.ToString();

            //if (SAV.Played.LastSavedDate.HasValue)
            //{
            //    CAL_LastSavedDate.Value = SAV.Played.LastSavedDate.Value;
            //    CAL_LastSavedTime.Value = SAV.Played.LastSavedDate.Value;
            //}
            //else
            //{
                L_LastSaved.Visible = CAL_LastSavedDate.Visible = CAL_LastSavedTime.Visible = false;
            //}

            L_Started.Visible = CAL_AdventureStartDate.Visible = CAL_AdventureStartTime.Visible = false;
            L_Fame.Visible = CAL_HoFDate.Visible = CAL_HoFTime.Visible = false;
            // Util.GetDateTime2000(SAV.SecondsToStart, out var date, out var time);
            // CAL_AdventureStartDate.Value = date;
            // CAL_AdventureStartTime.Value = time;
            // 
            // Util.GetDateTime2000(SAV.SecondsToFame, out date, out time);
            // CAL_HoFDate.Value = date;
            // CAL_HoFTime.Value = time;
        }

        private void Save()
        {
            SaveTrainerInfo();
        }

        private void SaveTrainerInfo()
        {
            SAV.Game = (byte)(CB_Game.SelectedIndex + 30);
            SAV.Gender = (byte)CB_Gender.SelectedIndex;

            SAV.Money = Util.ToUInt32(MT_Money.Text);
            SAV.Language = WinFormsUtil.GetIndex(CB_Language);
            SAV.OT = TB_OTName.Text;

            SAV.Misc.BP = (int)NUD_BP.Value;

            // Copy Position
            //if (GB_Map.Enabled && MapUpdated)
            //{
            //    SAV.Situation.M = (int)NUD_M.Value;
            //    SAV.Situation.X = (float)NUD_X.Value;
            //    SAV.Situation.Z = (float)NUD_Z.Value;
            //    SAV.Situation.Y = (float)NUD_Y.Value;
            //    SAV.Situation.R = (float)NUD_R.Value;
            //    SAV.Situation.UpdateOverworldCoordinates();
            //}

            // Save PlayTime
            SAV.PlayedHours = ushort.Parse(MT_Hours.Text);
            SAV.PlayedMinutes = ushort.Parse(MT_Minutes.Text)%60;
            SAV.PlayedSeconds = ushort.Parse(MT_Seconds.Text)%60;

            //SAV.SecondsToStart = (uint)Util.GetSecondsFrom2000(CAL_AdventureStartDate.Value, CAL_AdventureStartTime.Value);
            //SAV.SecondsToFame = (uint)Util.GetSecondsFrom2000(CAL_HoFDate.Value, CAL_HoFTime.Value);
            //
            //if (SAV.Played.LastSavedDate.HasValue)
            //    SAV.Played.LastSavedDate = new DateTime(CAL_LastSavedDate.Value.Year, CAL_LastSavedDate.Value.Month, CAL_LastSavedDate.Value.Day, CAL_LastSavedTime.Value.Hour, CAL_LastSavedTime.Value.Minute, 0);
        }

        private void ClickOT(object sender, MouseEventArgs e)
        {
            TextBox tb = sender as TextBox ?? TB_OTName;
            // Special Character Form
            if (ModifierKeys != Keys.Control)
                return;

            var d = new TrashEditor(tb, null, SAV);
            d.ShowDialog();
            tb.Text = d.FinalString;
        }

        private void B_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void B_Save_Click(object sender, EventArgs e)
        {
            Save();
            Origin.CopyChangesFrom(SAV);
            Close();
        }

        private void Change255(object sender, EventArgs e)
        {
            MaskedTextBox box = (MaskedTextBox)sender;
            if (box.Text.Length == 0) box.Text = "0";
            if (Util.ToInt32(box.Text) > 255) box.Text = "255";
        }

        private void ChangeMapValue(object sender, EventArgs e)
        {
            //if (!Loading)
            //    MapUpdated = true;
        }

        //private string UpdateTip(int index)
        //{
        //    switch (index)
        //    {
        //        case 2: // Storyline Completed Time
        //            var seconds = Util.GetSecondsFrom2000(CAL_AdventureStartDate.Value, CAL_AdventureStartTime.Value);
        //            return Util.ConvertDateValueToString(SAV.GetRecord(index), seconds);
        //        default:
        //            return null;
        //    }
        //}
    }
}