using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eto.Forms;

namespace MTRACK;

public class MainForm : Form
{
    public class Thing
    {
        public bool Name { get; set; }
        
        public string Value { get; set; }
    }
    
    public MainForm()
    {
        Title = "MTRACK";

        FilterCollection<Thing> collection = new FilterCollection<Thing>();
        collection.Add(new Thing() { Name = true, Value = "FatFogus" });
        collection.Add(new Thing() { Name = false, Value = "gggg" });
        collection.Refresh();

        GridView gridView = new GridView() { DataStore = collection };
        gridView.CellEdited += (sender, args) => collection.Refresh();
        gridView.Columns.Add(new GridColumn() { DataCell = new CheckBoxCell("Name") { }, Editable = true });
        gridView.Columns.Add(new GridColumn() { DataCell = new TextBoxCell("Value") { }, Editable = true });

        Content = gridView;
    }
}