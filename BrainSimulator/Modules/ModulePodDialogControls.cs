﻿//
// PROPRIETARY AND CONFIDENTIAL
// Brain Simulator 3 v.1.0
// © 2022 FutureAI, Inc., all rights reserved
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using static System.Math;
using System.Management;
using System.IO.Ports;

namespace BrainSimulator.Modules
{
    public class ModulePodDialogControls : ModuleBase
    {
        //any public variable you create here will automatically be saved and restored  with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XmlIgnore] 
        //public theStatus = 1;


        //set size parameters as needed in the constructor
        //set max to be -1 if unlimited
        public ModulePodDialogControls()
        {
            minHeight = 1;
            maxHeight = 500;
            minWidth = 1;
            maxWidth = 500;
        }
        List<string> inputLines = new ();
        List<string> incomingBuffer = new ();

        public void AppendInputLine(string incoming)
        {            
            if (incoming == null) return;
            incomingBuffer.Add(incoming);
        }

        void TransferBufferToInput()
        {
            if (incomingBuffer == null || incomingBuffer.Count == 0) return;
            for (int i = 0; i < incomingBuffer.Count; i++)
            {
                inputLines.Add(incomingBuffer[i]);
            }
        }

        public List<string> PullPodOutput()//Need to make async
        {            
            return inputLines;
        }

        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here
            TransferBufferToInput();
            UpdateDialog();
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
        }

        //the following can be used to massage public data to be different in the xml file
        //delete if not needed
        public override void SetUpBeforeSave()
        {
        }
        public override void SetUpAfterLoad()
        {
        }

        //called whenever the size of the module rectangle changes
        //for example, you may choose to reinitialize whenever size changes
        //delete if not needed
        public override void SizeChanged()
        {
            if (mv == null) return; //this is called the first time before the module actually exists
        }
    }
}