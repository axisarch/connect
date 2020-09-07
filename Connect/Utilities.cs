using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.DocObjects;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;

namespace Connect
{
    public static class Util
    {
        /// <summary>
        /// Limit a value to a range
        /// </summary>
        /// <param name="value">Value to limit</param>
        /// <param name="inclusiveMinimum">Minimum value</param>
        /// <param name="inlusiveMaximum">Maximum value</param>
        /// <returns>Limited Value</returns>
        public static T LimitToRange<T>(IComparable<T> value, T inclusiveMinimum, T inlusiveMaximum)
        {

            if (value.CompareTo(inclusiveMinimum) == 0 | value.CompareTo(inclusiveMinimum) == 1)
            {
                if (value.CompareTo(inlusiveMaximum) == -1 | value.CompareTo(inlusiveMaximum) == 0)
                {
                    return (T)value;
                }

                return inlusiveMaximum;
            }

            return inclusiveMinimum;
        }
    }
}

/// <summary>
/// This namespace provides functions for canvas manipulation in Grasshopper
/// </summary
namespace Canvas
{
    /// <summary>
    /// This class provides functions for components
    /// </summary>
    class Component
    {
        static public void SetValueList(GH_Document doc, GH_Component comp, int InputIndex, List<KeyValuePair<string, string>> valuePairs, string name)
        {
            if (valuePairs.Count == 0) return;
            GH_DocumentIO docIO = new GH_DocumentIO();
            docIO.Document = new GH_Document();

            if (docIO.Document == null) return;
            doc.MergeDocument(docIO.Document);

            docIO.Document.SelectAll();
            docIO.Document.ExpireSolution();
            docIO.Document.MutateAllIds();
            IEnumerable<IGH_DocumentObject> objs = docIO.Document.Objects;
            doc.DeselectAll();
            doc.UndoUtil.RecordAddObjectEvent("Create Accent List", objs);
            doc.MergeDocument(docIO.Document);

            doc.ScheduleSolution(10, chanegValuelist);


            void chanegValuelist(GH_Document document)
            {

                IList<IGH_Param> sources = comp.Params.Input[InputIndex].Sources;
                int inputs = sources.Count;


                // If nothing has been conected create a new component
                if (inputs == 0)
                {
                    //instantiate  new value list and clear it
                    GH_ValueList vl = new GH_ValueList();
                    vl.ListItems.Clear();
                    vl.NickName = name;
                    vl.Name = name;

                    //Create values for list and populate it
                    for (int i = 0; i < valuePairs.Count; ++i)
                    {
                        var item = new GH_ValueListItem(valuePairs[i].Key, valuePairs[i].Value);
                        vl.ListItems.Add(item);
                    }

                    //Add value list to the document
                    document.AddObject(vl, false, 1);

                    //get the pivot of the "accent" param
                    System.Drawing.PointF currPivot = comp.Params.Input[InputIndex].Attributes.Pivot;
                    //set the pivot of the new object
                    vl.Attributes.Pivot = new System.Drawing.PointF(currPivot.X - 210, currPivot.Y - 11);

                    // Connect to input
                    comp.Params.Input[InputIndex].AddSource(vl);
                }

                // If inputs exist replace the existing ones
                else
                {
                    for (int i = 0; i < inputs; ++i)
                    {
                        if (sources[i].Name == "Value List" | sources[i].Name == name)
                        {
                            //instantiate  new value list and clear it
                            GH_ValueList vl = new GH_ValueList();
                            vl.ListItems.Clear();
                            vl.NickName = name;
                            vl.Name = name;

                            //Create values for list and populate it
                            for (int j = 0; j < valuePairs.Count; ++j)
                            {
                                var item = new GH_ValueListItem(valuePairs[j].Key, valuePairs[j].Value);
                                vl.ListItems.Add(item);
                            }

                            document.AddObject(vl, false, 1);
                            //set the pivot of the new object
                            vl.Attributes.Pivot = sources[i].Attributes.Pivot;

                            var currentSource = sources[i];
                            comp.Params.Input[InputIndex].RemoveSource(sources[i]);

                            currentSource.IsolateObject();
                            document.RemoveObject(currentSource, false);

                            //Connect new vl
                            comp.Params.Input[InputIndex].AddSource(vl);
                        }
                        else
                        {
                            //Do nothing if it dosent mach any of the above
                        }
                    }
                }
            }
        }

