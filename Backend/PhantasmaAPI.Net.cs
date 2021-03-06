using System;
using System.Globalization;
using System.Net;
using LunarLabs.Parser;
using LunarLabs.Parser.JSON;
using Phantasma.Numerics;
using Phantasma.Cryptography;
using Phantasma.Domain;
using Phantasma.Core.Types;

namespace Phantasma.SDK
{
    public enum EPHANTASMA_SDK_ERROR_TYPE
    {
        API_ERROR,
        WEB_REQUEST_ERROR,
        FAILED_PARSING_JSON,
        MALFORMED_RESPONSE
    }

	internal static class APIUtils
    {
		internal static bool GetBoolean(this DataNode node, string name)
        {
            return node.GetBool(name);
        }
    }

    internal class JSONRPC_Client
    {
        internal void SendRequest(string url, string method, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback, 
                                            Action<DataNode> callback, params object[] parameters)
        {
            var paramData = DataNode.CreateArray("params");
            
            if (parameters!=null && parameters.Length > 0)
            {
                foreach (var obj in parameters)
                {
                    paramData.AddField(null, obj);
                }
            }

            var jsonRpcData = DataNode.CreateObject(null);
            jsonRpcData.AddField("jsonrpc", "2.0");
            jsonRpcData.AddField("method", method);
            jsonRpcData.AddField("id", "1");
            jsonRpcData.AddNode(paramData);
            
            string json;

            try
            {
				json = JSONWriter.WriteToString(jsonRpcData);				
            }
            catch (Exception e)
            {
                throw e;
            }

			string contents;			
			try {
				using (var wc = new WebClient())
				{
					wc.Headers[HttpRequestHeader.ContentType] = "application/json";
					contents = wc.UploadString(url, json);
				}
			}
			catch (Exception e)
			{
				if (errorHandlingCallback != null) errorHandlingCallback(EPHANTASMA_SDK_ERROR_TYPE.WEB_REQUEST_ERROR, e.Message);
                return;
			}
			
			var root = JSONReader.ReadFromString(contents);
			
			if (root == null)
			{
				if (errorHandlingCallback != null) errorHandlingCallback(EPHANTASMA_SDK_ERROR_TYPE.FAILED_PARSING_JSON, "failed to parse JSON");
			}
			else 
			if (root.HasNode("error")) {
				var errorDesc = root["error"].GetString("message");
				if (errorHandlingCallback != null) errorHandlingCallback(EPHANTASMA_SDK_ERROR_TYPE.API_ERROR, errorDesc);
			}
			else
			if (root.HasNode("result"))
			{
				var result = root["result"];
				callback(result);
			}
			else {					
				if (errorHandlingCallback != null) errorHandlingCallback(EPHANTASMA_SDK_ERROR_TYPE.MALFORMED_RESPONSE, "malformed response");
			}							
        }		
   }


	public struct LeaderboardRow
	{
		public string address; //
		public string value; //

		public static LeaderboardRow FromNode(DataNode node)
		{
			LeaderboardRow result;

			result.address = node.GetString("address");
			result.value = node.GetString("value");

			return result;
		}
	}



	public struct Leaderboard
	{
		public string name; //
		public LeaderboardRow[] rows; //

