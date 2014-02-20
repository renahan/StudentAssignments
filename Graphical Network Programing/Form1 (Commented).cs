//Assignemnt 3
//GNP
//Renny Yeager & Cody Moore
//Kroger Desktop Shopping List Application

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pro_Kroger_App
{
    
    public partial class Form1 : Form
    {
        string rrypath = @"C:\Users\Robert Yeager\Desktop\Kroger Development\Kroger_App_2\Pro_Kroger_App\Resources\";
        string jcmpath = @"";
        string ucpath = @"\\clusterfs.ceas1.uc.edu\students\yeagerrr\Desktop\Kroger Dev\Kroger_App_2\Pro_Kroger_App\Resources\";
        string path;
        SqlConnection KrogerConnection = new SqlConnection();
        SqlCommand KrogerCommand = new SqlCommand();
        SqlDataAdapter KrogerDataAdapter = new SqlDataAdapter();
        public DataSet ServerEntriesDS = new DataSet();
        string krogerplusnumber;
        bool connectivityflag;
        string ManualOrServer;
        bool activestatus;
        int userlines;

        public Form1()
        {
            InitializeComponent();
        }

        //**********************************************************************************
        //Application Setup
        //**********************************************************************************

        private void Form1_Load(object sender, EventArgs e)
        {
            path = ucpath;
            KrogerConnection.ConnectionString = "Data Source=10.63.1.116;Initial Catalog=kroger;User ID=moore2jc;Password=LAXHERO34";
            CheckConnectivity();
            backgroundpicture.Location = new Point(12, 59);							//Set up Shopping cart background picture
            backgroundpicture.Size = new Size(706, 500);
            userlines = Convert.ToInt32(UserData.ReadXml(path + "UserData.xml"));	//Checks if a user is present in the UserData table, which would mean they are still logged in
            ShoppingListViewBtn.Enabled = false;
            CouponViewBtn.Enabled = false;
            MapBtn.Enabled = false;
            AddItemBtn.Enabled = false;
            UpdateItembtn.Enabled = false;
            DeleteBtn.Enabled = false;
            if (userlines == 0)							//If no user is logged in, make log in panel available
            {
                Log_in_outBtn.Text = "Log in";
                Log_in_outBtn.PerformClick();
                dataGridView1.Visible = false;
            }
            if (userlines == 2)							//If a user exists in CML (logged in), prep app with their data. 
            {
                Log_in_outBtn.Text = "Log out";
                krogerplusnumber = UserData.Rows[0][0].ToString();	//load Kroger Plus Number from XML
                GetUserInfo();										//Get Info requests user info from Users table on server, if connected, and loads into User info panel
                appstartup();										//Appstartup loads manual entries from xml, and server entries if disconnected. Calls functions to set up ServerEntries data table and Datagridview1 (main datagridview)
            }
        }
        
        //Exit Button
        //#########################################################
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();									//Exit the application and write the data from the datasets into user specific xml files
            if (krogerplusnumber != null)
            {
                ManualEntries.WriteXml(path + krogerplusnumber + "_ManualEntries.xml", XmlWriteMode.DiffGram);
                ServerEntriesDS.Tables["ServerEntries"].WriteXml(path + krogerplusnumber + "_OfflineServerEntries.xml");
                UserData.WriteXml(path + "UserData.xml"); 		
            }
        }

        //Check for connectivity
        //#########################################################
        private void CheckConnectivity()						
        {
			//CheckConnectivity runs on startup to attempt to connect with the server
			//if the connection is successful, the Online status indicator is set to 
			//green and the connectivity flag is set to true.
            try
            {
                KrogerConnection.Open();
                Statuslbl.Visible = true;
                Statuslbl.Text = "Connection Successful!";
                listBoxStatus.Items.Add("Connection Successful!");
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                connectivityflag = true;
                OnlineStatuslbl.Text = "Online:";
                OnlineShape.BackColor = Color.LawnGreen;
            }
			//if there is no connectivity, the online status indicator is set to red and connectivity flag is set to false
            catch (Exception e)
            {
                listBoxStatus.Items.Add("Connection Open Error: " + e.ToString());
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                Statuslbl.Visible = true;
                Statuslbl.Text = "Connection to Database Failed.  Check Configuration.";
                Cursor.Current = Cursors.Default;
                connectivityflag = false;
                OnlineStatuslbl.Text = "Offline:";
                OnlineShape.BackColor = Color.Red;
            }
            try
            {
                KrogerConnection.Close();
                //Statuslbl.Text = "Connection Closed.";
                listBoxStatus.Items.Add("Connection Closed to " + "ServerEntries");
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            catch (Exception e)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add("Connection Close Error: " + e.ToString());
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
        }

        //Get user info
        //##########################################################
        private void GetUserInfo()
        {	
			//Get User Info retrieves use info with a given Kroger plus number
			//If there is connectivity it will pull in the data from the server
			//if there is no connectivity, and a user was still logged on upon
			//closing the application, the users info will still be loaded and 
			//offline functionality will still be available
			// Note: logging in while offline is not supported
            if (connectivityflag == true)
            {
                KrogerCommand.CommandType = CommandType.Text;
                KrogerCommand.CommandText = "SELECT FirstName,LastName,CurrentGasPoints From dbo.Users WHERE KrogerPlusNumber = " + krogerplusnumber;
                KrogerCommand.Parameters.Clear();
                KrogerCommand.Connection = KrogerConnection;
                SQLExecution("USERLOOKUP", null);
                dataGridViewSideView.DataSource = UserData;
                dataGridViewSideView.Refresh();
            }

            CusAcctNumLbl.Text = krogerplusnumber;
            CustNameLbl.Text = UserData.Rows[0][1].ToString() + " " + UserData.Rows[0][2].ToString();
            CusGasPtsLbl.Text = UserData.Rows[0][3].ToString();
        }

        //App Startup configuration
        //#########################################################
        private void appstartup()
        {   //Appstartup configures the app for either online or offline use
			//and runs functions to set up the datagridview.
			//It also enables the main functionality buttons of the app
			
            DSSetup();							//DSSetup sets up the ServerEntries table
			
            if (connectivityflag == false)		//if the app is offline, it calls read_in_offlineXML, which
            {									//loads the server entries from the previous session
                read_in_offlineXML();
            }
            LoadServerEntries();				//Triggers stored procedures to load POS entries to ListItems (server table where user items are stored), 
												//downloads the listItems table and downloads relevant coupon information
												
            if (File.Exists(path + krogerplusnumber + "_ManualEntries.xml"))	//if a user specific xml exists, load their manual entries.
            {
                ManualEntries.ReadXml(path + krogerplusnumber + "_ManualEntries.xml");
            }
            else																//if not, create an xml for a new user
            {
                ManualEntries.WriteXml(path + krogerplusnumber + "_ManualEntries.xml");
            }
            AddmanualentriestoSL(true, true, false);							//Loads server and manual entries to ShoppingList table (Holds both server and manual entries)
																				// true (add server); true (add manual); false (add all existing items)
            DGVsetup();							//Configures datagridview1 columns
			
            
            dataGridView1.ClearSelection();		//Preparation for app use
            dataGridView1.Visible = true;
            backgroundpicture.Visible = false;
            ShoppingListViewBtn.Enabled = true;
            CouponViewBtn.Enabled = true;
            MapBtn.Enabled = true;
            AddItemBtn.Enabled = true;
            UpdateItembtn.Enabled = true;
            DeleteBtn.Enabled = true;
        }

        //Read in offlineXML
        //#########################################################
        private void read_in_offlineXML()
        {	//Reads in data from Offlineserverentries.xml to ServerEntries table. This data was saved from the previous session
            try
            {
                listBoxStatus.Items.Add("Reading ...");
                int count = Convert.ToInt16(ServerEntriesDS.Tables["ServerEntries"].ReadXml(path + krogerplusnumber + "_OfflineServerEntries.xml"));
                dataGridView1.DataSource = ServerEntriesDS.Tables["ServerEntries"];
                listBoxStatus.Items.Add("Read " + count + " lines.");
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            catch (Exception ex)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add(ex.Message);
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
        }

        //Offline DataSet Setup
        //#########################################################
        private void DSSetup()
        {	//Cofigures ServerEntries table
			//Early on in development there was a need for a more dynamic table. 
			//This table could possibly be made from the GUI designer.
            if (ServerEntriesDS.Tables.Contains("ServerEntries"))
            {
                ServerEntriesDS.Tables.Remove("ServerEntries");	
            }
            ServerEntriesDS.Tables.Add("ServerEntries");
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("ItemActive", typeof(System.Boolean));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("ListItemUPC", typeof(System.String));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("ProductDescription", typeof(System.String));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("Price", typeof(System.Decimal));//System.Data.SqlTypes.SqlMoney));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("Quantity", typeof(System.Int32));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("Category", typeof(System.String));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("Department", typeof(System.String));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("Location", typeof(System.String));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("ListItemCouponUPC", typeof(System.String));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("CouponValue", typeof(System.Decimal));//System.Data.SqlTypes.SqlMoney));
            ServerEntriesDS.Tables["ServerEntries"].Columns.Add("ItemID", typeof(System.String));
        }

        //datagridview1 setup
        //#########################################################
        private void DGVsetup()
        {	//Configures datagridview1 column widths and text alignment. Hides tables that the user doesn't need to see
            dataGridView1.Columns[0].Width = 60;
            dataGridView1.Columns[1].Width = 70;
            dataGridView1.Columns[2].Width = 185;
            dataGridView1.Columns[3].Width = 40;
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[4].Width = 50;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[5].Visible = false;
            dataGridView1.Columns[6].Width = 65;
            dataGridView1.Columns[7].Width = 50;
            dataGridView1.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[8].Width = 110;
            dataGridView1.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[9].Width = 75;
            dataGridView1.Columns[9].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[10].Visible = false;
            dataGridView1.Columns[11].Visible = false;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
			
			for (int i = 1; i < 11; i++)		//Makes all rows read only, except first column (Item Active), which allows users to iteract with checkboxs
            {
                dataGridView1.Columns[i].ReadOnly = true;
            }
        }
        
        //Load Shopping List
        //#########################################################
	//Prepares KrogerCommand (sqlcommand) to Load POS items to ListItems Table
	//Downloads items from ListItems and Products Tables
	//Downloads related coupon information
        private void LoadServerEntries()
        {		
            KrogerCommand.CommandType = CommandType.StoredProcedure;	//Giving a kroger plus number, Last sync date and last login, 
            KrogerCommand.CommandText = "Last_POS_Transactions";	//the Items within POS system the were entered after the last sync
            KrogerCommand.Parameters.Clear();				//but before the latest login will be added to ListItems
            KrogerCommand.Connection = KrogerConnection;
            KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);
            KrogerCommand.Parameters.AddWithValue("@LastSync", 2012-12-3);	//hardcoded lastsynctime because we were not able to implement storage of timestamp before due date
            KrogerCommand.Parameters.AddWithValue("@LastLogin", DateTime.Now);

            SQLExecution("SELECT", "ServerEntries");	

            KrogerCommand.CommandType = CommandType.StoredProcedure;	//stored procedure will retrieve user specific items given a Kroger plus number
            KrogerCommand.CommandText = "User_Shopping_List";
            KrogerCommand.Parameters.Clear();
            KrogerCommand.Connection = KrogerConnection;
            KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);

            SQLExecution("SELECT", "ServerEntries");
	    dataGridView1.DataSource = ServerEntriesDS.Tables[tablename];

            KrogerCommand.CommandType = CommandType.StoredProcedure;	//Stored procedure downloads data for coupons that matched bought Items in ListItems
            KrogerCommand.CommandText = "User_Coupon_List";
            KrogerCommand.Parameters.Clear();
            KrogerCommand.Connection = KrogerConnection;
            KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);

            SQLExecution("SELECT", "Coupons");
        }
        
        //Add Item to ServerEntries (ListItems) table in Server
        //##########################################################
	//This function calls a stored procedure that adds a new item to the ListItems table on the server. Provided is the user's KP number, 
	//the UPC of the desired item and the desired quantity. After this function LoadServerEntries is called to download a fresh copy 
	//of the ListItems table, innerjoined with the item info from the Products table on the server.
        private void AddItem()
        {
            KrogerCommand.CommandType = CommandType.StoredProcedure;
            KrogerCommand.CommandText = "Add_Item_To_ListItems";
            KrogerCommand.Parameters.Clear();
            KrogerCommand.Connection = KrogerConnection;
            KrogerCommand.Parameters.AddWithValue("@ItemUPC", AddUPCMaskedTextBox.Text);
            KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);
            KrogerCommand.Parameters.AddWithValue("@Quantity", AddQtyUpDown.Value);

            SQLExecution("INSERT", "ListItems");
        }

        //Manual or Server Entry Check in datagridview1
        //##########################################################
	//Determines if the row that has been selected if a manual entry or server entry based on
	//the existence of the itemID which is a unique identifier for the server entries
        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {	
            try
            {
                string selectedID = Convert.ToString(dataGridView1.SelectedRows[0].Cells["ItemID"].Value);	//retrieves the ItemID of the item
                if (selectedID == "")										//if it is null, it is a manual item
                {
                    ManualOrServer = "Manual";
                }

                if (selectedID != "")										//If it is not null, it is a server item
                {
                    ManualOrServer = "Server";
                }
            }
            catch (DataException ex)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add(ex.Message);
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
        }

        //Get Active State from datagridview1
        //##########################################################
	//Receives the state of a checkbox within the ItemActive column
	//If it is a server item it will sync the state with the item data in ListItems on the sever
	//if it is a manual item it will synce that data with the manual dataset
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {	
            if (dataGridView1.CurrentCell.ColumnIndex == 0)
            {
                DataGridViewCheckBoxCell checkbox = (DataGridViewCheckBoxCell)dataGridView1.CurrentCell;
                activestatus = (bool)checkbox.EditedFormattedValue;

                int ID = 0;
                if (ManualOrServer == "Server")
                {
                    string selectedID = Convert.ToString(dataGridView1.SelectedRows[0].Cells["ItemID"].Value);	//gets ItemActive state
                    if (selectedID != "")	//if the itemid exists, synce the state with the server
                    {
                        ID = Convert.ToInt16(selectedID);
                        KrogerCommand.CommandType = CommandType.StoredProcedure;
                        KrogerCommand.CommandText = "Update_Active_In_ListItems";
                        KrogerCommand.Parameters.Clear();
                        KrogerCommand.Connection = KrogerConnection;
                        KrogerCommand.Parameters.AddWithValue("@ItemUPC", dataGridView1.SelectedRows[0].Cells["ListItemUPC"].Value);
                        KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);
                        KrogerCommand.Parameters.AddWithValue("@Active", activestatus);
                        SQLExecution("UPDATE", "ListItems");
                    }
                    ServerEntriesDS.Tables["ServerEntries"].Clear();	//clear the old data from ServerEntries
                    ShoppingList.Clear();								//Clear ShoppingList (What is displayed to user)
                    LoadServerEntries();								//re-download of the of server entries
                    AddmanualentriestoSL(true, true, false);			// rewrite server entries (true) and manual entries (true) to Shoppinglist table. False write all items
                    dataGridView1.Refresh();							
                }
                if (ManualOrServer == "Manual")
                {
                    string getmanualID = Convert.ToString(dataGridView1.SelectedRows[0].Cells[11].Value);	//gets manualID (unique identifier)
                    if (getmanualID != "")	//if it exists load into ID to pass to DSSearchandDestroy
                    {	
                        ID = Convert.ToInt32(getmanualID);
                    }
                    DSSearchandDestroy(ID, "UpdateActive");		//Cycles through the manual entries table to find selected row with manual id from datagridview1 and updates itemactive state
                    Price_CoupTotal();					//calculates current acive item price and coupon totals
                }
            }
        }


        //Login Panel
        //#########################################################
		// the following functions handle the log in process
		// the log in panel is made visible. app functionality is disabled until
		// kroger plus number is entered to log in user.
		// Upon log out the user specific manual and server entries are saved to xml
		// Note: Log in button toggles (log in/Log out)
        private void Log_in_outBtn_Click(object sender, EventArgs e)
        {	
            if (Log_in_outBtn.Text == "Log in")			//If logging in
            {
                loginpanel.Location = new Point(742, 120);	//make the log in panel visble and move to appropiate location
                loginpanel.Visible = true;
                LoginEnterBtn.Enabled = false;
                KPNmaskedTextBox.Clear();			//clear previous entry
            }
            if(Log_in_outBtn.Text == "Log out")			//If Logging out
            {
                Log_in_outBtn.Text = "Log in";
                ManualEntries.WriteXml(path + krogerplusnumber + "_ManualEntries.xml", XmlWriteMode.DiffGram);		//save user specific manual entries to xml
                ManualEntries.Clear();											//Clear manual entries datatable
                ServerEntriesDS.Tables["ServerEntries"].WriteXml(path + krogerplusnumber + "_OfflineServerEntries.xml");//Save user specific Server Entries to xml
                ServerEntriesDS.Tables["ServerEntries"].Clear();							//Clear Server Entries datatable
		krogerplusnumber = null;										//Clear the KP number                
                UserData.Clear();					//Clear out UserData
                ShoppingList.Clear();					//Clear out ShoppingList datatable (Source of Datagridview1)
                UserData.WriteXml(path + "UserData.xml");		//Overwrite the Userdata table xml with blank entry
                dataGridView1.Visible = false;				//Configure various settings for logged off view; disable app function buttons
                backgroundpicture.Location = new Point(12, 59);
                backgroundpicture.Size = new Size(706, 500);
                backgroundpicture.Visible = true;
                CusAcctNumLbl.Text = "";
                CustNameLbl.Text = "";
                CusGasPtsLbl.Text = "";
                ShoppingListViewBtn.Enabled = false;
                CouponViewBtn.Enabled = false;
                MapBtn.Enabled = false;
                AddItemBtn.Enabled = false;
                UpdateItembtn.Enabled = false;
                DeleteBtn.Enabled = false;
                CouponTotalLbl.Visible = false;
                PriceTotalLbl.Visible = false;
            }
        }

        private void LoginEnterBtn_Click(object sender, EventArgs e)
        {
            Log_in_outBtn.Text = "Log out";					//reset log in button to log out 
            krogerplusnumber = KPNmaskedTextBox.Text;				//save the KP number
            listBoxStatus.Items.Add("User logged in as " + krogerplusnumber);
            CusAcctNumLbl.Text = krogerplusnumber;
            GetUserInfo();							//Retrieve user info from server (with connectivity)
            loginpanel.Visible = false;
            CouponTotalLbl.Visible = true;
            PriceTotalLbl.Visible = true;
            appstartup();							//Reconfigure the app for logged in user 
        }
        
        private void LoginCancelBtn_Click(object sender, EventArgs e)
        {
            KPNmaskedTextBox.Clear();						//Clear KP number entry
            loginpanel.Visible = false;						//Remove Login panel
        }

        private void KPNmaskedTextBox_TextChanged(object sender, EventArgs e)
        {	
            if (KPNmaskedTextBox.MaskFull)					//If a user has entered a full KP number (10 digits, no alpha) the enter button is enabled
            {						
                LoginEnterBtn.Enabled = true;
            }
            else
            {
                LoginEnterBtn.Enabled = false;					//Unless there are 10 digits, the enter button remains disabled
            }
        }

        private void KPNmaskedTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            KPNmaskedTextBox.SelectionStart = 0;				//Upon mouse click into KPNmaskedTextBox, the carrot is moved to the left side of the textbox
        }


        //**********************************************************************************
        // Adding, Updating and Deleting an Item 
        //**********************************************************************************

        //Adding an Item to List
        //##########################################################
	//The following Event Handlers involve interaction with the AddItem Panel

        private void AddItemBtn_Click(object sender, EventArgs e)
        {   
	    //The addItembtn brings up the AddItem panel in its correct location and clears away any previous entries.
	    //UPCMaskedTextbox is given focus and the carrot is set to the left so the user can begin typing UPC code immediately.
	    //The enter button is disabled before required information has been entered.

            AddItemPanel.Visible = true;
            AddItemPanel.Location = new Point(185, 214);
            AddDescTextBox.Text = null;
            AddPriceUpDown.Value = 1;
            AddQtyUpDown.Value = 1;
            AddUPCMaskedTextBox.Clear();
            AddItemEnterBtn.Enabled = false;
            AddCoupontextBox.Enabled = true;
            AddCoupontextBox.Clear();
            AddCoupValueUpDown.Enabled = true;
            AddCoupValueUpDown.Refresh();
            AddDescTextBox.Enabled = true;
            AddDescTextBox.Clear();
            AddDeptComboBox.Enabled = true;
            AddDeptComboBox.Text = "Select Department";
            AddAisleComboBox.Enabled = true;
            AddAisleComboBox.Text = "Aisle #";
            AddPriceUpDown.Enabled = true;
            AddPriceUpDown.Refresh();
            AddUPCMaskedTextBox.Focus();
            AddUPCMaskedTextBox.SelectionStart = 0;
        }
	

        private void AddUPCTextBox_TextChanged(object sender, EventArgs e)
        {
            if (AddUPCMaskedTextBox.MaskFull)					//if AddUPCMaskedTextBox has received a ten digit UPC code the
            {									//information for the item is retrieved and loaded into the appropriate fields
                UPCLookUp();							//All of the fields, except quantity, are disabled so the user cannot edit the data
            }
            if (AddUPCMaskedTextBox.MaskFull == false)
            {									//if AddUPCMaskedTextBox is empty all of the other fields are enabled 
                AddItemEnterBtn.Enabled = false;				//to allow user entry for adding a manual item
                AddCoupontextBox.Enabled = true;				//if the user deletes the UPC they entered, the fields will be re-enabled as well.
                AddDescTextBox.Enabled = true;
                AddDescTextBox.Enabled = true;
                AddDeptComboBox.Enabled = true;
                AddAisleComboBox.Enabled = true;
                AddPriceUpDown.Enabled = true;
                AddCoupontextBox.Clear();
                AddDescTextBox.Clear();
                AddDeptComboBox.Text = "Select Department";
                AddAisleComboBox.Text = "Aisle #";
                AddPriceUpDown.Value = 1;
            }
        }	

        //Look up UPC 
        //##########################################################
	//This function is triggered once a user has put in the full UPC code for an item in the AddItemPanel
	//It populates all of the available data for the item into their respective fields so the user
	//can preview the item before they add it to their list
        private void UPCLookUp()
        {
            KrogerCommand.CommandType = CommandType.StoredProcedure;
            KrogerCommand.CommandText = "Crap_We_Need";
            KrogerCommand.Parameters.Clear();
            KrogerCommand.Connection = KrogerConnection;
            //KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);
            KrogerCommand.Parameters.AddWithValue("@ItemUPC", AddUPCMaskedTextBox.Text);
            SQLExecution("UPCLOOKUP", null);

            AddItemEnterBtn.Enabled = true;		//the enter button is enabled and all of the data fields are disabled so the user cannot edit the data. 
            AddCoupontextBox.Enabled = false;		//Quatity selection is still enabled.
            AddCoupValueUpDown.Enabled = false;
            AddDescTextBox.Enabled = false;
            AddDeptComboBox.Enabled = false;
            AddAisleComboBox.Enabled = false;
            AddPriceUpDown.Enabled = false;


        }

        private void AddEnterBtn_Click(object sender, EventArgs e)
        {
            int rowindex = dataGridView1.RowCount + 1;
            if(AddUPCMaskedTextBox.MaskFull == true)				//Checking if AddUPCMaskedTextBox is full determines if the entry is for an item from the server			
            {									//if it is not full, a manual entry will be made
                AddItem();
                ServerEntriesDS.Tables["ServerEntries"].Clear();
                LoadServerEntries();
                dataGridView1.Refresh();
                ShoppingList.Clear();
                AddmanualentriestoSL(true, true, false);
            }

            if (AddDescTextBox.Text != null && AddUPCMaskedTextBox.MaskFull == false)	//if AddUPCMaskedTextBox is not full and text has been entered into AddDescTextBox 
            {										//it is determined that a manual entry will be made
                if (ManualEntries.Rows.Count != 0)
                {
                    for (int i = 0; i < ManualEntries.Rows.Count; i++)
                    {
                        string ManualDesc = Convert.ToString(ManualEntries.Rows[i][0]);
                        if (ManualDesc == AddDescTextBox.Text)					//If the manual item alreadey exists, this snippet grabs the manualID and searches the manualentries table to increase the quantity of the item
                        {
                            decimal AddedManQTY = Convert.ToInt32(ManualEntries.Rows[i][2]) + AddQtyUpDown.Value;
                            int ManualID = Convert.ToInt32(ManualEntries.Rows[i][7]);
                            ManualEntries.Rows[i][2] = AddedManQTY;
                            for (int j = 0; j < ServerEntriesDS.Tables["ServerEntries"].Rows.Count; j++)
                            {
                                if (Convert.ToString(ServerEntriesDS.Tables["ServerEntries"].Rows[j]["Manual ID"]) != Convert.ToString(ManualID))
                                {
                                    continue;
                                }
                                if (Convert.ToInt32(ServerEntriesDS.Tables["ServerEntries"].Rows[j]["Manual ID"]) == ManualID)
                                {
                                    decimal AddedShopQTY = Convert.ToInt32(ServerEntriesDS.Tables["ServerEntries"].Rows[j][4]) + AddQtyUpDown.Value;
                                    ServerEntriesDS.Tables["ServerEntries"].Rows[j][4] = AddedShopQTY;
                                    listBoxStatus.Items.Add("Manual ID: " + ManualID + " New Qty: " + AddedShopQTY);
                                }
                            }
                            break;
                        }
                        else
                        {
                           Addmanualentries();				//Adds the manual entry to the manualentries table 
                           AddmanualentriestoSL(false, true, true);	//adds manual entries to Shoppinglist Table (false: no server entries, true: add manual entries, true: add the lateset entry from the table
                            break;
                        }
                    }
                }
                else
                {
                    Addmanualentries();					//Functions are called here to handle a new manual entry 
                    AddmanualentriestoSL(false, true, true);
                }
                    
            }
            AddItemPanel.Visible = false;				//Closes additem panel
        }

        private void AddUPCMaskedTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            AddUPCMaskedTextBox.Focus();				//On mouse click, AddUPCMaskedTextBox is given focus and carrot is moved to the left
            AddUPCMaskedTextBox.SelectionStart = 0;
        }

        private void AddCoupontextBox_MouseClick(object sender, MouseEventArgs e)
        {	
            AddCoupontextBox.Focus();					//On mouse click, AddUPCMaskedTextBox is given focus and carrot is moved to the left
            AddCoupontextBox.SelectionStart = 0;
        }

        private void AddDescTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            AddDescTextBox.Focus();
            AddDescTextBox.SelectionStart = 0;				//On mouse click, AddDescTextBox is given focus and carrot is moved to the left
        }

        private void AddDescTextBox_TextChanged(object sender, EventArgs e)
        {

            if (AddDescTextBox.TextLength == 0)				//If the AddDescTextBox is empty, the AddUPCMaskedTextBox is enabled
            {
                AddUPCMaskedTextBox.Enabled = true;
            }
            if (AddDescTextBox.TextLength > 0 && AddUPCMaskedTextBox.MaskFull == false)	//if at any point any length of text has been entered in AddDescTextBox and if AddUPCMaskedTextBox is empty, AddUPCMaskedTextBox is disabled
            {
                AddUPCMaskedTextBox.Enabled = false;
            }
        }

        private void AddDescTextBox_Leave(object sender, EventArgs e)
        {
            if (AddDescTextBox.Text != null && AddUPCMaskedTextBox.MaskFull == false)	//if there is text in AddDescTextBox and AddUPCMaskedTextBox is empty, the additemEnterbtn is enabled
            {
                NotFoundlbl.Visible = false;
                AddItemEnterBtn.Enabled = true;
                //DescTextBoxflag = true;
            }
            if (AddDescTextBox.TextLength == 0)				//if the AddDescTextBox has been emptied, the AddUPCMaskedTextBox is re-enabled for entry
            {
                AddUPCMaskedTextBox.Enabled = true;
            }
        }

        private void AddItemCancelBtn_Click(object sender, EventArgs e)	//if the user cancels adding an item, all the fields are cleared and the panel is hiden.
        {
            AddItemPanel.Visible = false;
            AddUPCMaskedTextBox.Clear();
            AddDescTextBox.Clear();
            AddPriceUpDown.Refresh();
            AddDeptComboBox.Text = "Select Department";
            AddAisleComboBox.Text = "Aisle #";
            AddQtyUpDown.Value = 1;
        }

        private void AddAisleComboBox_MouseClick(object sender, MouseEventArgs e)	//if AddAisleCombobox is clicked on, it makes the menu drop down, instead of the default funtion of editing the text
        {
            AddAisleComboBox.DroppedDown = true;
        }

        private void AddDeptComboBox_MouseClick(object sender, MouseEventArgs e)	//if AddDeptComboBox is clicked on, the menu drops.
        {
            AddDeptComboBox.DroppedDown = true;
        }

        private void AddAisleComboBox_SelectedValueChanged(object sender, EventArgs e)	//When a user picks an aisle number, the corresponding department is automatically selected. 
        {
            if (AddAisleComboBox.Text == "1")
            {
                AddDeptComboBox.Text = "Grocery";
            }
            if (AddAisleComboBox.Text == "2")
            {
                AddDeptComboBox.Text = "Non-food";
            }
            if (AddAisleComboBox.Text == "6")
            {
                AddDeptComboBox.Text = "Personal Care";
            }
            if (AddAisleComboBox.Text == "7")
            {
                AddDeptComboBox.Text = "Household";
            }
            if (AddAisleComboBox.Text == "8")
            {
                AddDeptComboBox.Text = "Grocery";
            }
            if (AddAisleComboBox.Text == "11")
            {
                AddDeptComboBox.Text = "Kitchen";
            }
            if (AddAisleComboBox.Text == "12")
            {
                AddDeptComboBox.Text = "Electronics";
            }
            if (AddAisleComboBox.Text == "14")
            {
                AddDeptComboBox.Text = "Grocery";
            }
            if (AddAisleComboBox.Text == "15")
            {
                AddDeptComboBox.Text = "Grocery";
            }
            if (AddAisleComboBox.Text == "99")
            {
                AddDeptComboBox.Text = "Misc";
            }
            if (AddAisleComboBox.Text == "Pharm")
            {
                AddDeptComboBox.Text = "Pharmacy";
            }
        }
        
        //Add Manual Entries to LocalList
        //##########################################################
	//This function transfers the manual entry data from the fields in the addItempanel to the manualentries 
	//table and writes the table to the user specific xml. Price Total and Coupons are recalculated.
        private void Addmanualentries()
        {
            DataRow mrow = ManualEntries.NewRow();
            mrow[0] = AddDescTextBox.Text;
            mrow[1] = AddPriceUpDown.Value;
            mrow[2] = AddQtyUpDown.Value;
            mrow[3] = AddDeptComboBox.Text;
            mrow[4] = AddAisleComboBox.Text;
            mrow[5] = AddCoupontextBox.Text;
            mrow[8] = AddCoupValueUpDown.Value;
            mrow[6] = true;
            ManualEntries.Rows.Add(mrow);
            ManualEntries.WriteXml(path + krogerplusnumber + "_ManualEntries.xml", XmlWriteMode.DiffGram);
            //dataGridViewSideView.DataSource = ManualEntries;
            Price_CoupTotal();
        }

        //Add Manual Entries to ServerEntries
        //##########################################################
	//This function transfers the data from the serverentries and manualentries table to the Shoppinglist table to be viewed as a merged table in datagridview1
	//There are three arguments for this function, all booleans. 
	//First argument: determines if the serverentries will be transfered
	//Second argument: determines if the manualentries will be transfered
	//Third argument: determines if the latest entry (true) or all of the existing entries (false) will be transfered.
	//Price Total and Coupons are recalculated after data is transfered.
 
        private void AddmanualentriestoSL(bool server, bool manual, bool allor1)
        {
            if (server == true && allor1 == false)
            {
                for (int j = 0; j < ServerEntriesDS.Tables["ServerEntries"].Rows.Count; j++)
                {
                    DataRow row = ShoppingList.NewRow();
                    row["ItemActive"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][0];
                    row["ListItemUPC"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][1];
                    row["ProductDescription"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][2];
                    row["Price"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][3];
                    row["Quantity"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][4];
                    row["Category"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][5];
                    row["Department"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][6];
                    row["Location"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][7];
                    row["ListItemCouponUPC"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][8];
                    row["CouponValue"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][9];
                    row["ItemID"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][10];
                    row["ManualID"] = ServerEntriesDS.Tables["ServerEntries"].Rows[j][11];
                    ShoppingList.Rows.Add(row);
                }
                dataGridView1.DataSource = ShoppingList;
            }
            if (server == true && allor1 == true)
            {
                int count1 = ServerEntriesDS.Tables["ServerEntries"].Rows.Count-1;
                MessageBox.Show("row count: " + ServerEntriesDS.Tables["ServerEntries"].Rows.Count + " row count-1: " + count1);
                DataRow row = ShoppingList.NewRow();
                row["ItemActive"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][0];
                row["ListItemUPC"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][1];
                row["ProductDescription"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][2];
                row["Price"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][3];
                row["Quantity"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][4];
                row["Category"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][5];
                row["Department"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][6];
                row["Location"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][7];
                row["ListItemCouponUPC"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][8];
                row["CouponValue"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][9];
                row["ItemID"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][10];
                row["ManualID"] = ServerEntriesDS.Tables["ServerEntries"].Rows[ServerEntriesDS.Tables["ServerEntries"].Rows.Count][11];
                ShoppingList.Rows.Add(row);
            }
            if (manual == true && allor1 == false)
            {
                for (int i = 0; i < ManualEntries.Rows.Count; i++)
                {
                    DataRow row = ShoppingList.NewRow();
                    row["ItemActive"] = ManualEntries.Rows[i][6];
                    row["ListItemUPC"] = "Manual Entry";
                    row["ProductDescription"] = ManualEntries.Rows[i][0];
                    row["Price"] = ManualEntries.Rows[i][1];
                    row["Quantity"] = ManualEntries.Rows[i][2];
                    row["Department"] = ManualEntries.Rows[i][3];
                    row["Location"] = ManualEntries.Rows[i][4];
                    row["ListItemCouponUPC"] = ManualEntries.Rows[i][5];
                    row["CouponValue"] = ManualEntries.Rows[i][8];
                    row["ManualID"] = ManualEntries.Rows[i][7];
                    ShoppingList.Rows.Add(row);
                }
            }
            if (manual == true && allor1 == true)
            {
                DataRow row = ShoppingList.NewRow();
                row["ItemActive"] = ManualEntries.Rows[ManualEntries.Rows.Count-1][6];
                row["ListItemUPC"] = "Manual Entry";
                row["ProductDescription"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][0];
                row["Price"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][1];
                row["Quantity"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][2];
                row["Department"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][3];
                row["Location"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][4];
                row["ListItemCouponUPC"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][5];
                row["CouponValue"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][8];
                row["ManualID"] = ManualEntries.Rows[ManualEntries.Rows.Count - 1][7];
                ShoppingList.Rows.Add(row);
            }
            dataGridView1.DataSource = ShoppingList;

            Price_CoupTotal();
        }

        //Search and Delete or Update in ManualEntires Table
        //##########################################################
	//This function cycles through to either Delete or update rows in both ManualEntries and ShoppingList tables 
	//Options include: Delete, Update Quantity, Update Coupon UPC and Value, and Update the ActiveItem state
	//Note: this is used soley for Manualentries
        private void DSSearchandDestroy (int SL_ManualID, string Function)
        {
            for (int i = 0; i < ManualEntries.Rows.Count; i++)                            
            {
                int ME_ManualID = Convert.ToInt16(ManualEntries.Rows[i][7]);
                if (SL_ManualID == ME_ManualID)                                                                         
                {
                    if(Function == "Delete")
                    {
                        ManualEntries.Rows[i].Delete();
                        break;
                    }
                    if (Function == "UpdateQTY")
                    {
                        ManualEntries.Rows[i][2] = UpdateQtyUpDown.Value;
                        break;
                    }
                    if (Function == "UpdateCoup")
                    {
                        ManualEntries.Rows[i][5] = UpdateCoupUPCMaskedTextBox.Text;
                        ManualEntries.Rows[i][8] = UpdateCoupValueUpDown.Value;
                        break;
                    }
                    if (Function == "UpdateActive")
                    {
                        ManualEntries.Rows[i][6] = activestatus;
                        break;
                    }
                }
            }
            for (int j = 0; j < ShoppingList.Rows.Count; j++)
            {
                string ME_ManualID2 = Convert.ToString(ShoppingList.Rows[j][11]);
                string SL = Convert.ToString(SL_ManualID);
                if (SL == ME_ManualID2)
                {
                    if (Function == "Delete")
                    {
                        ShoppingList.Rows[j].Delete(); 
                        break;
                    }
                    if (Function == "UpdateQTY")
                    {
                        ShoppingList.Rows[j][4] = UpdateQtyUpDown.Value;
                        break;
                    }

                    if (Function == "UpdateCoup")
                    {
                        ShoppingList.Rows[j][8] = UpdateCoupUPCMaskedTextBox.Text;
                        ShoppingList.Rows[j][9] = UpdateCoupValueUpDown.Value;
                        break;
                    }
                    if (Function == "UpdateActive")
                    {
                        ShoppingList.Rows[j][0] = activestatus;
                        break;
                    }
                }
            }
            Price_CoupTotal();
        }

        //Delete an Item from Shopping List (ListItems)
        //##########################################################
	//This function determines if the selected item in the shoppinglist (datagridview1) is a server entry or manual entry
	//and searchs the respective tables to delete the item and refreshed the shopping list (shoppinglist table)
	//Price and Coupon totals are recalculated
        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Selected == false)		//if no item is selected, the user is notified
            {
                MessageBox.Show("Please select an item to Delete.");
            }
            else
            {
                
                if (ManualOrServer == "Manual")
                {
                    Int32 getmanualID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[11].Value);		//gets the ManualID if the item is a manual entry

                    DSSearchandDestroy(getmanualID, "Delete");							//Uses DSSearchandDestroy to search the table and delete the entry 
                }												//from both manualentries and shoppinglist tables

                if (ManualOrServer == "Server")
                {
		    string selectedID = Convert.ToString(dataGridView1.SelectedRows[0].Cells["ItemID"].Value);	//Gets the itemID if the selection is a server entry

                    KrogerCommand.CommandType = CommandType.StoredProcedure;					//Deletes the item from the ListItems table on the server
                    KrogerCommand.CommandText = "Delete_Item_From_ListItems";
                    KrogerCommand.Parameters.Clear();
                    KrogerCommand.Connection = KrogerConnection;
                    KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);
                    KrogerCommand.Parameters.AddWithValue("@ItemID", selectedID);

                    SQLExecution("DELETE", "ServerEntries");

                    ServerEntriesDS.Tables["ServerEntries"].Clear();						//Clears the serverentries table and downloads a fresh version
                    ShoppingList.Clear();									//of ListItems innerjoined with Products from the server
                    LoadServerEntries();
                    dataGridView1.Refresh();
                    AddmanualentriestoSL(true, true, false);							//writes all of the entries from both serverentries and manualentries into the ShoppingList table
                }
            }
            Price_CoupTotal();
        }
        

        //Update Item in List
        //##########################################################
	//The following event handlers involve updating elements of existing items in the users shopping list

        private void UpdateItembtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Selected == false)			//If an item isn't selected for update, the user is notified
            {
                MessageBox.Show("Please select an item to update.");
            }
            else
            {
                UpdateItemPanel.Visible = true;					//The update panel is displayed; Data fields are cleared of previous entries.
                UpdateItemPanel.Location = new Point(265, 188);			
                UpdateQtyUpDown.Value = 1;
                UpdateCoupUPCMaskedTextBox.Clear();
                UpdateCoupUPCMaskedTextBox.Enabled = true;
                UpdateCoupValueUpDown.Enabled = true;
                if (ManualOrServer == "Server")					//Update coupon option is disabled if the item is a server item
                {
                    UpdateCoupUPCMaskedTextBox.Enabled = false;
                    UpdateCoupValueUpDown.Enabled = false;
		    UpdateCoupEnterBtn.Enabled = false;
                }
            }
        }

        private void UpdateQtyEnterBtn_Click(object sender, EventArgs e)
        {
            if (ManualOrServer == "Server")					//If the item is from the server, the new quantity is synced with the server and 
            {									//the lasted items from ListItems innerjoned with Products are downloaded
                KrogerCommand.CommandType = CommandType.StoredProcedure;
                KrogerCommand.CommandText = "Update_Quantity_In_ListItems";
                KrogerCommand.Parameters.Clear();
                KrogerCommand.Connection = KrogerConnection;
                KrogerCommand.Parameters.AddWithValue("@ItemUPC", dataGridView1.SelectedRows[0].Cells["ListItemUPC"].Value);
                KrogerCommand.Parameters.AddWithValue("@UserID", krogerplusnumber);
                KrogerCommand.Parameters.AddWithValue("@Quantity", UpdateQtyUpDown.Value);
                //KrogerCommand.Parameters.AddWithValue("@Active", datagridview1);

                SQLExecution("UPDATE", "ListItems");

                ServerEntriesDS.Tables["ServerEntries"].Clear();
                ShoppingList.Clear();
                LoadServerEntries();
                AddmanualentriestoSL(true, true, false);			//all of them manual and server entries are writen to ShoppingList which was just cleared.
                dataGridView1.Refresh();
                
            }
            if (ManualOrServer == "Manual")					//If the item is a manual entry, DSSearchandDestroy updates the quantity of the specified item
            {
                Int32 getmanualID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[11].Value);
                DSSearchandDestroy(getmanualID, "UpdateQTY");
            }
            UpdateItemPanel.Visible = false;					//Update panel is hidden
            Price_CoupTotal();							//Price and Coupon totals are recalculated
        }

        private void UpdateCoupUPCEnterbtn_Click(object sender, EventArgs e)			//Adds a coupon UPC and Value to a manual entry
        {
            Int32 getmanualID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[11].Value);
            DSSearchandDestroy(getmanualID, "UpdateCoup");
            UpdateItemPanel.Visible = false;
        }

        private void UpdateCoupUPCMaskedTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            UpdateCoupUPCMaskedTextBox.Focus();
            UpdateCoupUPCMaskedTextBox.SelectionStart = 0;
        }

        private void UpdateCoupUPCMaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (UpdateCoupUPCMaskedTextBox.Text.Length < 10 || UpdateCoupUPCMaskedTextBox.Text.Length > 0)	//A user must input the full 10-digit UPC for a coupon to enabled the enter button
            {													//if a user deletes when they typed in, the enter button is re-enabled to allow 
                UpdateCoupUPCEnterbtn.Enabled = false;								//a blank entry to be made, effectively deleting the coupon
            }
            if (UpdateCoupUPCMaskedTextBox.Text.Length == 10 || UpdateCoupUPCMaskedTextBox.Text.Length == 0)
            {
                UpdateCoupUPCEnterbtn.Enabled = true;
            }
        }

        private void UpdateCancelBtn_Click(object sender, EventArgs e)
        {
            UpdateItemPanel.Visible = false;					//Update panel is hidden
        }

        //Execute SQL Commands
        //##########################################################
	//This function opens a connection to the server and executes the commands to use stored procedures
	//It requires the desired function and the table in which the data will be store if data is being downloaded
        private int SQLExecution(string commandtype, string tablename)
        {
            int returnrowcount = 0;     // number of rows returned by the command

            try
            {
                KrogerConnection.Open();						//Opens the connection to the server with the connection string which is declared globally
                Statuslbl.Text = "Connection Successful!";			
                listBoxStatus.Items.Add("Connection Successful!");
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                connectivityflag = true;						//Connectivityflag is set to true if the connection is successful
                OnlineStatuslbl.Text = "Online:";					
                OnlineShape.BackColor = Color.LawnGreen;				//Online status indicator is set to green. 
            }

            catch (Exception e)
            {
                listBoxStatus.Items.Add("Connection Open Error: " + e.ToString());
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                Statuslbl.Text = "Connection to Database Failed.  Check Configuration.";
                Cursor.Current = Cursors.Default;
                connectivityflag = false;
                OnlineStatuslbl.Text = "Offline:";
                OnlineShape.BackColor = Color.Red;
                try
                {
                    ServerEntriesDS.Tables["ServerEntries"].WriteXml(path + krogerplusnumber +"_OfflineServerEntries.xml");	//if the connection is lost, the server entries are written to an xml
                    listBoxStatus.Items.Add("ServerEntries tables written to xml");
                    listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
		}
                catch (DataException ex)
                {
                    Statuslbl.Text = "Error encountered.";
                    listBoxStatus.Items.Add(ex.Message);
                    listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                }
            }

            try
            {
                if (commandtype == "SELECT")											//The select command executes the stored procedure involved in downloading 
                {														//the user's shoppinglist data from the Server
                    KrogerDataAdapter.SelectCommand = KrogerCommand;    					
                    returnrowcount = KrogerDataAdapter.Fill(ServerEntriesDS, tablename);     	
                    listBoxStatus.Items.Add("Inserted " + returnrowcount + " " + tablename + " records.");
                    listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                }
                if (commandtype == "UPDATE")											//UPDATE 
                {
                    KrogerDataAdapter.SelectCommand = KrogerCommand;                      
                    returnrowcount = KrogerDataAdapter.Fill(ServerEntriesDS, tablename);     
                    returnrowcount = KrogerCommand.ExecuteNonQuery();
		    listBoxStatus.Items.Add("Inserted " + returnrowcount + " " + tablename + " records.");
                    listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;

                }
                if (commandtype == "INSERT")											//INSERT is used to add new items to the ListItems table on the server, given a KP number, the item UPC, and quantity.
                {
                    KrogerDataAdapter.SelectCommand = KrogerCommand;                      
                    returnrowcount = KrogerCommand.ExecuteNonQuery();
		    listBoxStatus.Items.Add("Inserted " + returnrowcount + " " + tablename + " records.");
                    listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                }
                if (commandtype == "DELETE")											//Delete is used to execute a stored procedure to delete an item from ListItems,  give a KP number and the item UPC.
                {
                    KrogerDataAdapter.SelectCommand = KrogerCommand;                      
                    returnrowcount = KrogerCommand.ExecuteNonQuery();
                    listBoxStatus.Items.Add("Inserted " + returnrowcount + " " + tablename + " records.");
		    listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                    dataGridView1.DataSource = ServerEntriesDS.Tables[tablename];
                }
                if (commandtype == "UPCLOOKUP")											//UPCLOOKUP executes a stored procedure that retrieves information of an item that the user wants to add
                {														//This information is populated in the fields in the additem panel so the user may view it before they actually add the item
                    KrogerDataAdapter.SelectCommand = KrogerCommand;                      
                    SqlDataReader reader = KrogerCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        AddDescTextBox.Text = reader["ProductDescription"].ToString();
                        AddCoupontextBox.Text = reader["CouponUPC"].ToString();
                        AddCoupValueUpDown.Value = Convert.ToDecimal(reader["CouponValue"]);
                        AddPriceUpDown.Value = Convert.ToDecimal(reader["Price"]);
                        AddDeptComboBox.Text = reader["Department"].ToString();
                        AddAisleComboBox.Text = reader["Location"].ToString();
                    }
                    reader.Close();
                }
                if (commandtype == "USERLOOKUP")										//USERLOOKUP retrieves the user's info from Users table on the server, saving it to the local UserData table 
                {														//and populating the labels in the User Profile on the right side fo the application.
                    KrogerDataAdapter.SelectCommand = KrogerCommand;                      
                    SqlDataReader reader = KrogerCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        //string username = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                        //CustNameLbl.Text = username;
                        //CusGasPtsLbl.Text = reader["CurrentGasPoints"].ToString();
                        try
                        {
                            UserData.Clear();
                            DataRow row = UserData.NewRow();
                            row["KrogerPlusNumber"] = krogerplusnumber;
                            row["FirstName"] = reader["FirstName"].ToString();
                            row["LastName"] = reader["LastName"].ToString();
                            row["GasPoints"] = reader["CurrentGasPoints"].ToString();
                            UserData.Rows.Add(row);
                        }
                        catch (Exception ex)
                        {
                            Statuslbl.Text = "Error encountered.";
                            listBoxStatus.Items.Add(ex.Message);
                            listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
                        }
                    }
                    reader.Close();
                }

                listBoxStatus.Items.Add(commandtype + " returned " + returnrowcount);
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            catch (DataException de)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add(de.Message);
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            catch (SystemException se)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add(se.Message);
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            catch (Exception e)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add(e.Message);
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }

            try
            {
                KrogerConnection.Close();
                //Statuslbl.Text = "Connection Closed.";
                listBoxStatus.Items.Add("Connection Closed to " + "ServerEntries");
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            catch (Exception e)
            {
                Statuslbl.Text = "Error encountered.";
                listBoxStatus.Items.Add("Connection Close Error: " + e.ToString());
                listBoxStatus.SelectedIndex = listBoxStatus.Items.Count - 1;
            }
            return returnrowcount;
        }

        //**********************************************************************************
        // Other Functions
        //**********************************************************************************

        //Price and Coupon Total Function
        //##########################################################
	//
        private void Price_CoupTotal()
        {

            decimal getpricetotal;
            decimal pricetotal = 0;
            string getcoupontotal;
            decimal coupontotal = 0;
            decimal getquantity;
            decimal ctotal;
            int i;
            for (i = 0; i < dataGridView1.Rows.Count; i++)					
            {	//Using the amount of rows there are in the datagridview, which correspondes with the ShoppingList table,
		//the function cycles through to add the active item coupon values to provide a total of savings
	 											
                if (Convert.ToBoolean(LocalList.Tables[1].Rows[i][0]) == true)		//if the item is active
                {
                    getcoupontotal = Convert.ToString(ShoppingList.Rows[i][9]);		//copy the coupon value into getcoupontotal
                    if (getcoupontotal == "")						//if it is null, continue to the next row
                    {
                        continue;
                    }
                    else
                    {
                        if (decimal.TryParse(getcoupontotal, out ctotal))		//convert getcoupontotal from a string to decimal to allow addition
                        {
                            coupontotal += ctotal;					//add the amount (ctotal) to the cumulative coupontotal 
                        }
                    }
                }
            }
            CouponTotalLbl.Text = coupontotal.ToString("$ 0.00");			//convert coupontotal back to string with formatting and load into coupontotalLbl

            for (i = 0; i < dataGridView1.Rows.Count; i++) 				
            {	//Using the amount of rows there are in the datagridview, which correspondes with the ShoppingList table,
		//the function cycles through to add the active item price values to provide a price total.

                if (Convert.ToBoolean(LocalList.Tables[1].Rows[i][0]) == true)		//if the item is active
                {
                    getpricetotal = Convert.ToDecimal(LocalList.Tables[1].Rows[i][3].ToString());	//copy the price value to getpricetotal
                    getquantity = Convert.ToDecimal(ShoppingList.Rows[i][4]);		//copy the quantity to getquantity
                    pricetotal += (getpricetotal * getquantity);			//Muliplies the price by quantity and adds to cumulative pricetotal.
                }
            }
            pricetotal = pricetotal - coupontotal;					//subtract coupon total from pricetotal
            PriceTotalLbl.Text = pricetotal.ToString("$ 0.00");				//Convert pricetotal back to string with formatting and load into PriceTotalLbl
            dataGridView1.Refresh();
        }

        //Display Map
        //##########################################################
        private void MapBtn_Click(object sender, EventArgs e)
        {
            MapForm mapform = new MapForm();						//instantiates Mapform
            mapform.DataSource = LocalList;						//copies LocalList dataset from form1 to Mapform
            mapform.Visible = true;							
        }

        //Function to check for numeric values (isnumeric equivalent)
        //##########################################################
        private bool IsTextValidated(string strTextEntry)
        {
            Regex objNotWholePattern = new Regex("[^0-9]");
            return !objNotWholePattern.IsMatch(strTextEntry)
                 && (strTextEntry != "");
        }

        //View Buttons
        //##########################################################
	//These buttons change the view of the application. The user can either see their shoppinglist
	//or in depth information about the coupons linked to their server items
        private void CouponViewBtn_Click(object sender, EventArgs e)
        {
            ServerEntriesDS.Tables["Coupons"].Clear();					
            appstartup();								//After ServerEntries is cleared, the data is downloaded again
            dataGridView1.DataSource = ServerEntriesDS.Tables["Coupons"];

            CouponTotalLbl.Visible = false;
            PriceTotalLbl.Visible = false;
            AddItemBtn.Enabled = false;
            UpdateItembtn.Enabled = false;
            DeleteBtn.Enabled = false;
        }

        private void ShoppingListViewBtn_Click(object sender, EventArgs e)
        {
            ShoppingList.Clear();
            appstartup();								//After Shoppinglist is cleared, the data is downloaded again
            dataGridView1.DataSource = ShoppingList;

            CouponTotalLbl.Visible = true;
            PriceTotalLbl.Visible = true;
            AddItemBtn.Enabled = true;
            UpdateItembtn.Enabled = true;
            DeleteBtn.Enabled = true;
        }
    }
}