        public static void ChangeObjects(IEnumerable<IGH_Param> items, IGH_Param newObject)
        {
            foreach (IGH_Param item in items)
            {
                //get the input it is connected to
                if (item.Recipients.Count == 0) return;
                var parrent = item.Recipients[0];

                GH_DocumentIO docIO = new GH_DocumentIO();
                docIO.Document = new GH_Document();

                //get active GH doc
                GH_Document doc = item.OnPingDocument();
                if (doc == null) return;
                if (docIO.Document == null) return;

                Component.AddObject(docIO, newObject, parrent, item.Attributes.Pivot);
                Component.MergeDocuments(docIO, doc, $"Create {newObject.Name}");

                doc.RemoveObject(item, false);
                parrent.AddSource(newObject);
            }
        }

        public static GH_ValueList CreateValueList(string name, Dictionary<string, string> valuePairs)
        {
            //initialize object
            Grasshopper.Kernel.Special.GH_ValueList vl = new Grasshopper.Kernel.Special.GH_ValueList();
            //clear default contents
            vl.ListItems.Clear();

            //set component nickname
            vl.NickName = name;
            vl.Name = name;

            foreach (KeyValuePair<string, string> entety in valuePairs)
            {
                GH_ValueListItem vi = new GH_ValueListItem(entety.Key, entety.Value);
                vl.ListItems.Add(vi);
            }

            return vl;
        }
        public static GH_NumberSlider CreateNumbersilder(string name, decimal min, decimal max, int precision = 0, int length = 174)
        {
            var nS = new GH_NumberSlider();
            nS.ClearData();

            //Naming
            nS.Name = name;
            nS.NickName = name;

            nS.Slider.Minimum = min;
            nS.Slider.Maximum = max;

            nS.Slider.DecimalPlaces = Connect.Util.LimitToRange(precision, 0,12);

            if (precision == 0)
                nS.Slider.Type = Grasshopper.GUI.Base.GH_SliderAccuracy.Integer;
            else
                nS.Slider.Type = Grasshopper.GUI.Base.GH_SliderAccuracy.Float;

            nS.CreateAttributes();
            var bounds = nS.Attributes.Bounds;
            bounds.Width = length;
            nS.Attributes.Bounds = bounds;

            nS.SetSliderValue(min);
            return nS;
        }

        // private methods to magee the placement of ne objects
        static void AddObject(GH_DocumentIO docIO, IGH_Param Object, IGH_Param param, PointF location = new PointF())
        {
            // place the object
            docIO.Document.AddObject(Object, false, 1);

            //get the pivot of the "accent" param
            System.Drawing.PointF currPivot = param.Attributes.Pivot;

            if (location == new PointF()) Object.Attributes.Pivot = new System.Drawing.PointF(currPivot.X - 120, currPivot.Y - 11);
            //set the pivot of the new object
            else Object.Attributes.Pivot = location;
        }
        static void MergeDocuments(GH_DocumentIO docIO, GH_Document doc, string name = "Merge")
        {
            docIO.Document.SelectAll();
            docIO.Document.ExpireSolution();
            docIO.Document.MutateAllIds();
            IEnumerable<IGH_DocumentObject> objs = docIO.Document.Objects;
            doc.DeselectAll();
            doc.UndoUtil.RecordAddObjectEvent(name, objs);
            doc.MergeDocument(docIO.Document);
            //doc.ScheduleSolution(10);
        }

    }
    class Menu
    {
        /// <summary>
        /// Uncheck other dropdown menu items
        /// </summary>
        /// <param name="selectedMenuItem"></param>
        static public void UncheckOtherMenuItems(ToolStripMenuItem selectedMenuItem)
        {
            selectedMenuItem.Checked = true;

            // Select the other MenuItens from the ParentMenu(OwnerItens) and unchecked this,
            // The current Linq Expression verify if the item is a real ToolStripMenuItem
            // and if the item is a another ToolStripMenuItem to uncheck this.
            foreach (var ltoolStripMenuItem in (from object
                                                    item in selectedMenuItem.Owner.Items
                                                let ltoolStripMenuItem = item as ToolStripMenuItem
                                                where ltoolStripMenuItem != null
                                                where !item.Equals(selectedMenuItem)
                                                select ltoolStripMenuItem))
                (ltoolStripMenuItem).Checked = false;

            // This line is optional, for show the mainMenu after click
            //selectedMenuItem.Owner.Show();
        }
    }
}