		public static Leaderboard FromNode(DataNode node)
		{
			Leaderboard result;

			result.name = node.GetString("name");
			var rows_array = node.GetNode("rows");
			if (rows_array != null)
			{
				result.rows = new LeaderboardRow[rows_array.ChildCount];
				for (int i = 0; i < rows_array.ChildCount; i++)
				{

					result.rows[i] = LeaderboardRow.FromNode(rows_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.rows = new LeaderboardRow[0];
			}


			return result;
		}
	}



	public struct Balance
	{
		public string chain; //
		public string amount; //
		public string symbol; //
		public uint decimals; //
		public string[] ids; //

		public static Balance FromNode(DataNode node)
		{
			Balance result;

			result.chain = node.GetString("chain");
			result.amount = node.GetString("amount");
			result.symbol = node.GetString("symbol");
			result.decimals = node.GetUInt32("decimals");
			var ids_array = node.GetNode("ids");
			if (ids_array != null)
			{
				result.ids = new string[ids_array.ChildCount];
				for (int i = 0; i < ids_array.ChildCount; i++)
				{

					result.ids[i] = ids_array.GetNodeByIndex(i).AsString();
				}
			}
			else
			{
				result.ids = new string[0];
			}


			return result;
		}
	}

	public struct Interop
	{
		public string local; //
		public string external; //

		public static Interop FromNode(DataNode node)
		{
			Interop result;

			result.local = node.GetString("local");
			result.external = node.GetString("external");

			return result;
		}
	}

	public struct Platform
	{
		public string platform; //
		public string chain; //
		public string fuel; //
		public string[] tokens; //
		public Interop[] interop; //

		public static Platform FromNode(DataNode node)
		{
			Platform result;

			result.platform = node.GetString("platform");
			result.chain = node.GetString("chain");
			result.fuel = node.GetString("fuel");
			var tokens_array = node.GetNode("tokens");
			if (tokens_array != null)
			{
				result.tokens = new string[tokens_array.ChildCount];
				for (int i = 0; i < tokens_array.ChildCount; i++)
				{

					result.tokens[i] = tokens_array.GetNodeByIndex(i).AsString();
				}
			}
			else
			{
				result.tokens = new string[0];
			}

			var interop_array = node.GetNode("interop");
			if (interop_array != null)
			{
				result.interop = new Interop[interop_array.ChildCount];
				for (int i = 0; i < interop_array.ChildCount; i++)
				{

					result.interop[i] = Interop.FromNode(interop_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.interop = new Interop[0];
			}


			return result;
		}
	}

	public struct Swap
	{
		public string sourcePlatform; //
		public string sourceChain; //
		public string sourceHash; //
		public string sourceAddress; //
		public string destinationPlatform; //
		public string destinationChain; //
		public string destinationHash; //
		public string destinationAddress; //
		public string symbol; //
		public string value; //

		public static Swap FromNode(DataNode node)
		{
			Swap result;

			result.sourcePlatform = node.GetString("sourcePlatform");
			result.sourceChain = node.GetString("sourceChain");
			result.sourceHash = node.GetString("sourceHash");
			result.sourceAddress = node.GetString("sourceAddress");
			result.destinationPlatform = node.GetString("destinationPlatform");
			result.destinationChain = node.GetString("destinationChain");
			result.destinationHash = node.GetString("destinationHash");
			result.destinationAddress = node.GetString("destinationAddress");
			result.symbol = node.GetString("symbol");
			result.value = node.GetString("value");

			return result;
		}
	}

	public struct Governance
	{
		public string name; //
		public string value; //

		public static Governance FromNode(DataNode node)
		{
			Governance result;

			result.name = node.GetString("name");
			result.value = node.GetString("value");

			return result;
		}
	}



	public struct Organization
	{
		public string id; //
		public string name; //
		public string[] members; //

		public static Organization FromNode(DataNode node)
		{
			Organization result;

			result.id = node.GetString("id");
			result.name = node.GetString("name");
			var members_array = node.GetNode("members");
			if (members_array != null)
			{
				result.members = new string[members_array.ChildCount];
				for (int i = 0; i < members_array.ChildCount; i++)
				{

					result.members[i] = members_array.GetNodeByIndex(i).AsString();
				}
			}
			else
			{
				result.members = new string[0];
			}


			return result;
		}
	}



	public struct Nexus
	{
		public string name; //
		public Platform[] platforms; //
		public Token[] tokens; //
		public Chain[] chains; //
		public Governance[] governance; //
		public string[] organizations; //

		public static Nexus FromNode(DataNode node)
		{
			Nexus result;

			result.name = node.GetString("name");
			var platforms_array = node.GetNode("platforms");
			if (platforms_array != null)
			{
				result.platforms = new Platform[platforms_array.ChildCount];
				for (int i = 0; i < platforms_array.ChildCount; i++)
				{

					result.platforms[i] = Platform.FromNode(platforms_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.platforms = new Platform[0];
			}

			var tokens_array = node.GetNode("tokens");
			if (tokens_array != null)
			{
				result.tokens = new Token[tokens_array.ChildCount];
				for (int i = 0; i < tokens_array.ChildCount; i++)
				{

					result.tokens[i] = Token.FromNode(tokens_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.tokens = new Token[0];
			}

			var chains_array = node.GetNode("chains");
			if (chains_array != null)
			{
				result.chains = new Chain[chains_array.ChildCount];
				for (int i = 0; i < chains_array.ChildCount; i++)
				{

					result.chains[i] = Chain.FromNode(chains_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.chains = new Chain[0];
			}

			var governance_array = node.GetNode("governance");
			if (governance_array != null)
			{
				result.governance = new Governance[governance_array.ChildCount];
				for (int i = 0; i < governance_array.ChildCount; i++)
				{

					result.governance[i] = Governance.FromNode(governance_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.governance = new Governance[0];
			}

			var organizations_array = node.GetNode("organizations");
			if (organizations_array != null)
			{
				result.organizations = new string[organizations_array.ChildCount];
				for (int i = 0; i < organizations_array.ChildCount; i++)
				{

					result.organizations[i] = organizations_array.GetNodeByIndex(i).AsString();
				}
			}
			else
			{
				result.organizations = new string[0];
			}


			return result;
		}
	}



	public struct Stake
	{
		public string amount; //
		public uint time; //
		public string unclaimed; //

		public static Stake FromNode(DataNode node)
		{
			Stake result;

			result.amount = node.GetString("amount");
			result.time = node.GetUInt32("time");
			result.unclaimed = node.GetString("unclaimed");

			return result;
		}
	}

	public struct Account
	{
		public string address; //
		public string name; //
		public Stake stake; //
		public string relay; //
		public string validator; //
		public Balance[] balances; //

		public static Account FromNode(DataNode node)
		{
			Account result;

			result.address = node.GetString("address");
			result.name = node.GetString("name");
			result.stake = Stake.FromNode(node.GetNode("stakes"));
			result.relay = node.GetString("relay");
			result.validator = node.GetString("validator");
			var balances_array = node.GetNode("balances");
			if (balances_array != null)
			{
				result.balances = new Balance[balances_array.ChildCount];
				for (int i = 0; i < balances_array.ChildCount; i++)
				{

					result.balances[i] = Balance.FromNode(balances_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.balances = new Balance[0];
			}

			return result;
		}
	}

	public struct ContractParameter
	{
		public string name;
		public string type;
	}

	public struct ContractMethod
	{
		public string name;
		public string returnType;
		public ContractParameter[] parameters;
	}

	public struct Contract
	{
		public string address; //
		public string name; //
		public string script; //
		public ContractMethod[] methods;

		public static Contract FromNode(DataNode node)
		{
			Contract result;

			result.address = node.GetString("address");
			result.name = node.GetString("name");
			result.script = node.GetString("script");

			var methodNode = node.GetNode("methods");
			if (methodNode != null)
			{
				result.methods = new ContractMethod[methodNode.ChildCount];
				for (int i = 0; i < result.methods.Length; i++)
				{
					var child = methodNode.GetNodeByIndex(i);
					var method = new ContractMethod();
					method.name = child.GetString("name");
					method.returnType = child.GetString("returnType");

					var paramsNode = child.GetNode("parameters");
					if (paramsNode != null)
					{
						method.parameters = new ContractParameter[paramsNode.ChildCount];
						for (int j = 0; j < method.parameters.Length; j++)
						{
							var temp = paramsNode.GetNodeByIndex(j);
							var p = new ContractParameter();

							p.name = temp.GetString("name");
							p.type = temp.GetString("type");

							method.parameters[j] = p;
						}
					}
					else
					{
						method.parameters = new ContractParameter[0];
					}

					result.methods[i] = method;
				}
			}
			else
			{
				result.methods = new ContractMethod[0];
			}

			return result;
		}
	}

	public struct Chain
	{
		public string name; //
		public string address; //
		public string parentAddress; //
		public uint height; //
		public string[] contracts; //

		public static Chain FromNode(DataNode node)
		{
			Chain result;

			result.name = node.GetString("name");
			result.address = node.GetString("address");
			result.parentAddress = node.GetString("parentAddress");
			result.height = node.GetUInt32("height");
			var contracts_array = node.GetNode("contracts");
			if (contracts_array != null)
			{
				result.contracts = new string[contracts_array.ChildCount];
				for (int i = 0; i < contracts_array.ChildCount; i++)
				{

					result.contracts[i] = contracts_array.GetNodeByIndex(i).AsString();
				}
			}
			else
			{
				result.contracts = new string[0];
			}


			return result;
		}
	}

	public struct Event
	{
		public string address; //
		public EventKind kind; //
		public string data; //

		public static Event FromNode(DataNode node)
		{
			Event result;

			result.address = node.GetString("address");
			result.kind = node.GetEnum<EventKind>("kind");
			result.data = node.GetString("data");

			return result;
		}

		public override string ToString()
		{
			return $"{kind} @ {address}";
		}
	}

	public struct Transaction
	{
		public string hash; //
		public string chainAddress; //
		public uint timestamp; //
		public int confirmations; //
		public int blockHeight; //
		public string blockHash; //
		public string script; //
		public Event[] events; //
		public string result; //
		public string fee; //

		public static Transaction FromNode(DataNode node)
		{
			Transaction result;

			result.hash = node.GetString("hash");
			result.chainAddress = node.GetString("chainAddress");
			result.timestamp = node.GetUInt32("timestamp");
			result.confirmations = node.GetInt32("confirmations");
			result.blockHeight = node.GetInt32("blockHeight");
			result.blockHash = node.GetString("blockHash");
			result.script = node.GetString("script");
			var events_array = node.GetNode("events");
			if (events_array != null)
			{
				result.events = new Event[events_array.ChildCount];
				for (int i = 0; i < events_array.ChildCount; i++)
				{

					result.events[i] = Event.FromNode(events_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.events = new Event[0];
			}

			result.result = node.GetString("result");
			result.fee = node.GetString("fee");

			return result;
		}
	}

	public struct AccountTransactions
	{
		public string address; //
		public Transaction[] txs; //

		public static AccountTransactions FromNode(DataNode node)
		{
			AccountTransactions result;

			result.address = node.GetString("address");
			var txs_array = node.GetNode("txs");
			if (txs_array != null)
			{
				result.txs = new Transaction[txs_array.ChildCount];
				for (int i = 0; i < txs_array.ChildCount; i++)
				{

					result.txs[i] = Transaction.FromNode(txs_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.txs = new Transaction[0];
			}


			return result;
		}
	}

	public struct Block
	{
		public string hash; //
		public string previousHash; //
		public uint timestamp; //
		public uint height; //
		public string chainAddress; //
		public uint protocol; //
		public Transaction[] txs; //
		public string validatorAddress; //
		public string reward; //

		public static Block FromNode(DataNode node)
		{
			Block result;

			result.hash = node.GetString("hash");
			result.previousHash = node.GetString("previousHash");
			result.timestamp = node.GetUInt32("timestamp");
			result.height = node.GetUInt32("height");
			result.chainAddress = node.GetString("chainAddress");
			result.protocol = node.GetUInt32("protocol");
			var txs_array = node.GetNode("txs");
			if (txs_array != null)
			{
				result.txs = new Transaction[txs_array.ChildCount];
				for (int i = 0; i < txs_array.ChildCount; i++)
				{

					result.txs[i] = Transaction.FromNode(txs_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.txs = new Transaction[0];
			}

			result.validatorAddress = node.GetString("validatorAddress");
			result.reward = node.GetString("reward");

			return result;
		}
	}

	public class Token
	{
		public string symbol; //
		public string apiSymbol; // API symbols may differ.
		public string name; //
		public int decimals; //
		public string currentSupply; //
		public string maxSupply; //
		public string platform; //
		public string hash; //
		public string flags; //
		public decimal price;

		public static Token FromNode(DataNode node)
		{
			Token result = new Token();

			result.symbol = node.GetString("symbol");
			result.name = node.GetString("name");
			result.decimals = node.GetInt32("decimals");
			result.currentSupply = node.GetString("currentSupply");
			result.maxSupply = node.GetString("maxSupply");
			result.platform = node.GetString("platform");
			result.hash = node.GetString("hash");
			result.flags = node.GetString("flags");
			result.price = 0;

			return result;
		}
	}

	public struct TokenData
	{
		public string ID; //
		public string chainName; //
		public string ownerAddress; //
		public string ram; //
		public string rom; //
		public Boolean forSale; //

		public static TokenData FromNode(DataNode node)
		{
			TokenData result;

			result.ID = node.GetString("iD");
			result.chainName = node.GetString("chainName");
			result.ownerAddress = node.GetString("ownerAddress");
			result.ram = node.GetString("ram");
			result.rom = node.GetString("rom");
			result.forSale = node.GetBoolean("forSale");

			return result;
		}
	}

	public struct SendRawTx
	{
		public string hash; //
		public string error; //

		public static SendRawTx FromNode(DataNode node)
		{
			SendRawTx result;

			result.hash = node.GetString("hash");
			result.error = node.GetString("error");

			return result;
		}
	}

	public struct Auction
	{
		public string creatorAddress; //
		public string chainAddress; //
		public uint startDate; //
		public uint endDate; //
		public string baseSymbol; //
		public string quoteSymbol; //
		public string tokenId; //
		public string price; //
		public string rom; //
		public string ram; //

		public static Auction FromNode(DataNode node)
		{
			Auction result;

			result.creatorAddress = node.GetString("creatorAddress");
			result.chainAddress = node.GetString("chainAddress");
			result.startDate = node.GetUInt32("startDate");
			result.endDate = node.GetUInt32("endDate");
			result.baseSymbol = node.GetString("baseSymbol");
			result.quoteSymbol = node.GetString("quoteSymbol");
			result.tokenId = node.GetString("tokenId");
			result.price = node.GetString("price");
			result.rom = node.GetString("rom");
			result.ram = node.GetString("ram");

			return result;
		}
	}

	public struct Oracle
	{
		public string url; //
		public string content; //

		public static Oracle FromNode(DataNode node)
		{
			Oracle result;

			result.url = node.GetString("url");
			result.content = node.GetString("content");

			return result;
		}
	}

	public struct Script
	{
		public Event[] events; //
		public string result; //
		public string[] results; //
		public Oracle[] oracles; //

		public static Script FromNode(DataNode node)
		{
			Script result;

			var events_array = node.GetNode("events");
			if (events_array != null)
			{
				result.events = new Event[events_array.ChildCount];
				for (int i = 0; i < events_array.ChildCount; i++)
				{

					result.events[i] = Event.FromNode(events_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.events = new Event[0];
			}

			result.result = node.GetString("result");

			var results_array = node.GetNode("results");
			if (results_array != null)
			{
				result.results = new string[results_array.ChildCount];
				for (int i = 0; i < results_array.ChildCount; i++)
				{

					result.results[i] = results_array.GetNodeByIndex(i).Value;

				}
			}
			else
			{
				result.results = new string[0];
			}

			var oracles_array = node.GetNode("oracles");
			if (oracles_array != null)
			{
				result.oracles = new Oracle[oracles_array.ChildCount];
				for (int i = 0; i < oracles_array.ChildCount; i++)
				{

					result.oracles[i] = Oracle.FromNode(oracles_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.oracles = new Oracle[0];
			}


			return result;
		}
	}

	public struct Archive
	{
		public string hash; //
		public uint size; //
		public string flags; //
		public string key; //
		public int blockCount; //
		public string[] metadata; //

		public static Archive FromNode(DataNode node)
		{
			Archive result;

			result.hash = node.GetString("hash");
			result.size = node.GetUInt32("size");
			result.flags = node.GetString("flags");
			result.key = node.GetString("key");
			result.blockCount = node.GetInt32("blockCount");
			var metadata_array = node.GetNode("metadata");
			if (metadata_array != null)
			{
				result.metadata = new string[metadata_array.ChildCount];
				for (int i = 0; i < metadata_array.ChildCount; i++)
				{

					result.metadata[i] = metadata_array.GetNodeByIndex(i).AsString();
				}
			}
			else
			{
				result.metadata = new string[0];
			}


			return result;
		}
	}

	public struct ABIParameter
	{
		public string name; //
		public string type; //

		public static ABIParameter FromNode(DataNode node)
		{
			ABIParameter result;

			result.name = node.GetString("name");
			result.type = node.GetString("type");

			return result;
		}
	}

	public struct ABIMethod
	{
		public string name; //
		public string returnType; //
		public ABIParameter[] parameters; //

		public static ABIMethod FromNode(DataNode node)
		{
			ABIMethod result;

			result.name = node.GetString("name");
			result.returnType = node.GetString("returnType");
			var parameters_array = node.GetNode("parameters");
			if (parameters_array != null)
			{
				result.parameters = new ABIParameter[parameters_array.ChildCount];
				for (int i = 0; i < parameters_array.ChildCount; i++)
				{

					result.parameters[i] = ABIParameter.FromNode(parameters_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.parameters = new ABIParameter[0];
			}


			return result;
		}
	}

	public struct ABIContract
	{
		public string name; //
		public ABIMethod[] methods; //

		public static ABIContract FromNode(DataNode node)
		{
			ABIContract result;

			result.name = node.GetString("name");
			var methods_array = node.GetNode("methods");
			if (methods_array != null)
			{
				result.methods = new ABIMethod[methods_array.ChildCount];
				for (int i = 0; i < methods_array.ChildCount; i++)
				{

					result.methods[i] = ABIMethod.FromNode(methods_array.GetNodeByIndex(i));

				}
			}
			else
			{
				result.methods = new ABIMethod[0];
			}


			return result;
		}
	}

	public struct Channel
	{
		public string creatorAddress; //
		public string targetAddress; //
		public string name; //
		public string chain; //
		public uint creationTime; //
		public string symbol; //
		public string fee; //
		public string balance; //
		public Boolean active; //
		public int index; //

		public static Channel FromNode(DataNode node)
		{
			Channel result;

			result.creatorAddress = node.GetString("creatorAddress");
			result.targetAddress = node.GetString("targetAddress");
			result.name = node.GetString("name");
			result.chain = node.GetString("chain");
			result.creationTime = node.GetUInt32("creationTime");
			result.symbol = node.GetString("symbol");
			result.fee = node.GetString("fee");
			result.balance = node.GetString("balance");
			result.active = node.GetBoolean("active");
			result.index = node.GetInt32("index");

			return result;
		}
	}

	public struct Receipt
	{
		public string nexus; //
		public string channel; //
		public string index; //
		public uint timestamp; //
		public string sender; //
		public string receiver; //
		public string script; //

		public static Receipt FromNode(DataNode node)
		{
			Receipt result;

			result.nexus = node.GetString("nexus");
			result.channel = node.GetString("channel");
			result.index = node.GetString("index");
			result.timestamp = node.GetUInt32("timestamp");
			result.sender = node.GetString("sender");
			result.receiver = node.GetString("receiver");
			result.script = node.GetString("script");

			return result;
		}
	}

	public struct Peer
	{
		public string url; //
		public string flags; //
		public string fee; //
		public uint pow; //

		public static Peer FromNode(DataNode node)
		{
			Peer result;

			result.url = node.GetString("url");
			result.flags = node.GetString("flags");
			result.fee = node.GetString("fee");
			result.pow = node.GetUInt32("pow");

			return result;
		}
	}

	public struct Validator
	{
		public string address; //
		public string type; //

		public static Validator FromNode(DataNode node)
		{
			Validator result;

			result.address = node.GetString("address");
			result.type = node.GetString("type");

			return result;
		}
	}

	public enum ExchangeOrderSide
	{
		Buy,
		Sell
	}

	public enum ExchangeOrderType
	{
		OTC,
		Limit,              //normal limit order
		ImmediateOrCancel,  //special limit order, any unfulfilled part of the order gets cancelled if not immediately fulfilled
		Market,             //normal market order
							//TODO: FillOrKill = 4,         //Either gets 100% fulfillment or it gets cancelled , no partial fulfillments like in IoC order types
	}

	public struct ExchangeOrder
	{
		public BigInteger Uid;
		public Timestamp Timestamp;
		public Address Creator;
		public Address Provider;

		public BigInteger Amount;
		public string BaseSymbol;

		public BigInteger Price;
		public string QuoteSymbol;

		public ExchangeOrderSide Side;
		public ExchangeOrderType Type;

		public static ExchangeOrder FromNode(DataNode node)
		{
			ExchangeOrder result;
			result.Uid = BigInteger.Parse(node.GetString("uid"));
			result.Timestamp = (Timestamp) node.GetDateTime("timestamp");
			result.Creator = Address.FromText(node.GetString("creator"));
			result.Provider = Address.FromText(node.GetString("provider"));
			result.Amount = BigInteger.Parse(node.GetString("ammount"));
			result.BaseSymbol = node.GetString("baseSymbol");
			result.Price = BigInteger.Parse(node.GetString("timestamp"));
			result.QuoteSymbol = node.GetString("quoteSymbol");
			result.Side = node.GetEnum<ExchangeOrderSide>("Side");
			result.Type = node.GetEnum<ExchangeOrderType>("Type");

			return result;
		}
	}
	public class API {	   
		public readonly	string Host;
		private static JSONRPC_Client _client;
	   
		public API(string host) 
		{
			this.Host = host;
			_client = new JSONRPC_Client();
		}
	   
		
		//Returns the account name and balance of given address.
		public void GetAccount(string account, Action<Account> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getAccount", errorHandlingCallback, (node) => { 
				var result = Account.FromNode(node);
				callback(result);
			} , account);		   
		}
		
		
		//Returns the address that owns a given name.
		public void LookUpName(string name, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "lookUpName", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , name);		   
		}
		
		
		//Returns the height of a chain.
		public void GetBlockHeight(string chainInput, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getBlockHeight", errorHandlingCallback, (node) => { 
				var result = int.Parse(node.Value);
				callback(result);
			} , chainInput);		   
		}
		
		
		//Returns the number of transactions of given block hash or error if given hash is invalid or is not found.
		public void GetBlockTransactionCountByHash(string blockHash, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getBlockTransactionCountByHash", errorHandlingCallback, (node) => { 
				var result = int.Parse(node.Value);
				callback(result);
			} , blockHash);		   
		}
		
		
		//Returns information about a block by hash.
		public void GetBlockByHash(string blockHash, Action<Block> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getBlockByHash", errorHandlingCallback, (node) => { 
				var result = Block.FromNode(node);
				callback(result);
			} , blockHash);		   
		}
		
		
		//Returns a serialized string, containing information about a block by hash.
		public void GetRawBlockByHash(string blockHash, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getRawBlockByHash", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , blockHash);		   
		}
		
		
		//Returns information about a block by height and chain.
		public void GetBlockByHeight(string chainInput, uint height, Action<Block> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getBlockByHeight", errorHandlingCallback, (node) => { 
				var result = Block.FromNode(node);
				callback(result);
			} , chainInput, height);		   
		}
		
		
		//Returns a serialized string, in hex format, containing information about a block by height and chain.
		public void GetRawBlockByHeight(string chainInput, uint height, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getRawBlockByHeight", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , chainInput, height);		   
		}
		
		
		//Returns the information about a transaction requested by a block hash and transaction index.
		public void GetTransactionByBlockHashAndIndex(string blockHash, int index, Action<Transaction> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getTransactionByBlockHashAndIndex", errorHandlingCallback, (node) => { 
				var result = Transaction.FromNode(node);
				callback(result);
			} , blockHash, index);		   
		}
		
		
		//Returns last X transactions of given address.
		//This api call is paginated, multiple calls might be required to obtain a complete result 
		public void GetAddressTransactions(string account, uint page, uint pageSize, Action<AccountTransactions, int, int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getAddressTransactions", errorHandlingCallback, (node) => { 
				var currentPage = node.GetInt32("page");
				var totalPages = node.GetInt32("totalPages");
				node = node.GetNode("result");
				var result = AccountTransactions.FromNode(node);
				callback(result, currentPage, totalPages);
			} , account, page, pageSize);		   
		}
		
		
		//Get number of transactions in a specific address and chain
		public void GetAddressTransactionCount(string account, string chainInput, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getAddressTransactionCount", errorHandlingCallback, (node) => { 
				var result = int.Parse(node.Value);
				callback(result);
			} , account, chainInput);		   
		}
		
		
		//Allows to broadcast a signed operation on the network, but it&apos;s required to build it manually.
		public void SendRawTransaction(string txData, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "sendRawTransaction", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , txData);		   
		}
		
		
		//Allows to invoke script based on network state, without state changes.
		public void InvokeRawScript(string chainInput, string scriptData, Action<Script> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "invokeRawScript", errorHandlingCallback, (node) => { 
				var result = Script.FromNode(node);
				callback(result);
			} , chainInput, scriptData);		   
		}
		
		
		//Returns information about a transaction by hash.
		public void GetTransaction(string hashText, Action<Transaction> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getTransaction", errorHandlingCallback, (node) => { 
				var result = Transaction.FromNode(node);
				callback(result);
			} , hashText);		   
		}
		
		
		//Removes a pending transaction from the mempool.
		public void CancelTransaction(string hashText, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "cancelTransaction", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , hashText);		   
		}
		
		
		//Returns an array of all chains deployed in Phantasma.
		public void GetChains(Action<Chain[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getChains", errorHandlingCallback, (node) => { 
				var result = new Chain[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Chain.FromNode(child);
				}
				callback(result);
			} );		   
		}
		
		
		//Returns info about the nexus.
		public void GetNexus(Boolean extended, Action<Nexus> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getNexus", errorHandlingCallback, (node) => { 
				var result = Nexus.FromNode(node);
				callback(result);
			} , extended);		   
		}
		
		
		//Returns info about an organization.
		public void GetOrganization(string ID, Action<Organization> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getOrganization", errorHandlingCallback, (node) => { 
				var result = Organization.FromNode(node);
				callback(result);
			} , ID);		   
		}
		
		
		//Returns content of a Phantasma leaderboard.
		public void GetLeaderboard(string name, Action<Leaderboard> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getLeaderboard", errorHandlingCallback, (node) => { 
				var result = Leaderboard.FromNode(node);
				callback(result);
			} , name);		   
		}
		
		
		//Returns an array of tokens deployed in Phantasma.
		public void GetTokens(Boolean extended, Action<Token[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getTokens", errorHandlingCallback, (node) => { 
				var result = new Token[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Token.FromNode(child);
				}
				callback(result);
			} , extended);		   
		}
		
		
		//Returns info about a specific token deployed in Phantasma.
		public void GetToken(string symbol, Boolean extended, Action<Token> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getToken", errorHandlingCallback, (node) => { 
				var result = Token.FromNode(node);
				callback(result);
			} , symbol, extended);		   
		}
		
		
		//Returns data of a non-fungible token, in hexadecimal format.
		public void GetTokenData(string symbol, string IDtext, Action<TokenData> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getTokenData", errorHandlingCallback, (node) => { 
				var result = TokenData.FromNode(node);
				callback(result);
			} , symbol, IDtext);		   
		}
		
		
		//Returns data of a non-fungible token, in hexadecimal format.
		public void GetNFT(string symbol, string IDtext, Boolean extended, Action<TokenData> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getNFT", errorHandlingCallback, (node) => { 
				var result = TokenData.FromNode(node);
				callback(result);
			} , symbol, IDtext, extended);		   
		}
		
		
		//Returns the balance for a specific token and chain, given an address.
		public void GetTokenBalance(string account, string tokenSymbol, string chainInput, Action<Balance> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getTokenBalance", errorHandlingCallback, (node) => { 
				var result = Balance.FromNode(node);
				callback(result);
			} , account, tokenSymbol, chainInput);		   
		}
		
		
		//Returns the number of active auctions.
		public void GetAuctionsCount(string chainAddressOrName, string symbol, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getAuctionsCount", errorHandlingCallback, (node) => { 
				var result = int.Parse(node.Value);
				callback(result);
			} , chainAddressOrName, symbol);		   
		}
		
		
		//Returns the auctions available in the market.
		//This api call is paginated, multiple calls might be required to obtain a complete result 
		public void GetAuctions(string chainAddressOrName, string symbol, uint page, uint pageSize, Action<Auction[], int, int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getAuctions", errorHandlingCallback, (node) => { 
				var currentPage = node.GetInt32("page");
				var totalPages = node.GetInt32("totalPages");
				node = node.GetNode("result");
				var result = new Auction[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Auction.FromNode(child);
				}
				callback(result, currentPage, totalPages);
			} , chainAddressOrName, symbol, page, pageSize);		   
		}
		
		
		//Returns the auction for a specific token.
		public void GetAuction(string chainAddressOrName, string symbol, string IDtext, Action<Auction> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getAuction", errorHandlingCallback, (node) => { 
				var result = Auction.FromNode(node);
				callback(result);
			} , chainAddressOrName, symbol, IDtext);		   
		}
		
		
		//Returns info about a specific archive.
		public void GetArchive(string hashText, Action<Archive> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getArchive", errorHandlingCallback, (node) => { 
				var result = Archive.FromNode(node);
				callback(result);
			} , hashText);		   
		}
		
		
		//Writes the contents of an incomplete archive.
		public void WriteArchive(string hashText, int blockIndex, string blockContent, Action<Boolean> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "writeArchive", errorHandlingCallback, (node) => { 
				var result = Boolean.Parse(node.Value);
				callback(result);
			} , hashText, blockIndex, blockContent);		   
		}
		
		
		//Reads given archive block.
		public void ReadArchive(string hashText, int blockIndex, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "readArchive", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , hashText, blockIndex);		   
		}
		
		
		//Returns the ABI interface of specific contract.
		public void GetContract(string chainAddressOrName, string contractName, Action<Contract> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getContract", errorHandlingCallback, (node) => { 
				var result = Contract.FromNode(node);
				callback(result);
			} , chainAddressOrName, contractName);		   
		}
		
		
		//Returns list of known peers.
		public void GetPeers(Action<Peer[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getPeers", errorHandlingCallback, (node) => { 
				var result = new Peer[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Peer.FromNode(child);
				}
				callback(result);
			} );		   
		}
		
		
		//Writes a message to the relay network.
		public void RelaySend(string receiptHex, Action<Boolean> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "relaySend", errorHandlingCallback, (node) => { 
				var result = Boolean.Parse(node.Value);
				callback(result);
			} , receiptHex);		   
		}
		
		
		//Receives messages from the relay network.
		public void RelayReceive(string account, Action<Receipt[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "relayReceive", errorHandlingCallback, (node) => { 
				var result = new Receipt[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Receipt.FromNode(child);
				}
				callback(result);
			} , account);		   
		}
		
		
		//Reads pending messages from the relay network.
		public void GetEvents(string account, Action<Event[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getEvents", errorHandlingCallback, (node) => { 
				var result = new Event[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Event.FromNode(child);
				}
				callback(result);
			} , account);		   
		}
		
		
		//Returns an array of available interop platforms.
		public void GetPlatforms(Action<Platform[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getPlatforms", errorHandlingCallback, (node) => { 
				var result = new Platform[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Platform.FromNode(child);
				}
				callback(result);
			} );		   
		}
		
		
		//Returns an array of available validators.
		public void GetValidators(Action<Validator[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getValidators", errorHandlingCallback, (node) => { 
				var result = new Validator[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Validator.FromNode(child);
				}
				callback(result);
			} );		   
		}
		
		
		//Tries to settle a pending swap for a specific hash.
		public void SettleSwap(string sourcePlatform, string destPlatform, string hashText, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "settleSwap", errorHandlingCallback, (node) => { 
				var result = node.Value;
				callback(result);
			} , sourcePlatform, destPlatform, hashText);		   
		}
		
		
		//Returns platform swaps for a specific address.
		public void GetSwapsForAddress(string account, Action<Swap[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)  
		{	   
			_client.SendRequest(Host, "getSwapsForAddress", errorHandlingCallback, (node) => { 
				var result = new Swap[node.ChildCount];
				for (int i=0; i<result.Length; i++) { 
					var child = node.GetNodeByIndex(i);
					result[i] = Swap.FromNode(child);
				}
				callback(result);
			} , account);		   
		}
		
		
		
		public Hash SignAndSendTransaction(PhantasmaKeys keys, byte[] script, string chain, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            var tx = new Blockchain.Transaction("simnet", chain, script, DateTime.UtcNow + TimeSpan.FromHours(1));
            tx.Sign(keys);

            SendRawTransaction(Base16.Encode(tx.ToByteArray(true)), callback, errorHandlingCallback);
            return tx.Hash;
        }


		//Returns ExchangeOrders.
		public void GetOTC(Action<ExchangeOrder[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
		{
			_client.SendRequest(Host, "getOTC", errorHandlingCallback, (node) => {
				var result = new ExchangeOrder[node.ChildCount];
				for (int i = 0; i < result.Length; i++)
				{
					var child = node.GetNodeByIndex(i);
					result[i] = ExchangeOrder.FromNode(child);
				}
				callback(result);
			});
		}

		// Create an OTC
		public void CreateOTCOrder(Address from, Address provider, string baseSymbol, string quoteSymbol, BigInteger amount, BigInteger price, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
		{
			_client.SendRequest(Host, "openOTCOrder", errorHandlingCallback, (node) => {
				var result = node.Value;
				callback(result);
			}, from, provider, baseSymbol, quoteSymbol, amount, price);
		}

		// TakeOrder
		public void TakeOrder(Address from, BigInteger uid, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
		{
			_client.SendRequest(Host, "takeOrder", errorHandlingCallback, (node) => {
				var result = node.Value;
				callback(result);
			}, from, uid);
		}

		public static bool IsValidPrivateKey(string address)
        {
            return (address.StartsWith("L", false, CultureInfo.InvariantCulture) ||
                    address.StartsWith("K", false, CultureInfo.InvariantCulture)) && address.Length == 52;
        }

        public static bool IsValidAddress(string address)
        {
            return address.StartsWith("P", false, CultureInfo.InvariantCulture) && address.Length == 45;
        }
	}
}