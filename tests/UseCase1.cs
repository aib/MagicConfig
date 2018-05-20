using MagicConfig;
using MagicConfig.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace MagicConfig.Tests
{
	public class UseCase1: StaticMap<UseCase1>
	{
		public class Client: StaticMap<Client>, IKeyedItem {
			public string name;
			public SingleItem<string> ip;
			public SingleItem<int> port;

			public string GetKeyedItemKey() => name;
		}

		public KeyedItemList<Client> clients;
	}

	public class UseCase1Tests
	{
		[Fact]
		public void TestUseCase1()
		{
			var output = new List<string>();

			var uc1 = new UseCase1 {
				clients = new KeyedItemList<UseCase1.Client> {
					new UseCase1.Client { name = "localhost",  ip = "127.0.0.1",   port = 80 },
					new UseCase1.Client { name = "lanclient1", ip = "192.168.0.1", port = 8080 }
				}
			};
			var uc1Change = new UseCase1 {
				clients = new KeyedItemList<UseCase1.Client> {
					new UseCase1.Client { name = "localhost",  ip = "127.0.0.1",   port = 443 },
					new UseCase1.Client { name = "lanclient3", ip = "192.168.0.3", port = 8080 }
				}
			};

			uc1.clients.Updated     += (_, args) => output.Add($"Clients updated");
			uc1.clients.ItemAdded   += (_, args) => output.Add($"Client {args.Key} added");
			uc1.clients.ItemDeleted += (_, args) => output.Add($"Client {args.Key} deleted");
			uc1.clients.ItemUpdated += (_, args) => output.Add($"Client {args.Key} updated");

			uc1.clients["localhost"].Updated += (_, args) => output.Add($"localhost updated");
			uc1.clients["localhost"].ip.Updated   += (_, args) => output.Add($"localhost now at IP {args.NewValue}");
			uc1.clients["localhost"].port.Updated += (_, args) => output.Add($"localhost now at port {args.NewValue}");

			uc1.Assign(uc1Change);

			Assert.Equal(6, output.Count);
			Assert.Contains("Clients updated", output);
			Assert.Contains("Client localhost updated", output);
			Assert.Contains("Client lanclient1 deleted", output);
			Assert.Contains("Client lanclient3 added", output);
			Assert.Contains("localhost updated", output);
			Assert.Contains("localhost now at port 443", output);
		}
	}
}
