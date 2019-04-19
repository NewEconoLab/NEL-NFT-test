using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.ComponentModel;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace NEL_NFT_test
{
    public class NEL_NFT_test : SmartContract
    {
        //代币操作方法

        //无入参只读类
        //- name() : 返回代币合约名称
        //- decimals() : 返回代币合约精度
        //- supportedStandards() : 返回代币合约支持的协议，如{"NEP-10"}
        //- symbol() : 返回代币合约的单位
        //- totalSupply() : 返回代币的总数
        //- isOpen(): 返回合约是否开放任意地址

        //单token_id只读类
        //- allowance(token_id) : 返回代币的授权信息
        //- ownerOf(token_id) : 返回代币的所有者
        //- properties(token_id) : 返回代币的只读属性
        //- uri(token_id): 返回代币的URI信息
        //- rwProperties(token_id) : 返回代币的可重写属性
        //- token(token_id) : 返回代币的所有信息的字典

        //所有权类
        //- balanceOf(owner) : 返回地址拥有NFT的个数
        //- tokenIDsOfOwner(owner) :获取指定地址所有NFT tokenID
        //- transfer(to, token_id, extra_arg): 转移一个NFT（为测试superAdmin可任意执行此方法）
        //- approve(token_receiver, token_id, revoke) : 授权第三方操作NFT所有权（为测试superAdmin可任意执行此方法）
        //- transferFrom(spender, from, to, token_id): 在授权后执行NFT所有权转移（为测试superAdmin可任意执行此方法）


        //代币合约所有者操作(为测试开放所有地址和superAdmin，当isOpen=false时仅superAdmin)
        // TOKEN_CONTRACT_OWNER operations:
        //    - mintToken(owner, properties, URI, extra_arg): 铸造新的NFT
        //    - modifyURI(token_id, token_data) : 修改URI信息
        //    - setRWProperties(token_id,token_data):修改可变属性
        //    - setProperties(token_id,token_data):设置不可变属性，仅为管理需要，仅superAdmin

        //设置操作（仅superAdmin）
        //setters:
        //- setName(name) : 设置代币合约的名字
        //- setSymbol(symbol) : 设置代币合约的单位
        //- setSupportedStandards(supported_standards) : 设置和合约自持的标准,一般是一个数组，总是首先包含“NEP-10”
        //- setIsOpen(bool) : 仅测试使用，为了在合约被滥用的情况下，可以关闭任意地址铸币、修改URL、rwProperties

        //初始管理員
        static readonly byte[] superAdmin = Helper.ToScriptHash("AeaWf2v7MHGpzxH4TtBAu5kJRp5mRq2DQG");
        
        //不允许使用类变量
        //static string nameV = "NEL NFT Test";
        //static string symbolV = "NNT";
        //static string supportedStandardsV = "[\"NEP-10\"]";
        //static bool isOpenV = true;//是否开放铸币

        //事件
        //OnApprove = RegisterAction('approve', 'addr_from', 'addr_to', 'amount')
        //OnNFTApprove = RegisterAction('NFTapprove', 'addr_from', 'addr_to', 'tokenid')
        //OnTransfer = RegisterAction('transfer', 'addr_from', 'addr_to', 'amount')
        //OnNFTTransfer = RegisterAction('NFTtransfer', 'addr_from', 'addr_to', 'tokenid')
        //OnMint = RegisterAction('mint', 'addr_to', 'amount')
        //OnNFTMint = RegisterAction('NFTmint', 'addr_to', 'tokenid')

        //铸造事件
        public delegate void deleMint(byte[] addrOwner, BigInteger amount);
        [DisplayName("mint")]
        public static event deleMint onMint;
        public delegate void deleNFTMint(byte[] addrOwner, BigInteger tokenID,Token token);
        [DisplayName("NFTmint")]
        public static event deleNFTMint onNFTMint;

        //NFT修改事件
        public delegate void deleNFTModify(BigInteger tokenID,string elementName,string elementData);
        [DisplayName("NFTModify")]
        public static event deleNFTModify onNFTModify;

        //授权事件
        public delegate void deleApprove(byte[] addrOwner, byte[] addrApproved, BigInteger amount);
        [DisplayName("approve")]
        public static event deleApprove onApprove;
        public delegate void deleNFTApprove(byte[] addrOwner, byte[] addrApproved, BigInteger tokenID);
        [DisplayName("NFTapprove")]
        public static event deleNFTApprove onNFTApprove;

        //转账事件
        public delegate void deleTransfer(byte[] addrFrom, byte[] addrTo, BigInteger amount);
        [DisplayName("transfer")]
        public static event deleTransfer onTransfer;
        public delegate void deleNFTTransfer(byte[] addrFrom, byte[] addrTo, BigInteger tokenID);
        [DisplayName("NFTtransfer")]
        public static event deleNFTTransfer onNFTTransfer;

        //设置变更事件
        public delegate void deleNameModify(string newName);
        [DisplayName("nameModify")]
        public static event deleNameModify onNameModify;
        public delegate void deleSymbolModify(string newSymbol);
        [DisplayName("symbolModify")]
        public static event deleSymbolModify onSymbolModify;
        public delegate void deleSupportedStandardsModify(string[] newSupportedStandards);
        [DisplayName("supportedStandardsModify")]
        public static event deleSupportedStandardsModify onSupportedStandardsModify;
        public delegate void deleIsOpenChange(bool isOpen);
        [DisplayName("isOpenChange")]
        public static event deleIsOpenChange onIsOpenChange;


        public class Token
        {
            public Token() {
                token_id = 0;
                owner = new byte[0];
                approved = new byte[0];
                properties = "";
                uri = "";
                rwProperties = "";
            }
            //不能使用get set

            public BigInteger token_id;// { get; set; } //代币ID
            public byte[] owner;//  { get; set; } //代币所有权地址
            public byte[] approved;//  { get; set; } //代币授权处置权地址
            public string properties;//  { get; set; } //代币只读属性
            public string uri;//  { get; set; } //代币URI链接
            public string rwProperties;//  { get; set; } //代币可修改属性
        }

        public static string name() {
            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            var data = sysStateMap.Get("name");
            if (data.Length > 0) return data.AsString();
            return "NEL NFT Test";
        }

        public static string symbol()
        {
            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            var data = sysStateMap.Get("symbol");
            if (data.Length > 0) return data.AsString();
            return "NNT";
        }

        public static BigInteger decimals() {
            return 0;
        }

        public static string[] supportedStandards() {
            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            var data = sysStateMap.Get("supportedStandards");
            if (data.Length > 0) return Helper.Deserialize(data) as string[];
            return new string[] { "NEP-10" };
        }

        public static bool isOpen() {
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

        public static BigInteger totalSupply() {
            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            return sysStateMap.Get("totalSupply").AsBigInteger();
        }

        public static Token getToken(BigInteger tokenID) {
            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");

            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length > 0)
            {
                Token token = Helper.Deserialize(data) as Token;

                return token;
            }
            return new Token();
        }

        public static byte[] allowance(BigInteger tokenID)
        {
            Token token = getToken(tokenID);
            if (token.approved.Length > 0)
            {
                return token.owner.Concat(token.approved);
            }
            else
            {
                return new byte[0];
            }      
        }
        public static byte[] ownerOf(BigInteger tokenID)
        {
            return getToken(tokenID).owner;
        }
        public static string properties(BigInteger tokenID)
        {
            return getToken(tokenID).properties;
        }
        public static string uri(BigInteger tokenID)
        {
            return getToken(tokenID).uri;
        }
        public static string rwProperties(BigInteger tokenID)
        {
            return getToken(tokenID).rwProperties;
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

        public static void addrNFTlistAdd(byte[] addr, BigInteger tokenID) {
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

        public static void addrNFTlistRemove(byte[] addr, BigInteger tokenID)
        {
            StorageMap addrNFTlistMap = Storage.CurrentContext.CreateMap("addrNFTlist");//0,存储addr拥有NFT总数

            Map<BigInteger, BigInteger> addrNFTlist = getAddrNFTlist(addr);

            if (addrNFTlist.HasKey(tokenID))
            {
                addrNFTlist[0] = addrNFTlist[0] - 1;
                addrNFTlist.Remove(tokenID);               
                addrNFTlistMap.Put(addr, Helper.Serialize(addrNFTlist));
            }
        }

        public static BigInteger balanceOf(byte[] addr) {
            Map<BigInteger, BigInteger> addrNFTlist = getAddrNFTlist(addr);
            if (addrNFTlist.HasKey(0)) {
                return addrNFTlist[0];
            }
            return 0;
        }

        public static Map<BigInteger, BigInteger> tokenIDsOfOwner(byte[] addr) {
            return getAddrNFTlist(addr);
        }

        public static bool transfer(byte[] addrTo, BigInteger tokenID) {
            if (addrTo.Length != 20) return false;

            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");
            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length >0) {
                Token token = Helper.Deserialize(data) as Token;
                if (!Runtime.CheckWitness(token.owner) && !Runtime.CheckWitness(superAdmin)) return false;
                var addrFrom = token.owner;
                token.owner = addrTo;

                tokenMap.Put(tokenID.AsByteArray(), Helper.Serialize(token));
                addrNFTlistRemove(addrFrom, tokenID);
                addrNFTlistAdd(addrTo, tokenID);

                onTransfer(addrFrom, addrTo, 1);
                onNFTTransfer(addrFrom, addrTo, tokenID);

                return true;
            }

            return false;
        }

        public static bool approve(byte[] addr, BigInteger tokenID, bool revoke) {
            if (addr.Length != 20) return false;

            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");
            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length > 0)
            {
                Token token = Helper.Deserialize(data) as Token;
                if (!Runtime.CheckWitness(token.owner) && !Runtime.CheckWitness(superAdmin)) return false;

                if (!revoke)
                {
                    token.approved = addr;
                }
                else {
                    token.approved = new byte[0];
                }

                tokenMap.Put(tokenID.AsByteArray(), Helper.Serialize(token));

                onApprove(token.owner, token.approved, 1);
                onNFTApprove(token.owner, token.approved, tokenID);

                return true;
            }

            return false;
        }

        public static bool transferFrom(byte[] addrTo, BigInteger tokenID)
        {
            if (addrTo.Length != 20) return false;

            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");
            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length > 0)
            {
                Token token = Helper.Deserialize(data) as Token;
                if (!Runtime.CheckWitness(token.approved) && !Runtime.CheckWitness(superAdmin)) return false;
                var addrFrom = token.owner;
                token.owner = addrTo;

                tokenMap.Put(tokenID.AsByteArray(), Helper.Serialize(token));
                addrNFTlistRemove(addrFrom, tokenID);
                addrNFTlistAdd(addrTo, tokenID);

                onTransfer(addrFrom, addrTo, 1);
                onNFTTransfer(addrFrom, addrTo, tokenID);

                return true;
            }

            return false;
        }

        public static bool mintToken(byte[] owner,string properties,string URI,string rwProperties) {           
            if (!isOpen() && !Runtime.CheckWitness(superAdmin)) return false;
            if (!Runtime.CheckWitness(owner)) return false;

            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");

            BigInteger totalSupply = sysStateMap.Get("totalSupply").AsBigInteger();
            Token newToken = new Token();
            newToken.token_id = totalSupply + 1;
            newToken.owner = owner;
            newToken.approved = new byte[0];
            newToken.properties = properties;
            newToken.uri = URI;
            newToken.rwProperties = rwProperties;

            sysStateMap.Put("totalSupply", newToken.token_id);
            tokenMap.Put(newToken.token_id.AsByteArray(), Helper.Serialize(newToken));
            addrNFTlistAdd(owner, newToken.token_id);

            onMint(owner, 1);
            onNFTMint(owner, newToken.token_id, newToken);

            return true;
        }

        public static bool setProperties(BigInteger tokenID, string properties)
        {
            if (!Runtime.CheckWitness(superAdmin)) return false;

            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");

            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length > 0)
            {
                Token token = Helper.Deserialize(data) as Token;
                token.properties = properties;
                tokenMap.Put(token.token_id.AsByteArray(), Helper.Serialize(token));

                onNFTModify(tokenID, "properties", properties);

                return true;
            }
            return false;
        }

        public static bool modifyURI(BigInteger tokenID, string URI) {
            if (!isOpen() && !Runtime.CheckWitness(superAdmin)) return false;

            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");

            var data = tokenMap.Get(tokenID.AsByteArray());
            if(data.Length > 0) {
                Token token = Helper.Deserialize(data) as Token;
                if (!Runtime.CheckWitness(token.owner) && !Runtime.CheckWitness(superAdmin)) return false;
                token.uri = URI;
                tokenMap.Put(token.token_id.AsByteArray(), Helper.Serialize(token));

                onNFTModify(tokenID, "uri", URI);

                return true;
            }
            return false;
        }

        public static bool setRWProperties(BigInteger tokenID, string rwProperties)
        {
            if (!isOpen() && !Runtime.CheckWitness(superAdmin)) return false;

            StorageMap tokenMap = Storage.CurrentContext.CreateMap("token");

            var data = tokenMap.Get(tokenID.AsByteArray());
            if (data.Length > 0)
            {
                Token token = Helper.Deserialize(data) as Token;
                if (!Runtime.CheckWitness(token.owner) && !Runtime.CheckWitness(superAdmin)) return false;
                token.rwProperties = rwProperties;
                tokenMap.Put(token.token_id.AsByteArray(), Helper.Serialize(token));

                onNFTModify(tokenID, "rwProperties", rwProperties);

                return true;
            }
            return false;
        }

        public static bool setName(string newName) {
            if (!Runtime.CheckWitness(superAdmin)) return false;

            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            sysStateMap.Put("name", newName);

            onNameModify(newName);
            return true;
        }

        public static bool setSymbol(string newSymbol)
        {
            if (!Runtime.CheckWitness(superAdmin)) return false;

            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            sysStateMap.Put("symbol", newSymbol);

            onSymbolModify(newSymbol);
            return true;
        }

        public static bool setSupportedStandards(string[] newSupportedStandards)
        {
            if (!Runtime.CheckWitness(superAdmin)) return false;

            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            sysStateMap.Put("supportedStandards",Helper.Serialize(newSupportedStandards));

            onSupportedStandardsModify(newSupportedStandards);
            return true;
        }

        public static bool setIsOpen(bool newIsOpen)
        {
            if (!Runtime.CheckWitness(superAdmin)) return false;

            StorageMap sysStateMap = Storage.CurrentContext.CreateMap("sysState");
            var isOpenStr = "0";
            if (newIsOpen) isOpenStr = "1";
            sysStateMap.Put("isOpen", isOpenStr);

            onIsOpenChange(newIsOpen);
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
                //无入参只读类
                if (operation == "name")
                {
                    return name();
                }
                if (operation == "symbol")
                {
                    return symbol();
                }
                if (operation == "decimals")
                {
                    return decimals();
                }
                if (operation == "supportedStandards")
                {
                    return supportedStandards();
                }
                if (operation == "totalSupply")
                {
                    return totalSupply();
                }
                if (operation == "isOpen")
                {
                    return isOpen();
                }

                //单token_id只读类
                if (operation == "allowance")
                {
                    if (args.Length != 1) return false;
                    return allowance((BigInteger)args[0]);
                }
                if (operation == "ownerOf")
                {
                    if (args.Length != 1) return false;
                    return ownerOf((BigInteger)args[0]);
                }
                if (operation == "properties")
                {
                    if (args.Length != 1) return false;
                    return properties((BigInteger)args[0]);
                }
                if (operation == "uri")
                {
                    if (args.Length != 1) return false;
                    return uri((BigInteger)args[0]);
                }
                if (operation == "rwProperties")
                {
                    if (args.Length != 1) return false;
                    return rwProperties((BigInteger)args[0]);
                }
                if (operation == "token")
                {
                    if (args.Length != 1) return false;
                    return getToken((BigInteger)args[0]);
                }

                //所有权类
                if (operation == "balanceOf")
                {
                    if (args.Length != 1) return false;
                    return balanceOf((byte[])args[0]);
                }
                if (operation == "tokenIDsOfOwner")
                {
                    if (args.Length != 1) return false;
                    return tokenIDsOfOwner((byte[])args[0]);
                }
                if (operation == "transfer")
                {
                    if (args.Length != 2) return false;
                    return transfer((byte[])args[0],(BigInteger)args[1]);
                }
                if (operation == "approve")
                {
                    if (args.Length != 2 && args.Length!=3) return false;
                    if (args.Length == 2) return approve((byte[])args[0], (BigInteger)args[1], false);
                    return approve((byte[])args[0],(BigInteger)args[1],(bool)args[2]);
                }
                if (operation == "transferFrom")
                {
                    if (args.Length != 2) return false;
                    return transferFrom((byte[])args[0], (BigInteger)args[1]);
                }

                //代币合约所有者操作(为测试开放所有地址和superAdmin，当isOpen=false时仅superAdmin)
                if (operation == "mintToken")
                {
                    if (args.Length != 4) return false;
                    return mintToken((byte[])args[0], (string)args[1], (string)args[2], (string)args[3]);
                }
                if (operation == "modifyURI")
                {
                    if (args.Length != 2) return false;
                    return modifyURI((BigInteger)args[0], (string)args[1]);
                }
                if (operation == "setRWProperties")
                {
                    if (args.Length != 2) return false;
                    return setRWProperties((BigInteger)args[0], (string)args[1]);
                }
                if (operation == "setProperties")
                {
                    if (args.Length != 2) return false;
                    return setProperties((BigInteger)args[0], (string)args[1]);
                }

                //设置操作（仅superAdmin）
                if (operation == "setName")
                {
                    if (args.Length != 1) return false;
                    return setName((string)args[0]);
                }
                if (operation == "setSymbol")
                {
                    if (args.Length != 1) return false;
                    return setSymbol((string)args[0]);
                }
                if (operation == "setSupportedStandards")
                {
                    if (args.Length != 1) return false;
                    return setSupportedStandards((string[])args[0]);
                }
                if (operation == "setIsOpen")
                {
                    if (args.Length != 1) return false;
                    return setIsOpen((bool)args[0]);
                }
            }
            return false;
        }
    }
}
