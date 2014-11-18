using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Xml.Linq;

namespace eveOnlineMarketSpy {
	public partial class OrdersManagement : UserControl {
		private int type_id;
		private IDictionary<int,string> regions;
		IEnumerable<Order> sell_orders;
		IEnumerable<Order> buy_orders;
		DataTable visible_sell_orders = new DataTable();
		DataTable visible_buy_orders = new DataTable();

        class Order
        {
            public string region;
            public string station;
            public string price;
            public int volume;

            public Order(XElement x, IDictionary<int, string> regions)
            {
                region = regions[int.Parse(x.Element("region").Value)];
                station = x.Element("station_name").Value.Trim();
                price = float.Parse(x.Element("price").Value).ToString("N2");
                volume = int.Parse(x.Element("vol_remain").Value);
            }
        }

        private IEnumerable<Order> parse_orders(XDocument xml, string type)
        {
            return (from x in xml.Descendants(type).Elements("order")
                    select new Order(x, regions)
            );
        }

        public void filter(string q)
        {
            this.visible_sell_orders.Clear();
            q = q.ToLower();
            foreach (Order order in this.sell_orders)
                if (q.Length == 0 || order.station.ToLower().Contains(q))
                    this.visible_sell_orders.Rows.Add(new object[] { order.region, order.station, order.price, order.volume });
            foreach (Order order in this.buy_orders)
                if (q.Length == 0 || order.station.ToLower().Contains(q))
                    this.visible_buy_orders.Rows.Add(new object[] { order.region, order.station, order.price, order.volume });
        }


        /*ordersManagement constructor. Takes 3 arguements from itemBrowserSubTypes_MouseDoubleClick, type_id, the item that was double clicked
         the dictionary regions, and the text that was in the regions box, default = jita*/
        public OrdersManagement(int type_id, IDictionary<int,string> regions, string filter) {
			InitializeComponent();
			this.type_id = type_id;
			this.regions = regions;

            create_table(this.visible_sell_orders, sellOrders);
            create_table(this.visible_buy_orders, buyOrders);

			WebClient downloader = new WebClient();
			string text = downloader.DownloadString("http://api.eve-central.com/api/quicklook?typeid=" + this.type_id);
			XDocument xml = XDocument.Parse(text);

			this.sell_orders = parse_orders(xml, "sell_orders");
			this.buy_orders = parse_orders(xml, "buy_orders");
			this.filter(filter);
		}

		private void create_table(DataTable table, DataGrid grid_outline) {
			
            /*element order important - region station price volume*/
            table.Columns.Add("Region", typeof(string));
			table.Columns.Add("Station", typeof(string));
			table.Columns.Add("Price", typeof(string));
			table.Columns.Add("Volume", typeof(int));
			grid_outline.DataContext = table;
			grid_outline.ItemsSource = table.DefaultView;
		}

		

		
	}

	
    /**/

}