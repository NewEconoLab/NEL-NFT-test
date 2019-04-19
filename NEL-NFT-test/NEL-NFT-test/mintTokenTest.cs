using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.ComponentModel;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace NEL_NFT_test
{
    public class mintTokenTest : SmartContract
    {
        //初始管理員
        static readonly byte[] superAdmin = Helper.ToScriptHash("AeaWf2v7MHGpzxH4TtBAu5kJRp5mRq2DQG");     

        //铸造事件
        public delegate void deleMint(byte[] addrOwner, BigInteger amount);
        [DisplayName("mint")]
        public static event deleMint onMint;
        public delegate void deleNFTMint(byte[] addrOwner, BigInteger tokenID, Token token);
        [DisplayName("NFTmint")]
        public static event deleNFTMint onNFTMint;

        //log
        public delegate void deleLog(string name, object data);
        [DisplayName("log")]
        public static event deleLog onLog;

        public class Token
        {
            public Token()
            {
                token_id = 0;
                owner = new byte[0];
                approved = new byte[0];
                properties = "";
                uri = "";
                rwProperties = "";
            }

            //不能使用get set

            public BigInteger token_id;// { get; set; } //代币ID
            public byte[] owner;// { get; set; } //代币所有权地址
            public byte[] approved;// { get; set; } //代币授权处置权地址
            public string properties;// { get; set; } //代币只读属性
            public string uri;// { get; set; } //代币URI链接
            public string rwProperties;// { get; set; } //代币可修改属性
        }

        public static bool isOpen()
        {
            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            var data = sysStateMap.Get("isOpen");

            if (data.Length > 0)
            {
                var isOpenStr = data.AsString();
                if (isOpenStr == "1") return true;
                else return false;
            }
            return true;
        }

        public static Token getToken(BigInteger tokenID)
        {
            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");

            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length > 0)
            {
                Token token = Helper.Deserialize(data) as Token;

                return token;
            }
            return new Token();
        }

        public static Map<BigInteger, BigInteger> getAddrNFTlist(byte[] addr)
        {
            StorageMap addrNFTlistMap = Storage.CurrentContext.CreateMap("addrNFTlist");
            var data = addrNFTlistMap.Get(addr);

            if (data.Length > 0)
            {
                return Helper.Deserialize(data) as Map<BigInteger, BigInteger>;
            }
            else
            {
                return new Map<BigInteger, BigInteger>();
            }

        }

        public static Map<BigInteger, BigInteger> tokenIDsOfOwner(byte[] addr)
        {
            return getAddrNFTlist(addr);
        }

        public static void addrNFTlistAdd(byte[] addr, BigInteger tokenID)
        {
            StorageMap addrNFTlistMap = Storage.CurrentContext.CreateMap("addrNFTlist");//0,存储addr拥有NFT总数

            Map<BigInteger, BigInteger> addrNFTlist = getAddrNFTlist(addr);
            if (addrNFTlist.HasKey(0))
            {
                addrNFTlist[0] = addrNFTlist[0] + 1;
            }
            else
            {
                addrNFTlist[0] = 1;
            }
            addrNFTlist[tokenID] = 1;

            addrNFTlistMap.Put(addr, Helper.Serialize(addrNFTlist));
        }

        public static bool mintToken(byte[] owner, string properties, string URI, string rwProperties)
        {
            if (!isOpen() && !Runtime.CheckWitness(superAdmin)) return false;
            onLog("116", "go");
            if (!Runtime.CheckWitness(owner)) return false;
            onLog("118", "go");

            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");
            onLog("121", "go");

            BigInteger totalSupply = sysStateMap.Get("totalSupply").AsBigInteger();
            onLog("125", "go");
            Token newToken = new Token();
            onLog("127", "go");
            newToken.token_id = totalSupply + 1;
            onLog("129", "go");
            newToken.owner = owner;
            onLog("131", "go");
            newToken.approved = new byte[0];
            onLog("133", "go");
            newToken.properties = properties;
            onLog("135", "go");
            newToken.uri = URI;
            onLog("137", "go");
            newToken.rwProperties = rwProperties;
            onLog("139", "go");

            sysStateMap.Put("totalSupply", newToken.token_id);
            onLog("142", "go");
            tokenMap.Put(newToken.token_id.AsByteArray(), Helper.Serialize(newToken));
            onLog("144", "go");
            addrNFTlistAdd(owner, newToken.token_id);
            onLog("146", "go");

            onMint(owner, 1);
            onLog("149", "go");
            onNFTMint(owner, newToken.token_id, newToken);
            onLog("151", "go");

            return true;
        }

        public static object Main(string operation, object[] args)
        {
            //UTXO转账转入转出都不允许
            if (Runtime.Trigger == TriggerType.Verification || Runtime.Trigger == TriggerType.VerificationR)
            {
                return false;
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                if (operation == "isOpen")
                {
                    return isOpen();
                }

                if (operation == "token")
                {
                    if (args.Length != 1) return false;
                    return getToken((BigInteger)args[0]);
                }

                if (operation == "tokenIDsOfOwner")
                {
                    if (args.Length != 1) return false;
                    return tokenIDsOfOwner((byte[])args[0]);
                }              

                //代币合约所有者操作(为测试开放所有地址和superAdmin，当isOpen=false时仅superAdmin)
                if (operation == "mintToken")
                {
                    if (args.Length != 4) return false;
                    return mintToken((byte[])args[0], (string)args[1], (string)args[2], (string)args[3]);
                }
            }
            return false;
        }
    }
}
