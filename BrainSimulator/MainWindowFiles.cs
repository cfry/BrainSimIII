﻿//
// PROPRIETARY AND CONFIDENTIAL
// Brain Simulator 3 v.1.0
// © 2022 FutureAI, Inc., all rights reserved
// 

using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static StackPanel loadedModulesSP;
        private async void LoadFile(string fileName)
        {
            CloseAllModuleDialogs();
            CloseHistoryWindow();
            CloseNotesWindow();
            // theNeuronArrayView.theSelection.selectedRectangles.Clear();
            CloseAllModules();
            SuspendEngine();
            Modules.Sallie.VideoQueue.Clear();

            bool success = false;
            // await Task.Run(delegate { success = XmlFile.Load(ref theNeuronArray, fileName); });
            if (!success)
            {
                CreateEmptyNetwork();
                Properties.Settings.Default["CurrentFile"] = currentFileName;
                Properties.Settings.Default.Save();
                ResumeEngine();
                return;
            }
            currentFileName = fileName;

            ReloadNetwork.IsEnabled = true;
            Reload_network.IsEnabled = true;
            if (XmlFile.CanWriteTo(currentFileName))
                SaveButton.IsEnabled = true;
            else
                SaveButton.IsEnabled = false;

            SetTitleBar();
            await Task.Delay(1000).ContinueWith(t => ShowDialogs());
            loadedModulesSP = LoadedModuleSP;
            LoadedModuleSP.Children.Clear();

            for ( int i = 0; i < theNeuronArray.modules.Count; i++ )
            {
                ModuleView na = theNeuronArray.modules[i];
                if (na.TheModule != null)
                {
                    na.TheModule.SetUpAfterLoad();
                }
            }

            ReloadLoadedModules();
            theNeuronArray.LoadComplete = true;

            // if (theNeuronArray.displayParams != null)
            //     theNeuronArrayView.Dp = theNeuronArray.displayParams;

            AddFileToMRUList(currentFileName);
            Properties.Settings.Default["CurrentFile"] = currentFileName;
            Properties.Settings.Default.Save();

            Update();
            SetShowSynapsesCheckBox(theNeuronArray.ShowSynapses);
            SetPlayPauseButtonImage(theNeuronArray.EngineIsPaused);
            SetSliderPosition(theNeuronArray.EngineSpeed);

            engineIsPaused = theNeuronArray.EngineIsPaused;

            engineSpeedStack.Clear();
            engineSpeedStack.Push(theNeuronArray.EngineSpeed);

            if (!engineIsPaused)
                ResumeEngine();

            undoCountAtLastSave = 0;
        }

        public static void ReloadLoadedModules()
        {
            if (loadedModulesSP == null) return;
            loadedModulesSP.Children.Clear();
            System.Collections.Generic.SortedDictionary<string, int> nameList = new();

            //build the sorted list of names and module numbers
            for (int i = 0; i < theNeuronArray.modules.Count; i++)
            {
                nameList.Add(theNeuronArray.modules[i].Label, i);
            }
            //add the modules to the stackPanel
            foreach (var x in nameList)
            {
                ModuleView mv = theNeuronArray.modules[x.Value];
                AddModuleToLoadedModules(x.Value, mv);
            }

            //in case sorting causes an issue, this was the original code
            //for (int i = 0; i < theNeuronArray.modules.Count; i++)
            //{
            //    ModuleView mv = theNeuronArray.modules[i];
            //    AddModuleToLoadedModules(i, mv);
            //}
        }

        private static void AddModuleToLoadedModules(int i, ModuleView na)
        {
            TextBlock tb = new TextBlock();
            tb.Text = na.Label;
            tb.Margin = new Thickness(5, 2, 5, 2);
            tb.Padding = new Thickness(10, 3, 10, 3);
            tb.ContextMenu = new ContextMenu();
            ModuleView.CreateContextMenu(i, na, tb, tb.ContextMenu);
            if (na.TheModule.isEnabled) tb.Background = new SolidColorBrush(Colors.LightGreen);
            else tb.Background = new SolidColorBrush(Colors.Pink);
            loadedModulesSP.Children.Add(tb);
        }

        private bool LoadClipBoardFromFile(string fileName)
        {

            // XmlFile.Load(ref myClipBoard, fileName);

            foreach (ModuleView na in myClipBoard.modules)
            {
                if (na.TheModule != null)
                    na.TheModule.SetUpAfterLoad();
                {
                    try
                    {
                        na.TheModule.SetUpAfterLoad();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("SetupAfterLoad failed on module " + na.Label + ".   Message: " + e.Message);
                    }
                }
            }
            return true;
        }

        private bool SaveFile(string fileName)
        {
            SuspendEngine();
            //If the path contains "bin\64\debug" change the path to the actual development location instead
            //because file in bin..debug can be clobbered on every rebuild.
            if (fileName.ToLower().Contains("bin\\debug\\net6.0-windows"))
            {
                MessageBoxResult mbResult = System.Windows.MessageBox.Show(this, "Save to source folder instead?", "Save", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Asterisk, MessageBoxResult.No);
                if (mbResult == MessageBoxResult.Yes)
                    fileName = fileName.ToLower().Replace("bin\\debug\\net6.0-windows\\", "");
                if (mbResult == MessageBoxResult.Cancel)
                    return false;
            }

            foreach (ModuleView na in theNeuronArray.modules)
            {
                if (na.TheModule != null)
                {
                    try
                    {
                        na.TheModule.SetUpBeforeSave();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("SetupBeforeSave failed on module " + na.Label + ".   Message: " + e.Message);
                    }
                }
            }

            // theNeuronArray.displayParams = theNeuronArrayView.Dp;
            /*
            if (XmlFile.Save(theNeuronArray, fileName))
            {
                currentFileName = fileName;
                SetCurrentFileNameToProperties();
                ResumeEngine();
                undoCountAtLastSave = theNeuronArray.GetUndoCount();
                return true;
            }
            else
            {
                ResumeEngine();
                return false;
            }
            */
            return false;
        }
        private void SaveClipboardToFile(string fileName)
        {
            foreach (ModuleView na in myClipBoard.modules)
            {
                if (na.TheModule != null)
                    na.TheModule.SetUpBeforeSave();
            }

            // if (XmlFile.Save(myClipBoard, fileName))
            //     currentFileName = fileName;
        }

        private void AddFileToMRUList(string filePath)
        {
            StringCollection MRUList = (StringCollection)Properties.Settings.Default["MRUList"];
            if (MRUList == null)
                MRUList = new StringCollection();
            MRUList.Remove(filePath); //remove it if it's already there
            MRUList.Insert(0, filePath); //add it to the top of the list
            Properties.Settings.Default["MRUList"] = MRUList;
            Properties.Settings.Default.Save();
        }

        private void LoadCurrentFile()
        {
            LoadFile(currentFileName);
        }

        private static void SetCurrentFileNameToProperties()
        {
            Properties.Settings.Default["CurrentFile"] = currentFileName;
            Properties.Settings.Default.Save();
        }

        public int undoCountAtLastSave = 0;
        private bool PromptToSaveChanges()
        {
            if (IsArrayEmpty()) return false;
            // if (theNeuronArray.GetUndoCount() == undoCountAtLastSave) return false; //no changes have been made

            bool canWrite = XmlFile.CanWriteTo(currentFileName, out string message);

            SuspendEngine();

            bool retVal = false;
            MessageBoxResult mbResult = System.Windows.MessageBox.Show(this, "Do you want to save changes?", "Save", MessageBoxButton.YesNoCancel,
            MessageBoxImage.Asterisk, MessageBoxResult.No);
            if (mbResult == MessageBoxResult.Yes)
            {
                if (currentFileName != "" && canWrite)
                {
                    // if (SaveFile(currentFileName))
                    //     undoCountAtLastSave = theNeuronArray.GetUndoCount();
                }
                else
                {
                    if (SaveAs())
                    {
                    }
                    else
                    {
                        retVal = true;
                    }
                }
            }
            if (mbResult == MessageBoxResult.Cancel)
            {
                retVal = true;
            }
            ResumeEngine();
            return retVal;
        }
        private bool SaveAs()
        {
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            defaultPath += "\\BrainSim";
            try
            {
                if (Directory.Exists(defaultPath)) defaultPath = "";
                else Directory.CreateDirectory(defaultPath);
            }
            catch
            {
                //maybe myDocuments is readonly of offline? let the user do whatever they want
                defaultPath = "";
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = Utils.FilterXMLs,
                Title = Utils.TitleBrainSimSave,
                InitialDirectory = defaultPath
            };

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            Nullable<bool> result = saveFileDialog1.ShowDialog();
            if (result ?? false)// System.Windows.Forms.DialogResult.OK)
            {
                if (SaveFile(saveFileDialog1.FileName))
                {
                    AddFileToMRUList(currentFileName);
                    SetTitleBar();
                    return true;
                }
            }
            return false;
        }

    }
}