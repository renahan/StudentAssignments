using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pro_Kroger_App
{
    public partial class MapForm : Form
    {
        string AisleNum;
        public MapForm()
        {
            InitializeComponent();
        }

        private void MapForm_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = MapDS.Tables[1];				//View ShoppingList items
            dataGridView1.Columns[2].Width = 200;					//Datagridview formatting. Only shows the item description and location.
            for (int i = 0; i < 12; i++)
            {
                dataGridView1.Columns[i].Visible = false;
            }
            dataGridView1.Columns[2].Width = 149;
            dataGridView1.Columns[2].Visible = true;
            dataGridView1.Columns[7].Width = 55;
            dataGridView1.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[7].Visible = true;
            dataGridView1.ClearSelection();
        }

        public DataSet DataSource
        {
            set
            {
                MapDS = value;
            }
        }
        
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Close();								//Close the window
        }
		
        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            ClearAisles();							//Set all image objects on map to invisible

            Selections();							//Cycle through shoppinglist to show the image object corresponding with the selected item  
        }

        private void SelectAllBtn_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.MultiSelect = true;

            dataGridView1.SelectAll();
            Selections();
        }

        private void ClearAisles()
        {   //Set all image objects on map to invisible
            Aisle1.Visible = false;

            Aisle2.Visible = false;

            Aisle3.Visible = false;

            Aisle4.Visible = false;

            Aisle5.Visible = false;

            Aisle6.Visible = false;

            Aisle7.Visible = false;

            Aisle8.Visible = false;

            Aisle9.Visible = false;

            Aisle10.Visible = false;

            Aisle11.Visible = false;

            Aisle12.Visible = false;

            Aisle13.Visible = false;

            Aisle14.Visible = false;

            Aisle15.Visible = false;

            Aisle16.Visible = false;

            Aisle17.Visible = false;

            Aisle18.Visible = false;

            Aisle19.Visible = false;

            Aisle20.Visible = false;

            Aisle21.Visible = false;

            Aisle99.Visible = false;

            AislePharm.Visible = false;
        }
		
        private void Selections()
        {
			//Cycle through shoppinglist to show the image object corresponding with the selected item
            var i = 0;

            for (i = 0; i < dataGridView1.SelectedRows.Count; i++)
            {
                AisleNum = Convert.ToString(dataGridView1.SelectedRows[i].Cells["Location"].Value);
                if (AisleNum == "1")
                {
                    Aisle1.Visible = true;
                }
                if (AisleNum == "2")
                {
                    Aisle2.Visible = true;
                }
                if (AisleNum == "3")
                {
                    Aisle3.Visible = true;
                }
                if (AisleNum == "4")
                {
                    Aisle4.Visible = true;
                }
                if (AisleNum == "5")
                {
                    Aisle5.Visible = true;
                }
                if (AisleNum == "6")
                {
                    Aisle6.Visible = true;
                }
                if (AisleNum == "7")
                {
                    Aisle7.Visible = true;
                }
                if (AisleNum == "8")
                {
                    Aisle8.Visible = true;
                }
                if (AisleNum == "9")
                {
                    Aisle9.Visible = true;
                }
                if (AisleNum == "10")
                {
                    Aisle10.Visible = true;
                }
                if (AisleNum == "11")
                {
                    Aisle11.Visible = true;
                }
                if (AisleNum == "12")
                {
                    Aisle12.Visible = true;
                }
                if (AisleNum == "13")
                {
                    Aisle13.Visible = true;
                }
                if (AisleNum == "14")
                {
                    Aisle14.Visible = true;
                }
                if (AisleNum == "15")
                {
                    Aisle15.Visible = true;
                }
                if (AisleNum == "16")
                {
                    Aisle16.Visible = true;
                }
                if (AisleNum == "17")
                {
                    Aisle17.Visible = true;
                }
                if (AisleNum == "18")
                {
                    Aisle18.Visible = true;
                }
                if (AisleNum == "19")
                {
                    Aisle19.Visible = true;
                }
                if (AisleNum == "20")
                {
                    Aisle20.Visible = true;
                }
                if (AisleNum == "21")
                {
                    Aisle21.Visible = true;
                }
                if (AisleNum == "99")
                {
                    Aisle99.Visible = true;
                }
                if (AisleNum == "Pharm")
                {
                    AislePharm.Visible = true;
                }
            }
        }
    }
}
