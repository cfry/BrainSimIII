﻿//
// PROPRIETARY AND CONFIDENTIAL
// Brain Simulator 3 v.1.0
// © 2022 FutureAI, Inc., all rights reserved
// 

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace BrainSimulator
{

    public partial class ModuleView
    {
        int firstNeuron = 0;
        string label;
        string moduleTypeStr;
        int color;
        public Modules.ModuleBase theModule = null;
        int width = 0;
        int height = 0;
        [XmlIgnore] //used when displaying the module at small scales
        public System.Windows.Media.Imaging.WriteableBitmap bitmap = null;

        public ModuleView(int firstNeuron1, int width, int height, string theLabel, string theModuleTypeStr, int theColor)
        {
            int index = theModuleTypeStr.IndexOf("(");
            if (index != -1)
                theModuleTypeStr = theModuleTypeStr.Substring(0, index);


            FirstNeuron = firstNeuron1;
            Width = width;
            Height = height;
            Label = theLabel;
            ModuleTypeStr = theModuleTypeStr;
            color = theColor;

            Type t = Type.GetType("BrainSimulator.Modules." + theModuleTypeStr);
            theModule = (Modules.ModuleBase)Activator.CreateInstance(t);
        }

        public ModuleView() { }
        public string Label { get => label.StartsWith("Module") ? label.Replace("Module", "") : label; set => label = value; }
        public int FirstNeuron { get => firstNeuron; set => firstNeuron = value; }
        public int Height
        {
            get => height;
            set
            {
                height = value;
                if (TheModule != null) TheModule.SizeChanged();
            }
        }
        public int Width
        {
            get => width;
            set
            {
                width = value;
                if (TheModule != null) TheModule.SizeChanged();
            }
        }
        public int Color { get => color; set => color = value; }

        public string ModuleTypeStr { get => moduleTypeStr; set => moduleTypeStr = value; }
        private int Rows { get { return MainWindow.theNeuronArray.rows; } }

        public int NeuronCount { get { return Width * Height; } }
        public Modules.ModuleBase TheModule { get => theModule; set => theModule = value; }
        public int LastNeuron { get { return firstNeuron + (height - 1) + Rows * (Width - 1); } }

        public Rectangle GetRectangle(DisplayParams dp)
        {
            Rectangle r = new Rectangle();
            Point p1 = dp.pointFromNeuron(firstNeuron);
            Point p2 = dp.pointFromNeuron(LastNeuron);
            p2.X += dp.NeuronDisplaySize;
            p2.Y += dp.NeuronDisplaySize;
            r.Width = Math.Abs(p2.X - p1.X);
            r.Height = Math.Abs(p2.Y - p1.Y);
            Canvas.SetTop(r, p1.Y);
            Canvas.SetLeft(r, p1.X);
            return r;
        }
    }
}