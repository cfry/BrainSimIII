﻿//
// PROPRIETARY AND CONFIDENTIAL
// Brain Simulator 3 v.1.0
// © 2022 FutureAI, Inc., all rights reserved
// 

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrainSimulator.Modules
{
    public partial class ModuleEventSequencingDlg : ModuleBaseDlg
    {
        public ModuleEventSequencingDlg()
        {
            InitializeComponent();
        }

        public override bool Draw(bool checkDrawTimer)
        {
            if (!base.Draw(checkDrawTimer)) return false;
            //this has a timer so that no matter how often you might call draw, the dialog
            //only updates 10x per second

            //use a line like this to gain access to the parent's public variables
            //ModuleEmpty parent = (ModuleEmpty)base.ParentModule;

            //here are some other possibly-useful items
            //theGrid.Children.Clear();
            //Point windowSize = new Point(theGrid.ActualWidth, theGrid.ActualHeight);
            //Point windowCenter = new Point(windowSize.X / 2, windowSize.Y / 2);
            //float scale = (float)Math.Min(windowSize.X, windowSize.Y) / 12;
            //if (scale == 0) return false;

            return true;
        }

        private void TheGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw(false);
        }

        private void StopLightTestButton_Click(object sender, RoutedEventArgs e)
        {
            ((ModuleEventSequencing)ParentModule).StoplightEvents();
        }

        private void UpdateSituationButton_Click(object sender, RoutedEventArgs e)
        {
            SituationEntry();
        }

        private void situationTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter) SituationEntry();
        }

        private void SituationEntry()
        {
            ((ModuleEventSequencing)ParentModule).UpdateSituation(situationTextBox.Text);
            HistoryTextBlock.Text += "\nSituation: " + situationTextBox.Text;
        }

        private void TakeActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionEntry();
        }

        private void ActionTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ActionEntry();
        }

        private void ActionEntry()
        {
            ((ModuleEventSequencing)ParentModule).TakeAction(actionTextBox.Text);
            HistoryTextBlock.Text += "\nAction: " + actionTextBox.Text;
        }
    }
}