using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace eveOnlineMarketSpy {
	public partial class MainWindow : Window {
		private IDictionary<int,MarketGroupItem> market_groups = new Dictionary<int,MarketGroupItem>();
		private IDictionary<int,IList<SUb_Type>> types = new Dictionary<int,IList<SUb_Type>>();
		private IDictionary<int,string> regions = new Dictionary<int,string>();

		public MainWindow() {
			InitializeComponent();
		}

        struct MarketGroupItem
        {
            public int? parent_id;
            public string name;
            public bool has_types;
        }

        struct SUb_Type
        {
            public int id;
            public string name;
        }
        
        
        /*onStart_window_loaded populates our 3 Dictionary key-> value maps market_groups
         types and regions*/
        private void onStart_Window_Loaded(object sender, RoutedEventArgs e) {
            //marketGroup_reader is a placeholder variable used as we iterate through master market groups
            IEnumerable<string> marketGroup_reader = System.IO.File.ReadLines("master_market_groups.csv");
			foreach (string line in marketGroup_reader) {
				MarketGroupItem marketGrpOBJ = new MarketGroupItem();
				string[] split = txt_File_Search_andGrab(line, 4, 2);
				int mg_id = int.Parse(split[0]);
				string parent = split[1];
				if (parent == "null")
					marketGrpOBJ.parent_id = null;
				else
					marketGrpOBJ.parent_id = int.Parse(parent);
				marketGrpOBJ.name = split[2];
				marketGrpOBJ.has_types = (split[3] == "1");
				market_groups.Add(mg_id, marketGrpOBJ);
			}
            
            /**/
            add_children(itemBrowser.Items, null);

			
            
            /*Organizes the sub types of each master market, creates a Type object 't', uses parse to find the id and name of the items
             and assigns them to t.id and t.name*/
            IEnumerable<string> type_reader = System.IO.File.ReadLines("sub_types.csv");
			foreach (string line in type_reader) {
				string[] split = txt_File_Search_andGrab(line, 3, 1);
				SUb_Type t = new SUb_Type();
				t.id = int.Parse(split[0]);
				t.name = split[1];
				//if field 2's match, they belong to a 'parent' item. i.e. minmatar bullet ammunition all belongs to the same family of items
                //caldari missiles all belong to the same family of items etc.
                int mg_id = int.Parse(split[2]);
				if (!types.ContainsKey(mg_id))
					types.Add(mg_id, new List<SUb_Type>());
				types[mg_id].Add(t);
			}

			
            /*Read in solar system regions.  Use parse to grab element 2 and 1, assign element 2 to region
             with its id  (element 1) assigned to id*/
            IEnumerable<string> region_reader = System.IO.File.ReadLines("regions.csv");
			foreach (string line in region_reader) {
				string[] split = txt_File_Search_andGrab(line, 2, 1);
				int id = int.Parse(split[0]);
				regions[id] = split[1];
			}
		}

		private string[] txt_File_Search_andGrab(string line, int fields, int string_index) {
			
            /*SPLIT! - returns a string array containing substrings delimited by elements of a specified unicode character-
             in this case ',' from the input files - */
            string[] split = line.Split(new char[]{','}, fields);
			string str = split[string_index];
        
            /*
            string str = split[string_index - str.Length - 1];
            for (int i = 0; i < str.Length; i++)
            {

            }
              */
                if (str[str.Length - 1] != '\"')
                {
                    string next = split[string_index + 1];
                    int end = next.LastIndexOf('\"');
                    str += "," + next.Substring(0, end);
                    next = next.Substring(end + 2);
                    split[string_index + 1] = next;
                }
			/*Eliminates the quotation marks around the item names
             BROKEN*/
            //split[string_index] = str.Substring(1, str.Length-1); 
			return split;
		
        /**/
        
        
        
        }
        
		
        /**/
        private void add_children(ItemCollection group, int? id) {
            /*KeyValuepair http://msdn.microsoft.com/en-us/library/5tbh8a42(v=vs.110).aspx
             * 
             * Defines a key/value pair that can be set or retrieved.
             TKey:
         The type of the key.
    
             TValue:
         The type of the value.*/
            
            foreach (KeyValuePair<int,MarketGroupItem> marketOBJ in market_groups) {
				/**/
                if (marketOBJ.Value.parent_id == id) {
					TreeViewItem item = new TreeViewItem();
					item.Header = marketOBJ.Value.name;
					item.Tag = marketOBJ.Key;
					if (!marketOBJ.Value.has_types)
						add_children(item.Items, marketOBJ.Key);
					group.Add(item);
				}
			}
		}

        /*Filters the results in the itemResults box to display only buy/sell orders
         from the region specified in the region box*/
        private void regionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (TabItem ti in itemResults.Items)
                ((OrdersManagement)ti.Content).filter(region.Text);
        }
		
        /**/
        private void itemBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			TreeViewItem value = (TreeViewItem)e.NewValue;
			int mg_id = (int)value.Tag;
            itemBrowserSubTypes.Items.Clear();
			if (market_groups[mg_id].has_types)
				foreach (SUb_Type t in types[mg_id]) {
					ListBoxItem item = new ListBoxItem();
					item.Content = t.name;
					item.Tag = t.id;
                    itemBrowserSubTypes.Items.Add(item);
				}
		}

        /*on double click of item in itemBrowserSubTypes, display the buy/sell/volume information in itemResults*/
		private void itemBrowserSubTypes_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ListBoxItem item = (ListBoxItem)itemBrowserSubTypes.SelectedItem;
			int type_id = (int)item.Tag;
            OrdersManagement oc = new OrdersManagement(type_id, regions, region.Text);
			TabItem tab = new TabItem();
			tab.Header = item.Content;
			tab.Content = oc;
            itemResults.Items.Add(tab);
            itemResults.SelectedItem = tab;
		}

		
	}

	
}