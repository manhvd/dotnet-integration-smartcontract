using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nethereum.Model;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.RPC.TransactionManagers;
using Nethereum.RPC.Eth.DTOs;
using System.Text;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace dotnet_interaction_ethnode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaultController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _networkUrl;
        private readonly string _vaultAddress;
        public VaultController(IConfiguration configuration)
        {
            _configuration = configuration;
            _vaultAddress = _configuration["VaultAddress"];
            _networkUrl = _configuration["NetworkUrl"];
        }
        // GET: api/<GetBlockNumber>
        [HttpGet]
        [Route("GetBlockNumber")]
        public async Task<string> GetBlockNumber()
        {
            var web3 = new Web3(_networkUrl);
            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            Console.WriteLine($"Latest Block Number is: {latestBlockNumber}");
            return JsonConvert.SerializeObject(latestBlockNumber);
        }
        // GET: api/<GetOwner>
        [HttpGet]
        [Route("GetOwner")]
        public async Task<string> GetOwner()
        {
            var _vaultABI = LoadJsonContract();
            var web3 = new Web3(_networkUrl);
            var contract = web3.Eth.GetContract(_vaultABI, _vaultAddress);
            var result = await contract.GetFunction("owner").CallAsync<string>();
            return result;
        }
        
        // POST api/<WithDraw>
        [HttpPost]
        [Route("WithDraw")]
        public async Task<TransactionReceipt> WithDraw(string toAddress, decimal amount)
        {
            //pravite account had role withdraw
            var privateKey = "eb958dxxxxxxxxxxxx75dd66be";
            var account = new Nethereum.Web3.Accounts.Account(privateKey);
            var web3 = new Web3(account,_networkUrl );
            web3.TransactionManager.UseLegacyAsDefault = true;
            var _vaultABI = LoadJsonContract();
            var contract = web3.Eth.GetContract(_vaultABI, _vaultAddress);
            var value = Nethereum.Web3.Web3.Convert.ToWei(amount);
            var withdrawFunction = contract.GetFunction("withdraw");
            var result = await withdrawFunction.SendTransactionAndWaitForReceiptAsync(account.Address, new HexBigInteger(100000), null, null, value,toAddress);
            return result;
        }
        [HttpPost]
        [Route("deposit")]
        public async Task<TransactionReceipt> Deposit(decimal amount)
        {
            //pravite account enough token
            var privateKey = "3861078fxxxxxxxxxxxx512b";
            var account = new Nethereum.Web3.Accounts.Account(privateKey);
            var web3 = new Web3(account, _networkUrl);
            web3.TransactionManager.UseLegacyAsDefault = true;
            var _vaultABI = LoadJsonContract();
            var contract = web3.Eth.GetContract(_vaultABI, _vaultAddress);
            var value = Nethereum.Web3.Web3.Convert.ToWei(amount);
            var deposit = contract.GetFunction("deposit");
            var gas = await deposit.EstimateGasAsync(account.Address, null, null, value);
            var result = await deposit.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, value);
            return result;
        }
        private string LoadJsonContract()
        {
            using (StreamReader r = new StreamReader("./Contracts/MVault.json"))
            {
                string json = r.ReadToEnd();
                ABI item = JsonConvert.DeserializeObject<ABI>(json);
                return JsonConvert.SerializeObject(item.abi);
            }
        }
    }
    public class ABI
    {
        public List<dynamic> abi { get; set; }
    }
}
