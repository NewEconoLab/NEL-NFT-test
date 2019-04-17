using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace NEL_NFT_test
{
    class mapTest : SmartContract
    {
        public static Map<BigInteger, BigInteger> getMap() {
            StorageMap addrNFTlistMap = Storage.CurrentContext.CreateMap("addrNFTlist");
            var data = addrNFTlistMap.Get("1".AsByteArray());

            if (data.Length > 0)
            {
                return Helper.Deserialize(data) as Map<BigInteger, BigInteger>;
            }
            else
            {
                return new Map<BigInteger, BigInteger>();
            }

        }

        public static object Main(string operation, object[] args)
        {
            StorageMap addrNFTlistMap = Storage.CurrentContext.CreateMap("addrNFTlist");

            if (operation == "getMap") //["(str)getMap",[]]
            {
                return getMap();

                //Map<BigInteger, BigInteger> addrNFTlist = new Map<BigInteger, BigInteger>();           
                //addrNFTlist[0] = 3;//
                //addrNFTlist[1] = 1;//
                //addrNFTlist[2] = 1;//
                //addrNFTlist[3] = 1;//
                //return addrNFTlist;
            }
            if (operation == "addMap") //["(str)addMap",["(int)1"]]
            {
                Map<BigInteger, BigInteger> addrNFTlist = getMap();
                addrNFTlist[(BigInteger)args[0]] = 1;
                if (addrNFTlist.HasKey(0))
                {
                    addrNFTlist[0] = addrNFTlist[0] + 1;
                }
                else
                {
                    addrNFTlist[0] = 1;
                }
                
                addrNFTlistMap.Put("1".AsByteArray(), Helper.Serialize(addrNFTlist));

                return true;
            }
            if (operation == "removeMap") //["(str)removeMap",["(int)2"]]
            {
                Map<BigInteger, BigInteger> addrNFTlist = getMap();
                
                if (addrNFTlist.HasKey((BigInteger)args[0])) {
                    addrNFTlist.Remove((BigInteger)args[0]);
                    addrNFTlist[0] = addrNFTlist[0] - 1;
                    addrNFTlistMap.Put("1".AsByteArray(), Helper.Serialize(addrNFTlist));
                    return true;
                }                            
            }

            return false;
        }
    }
}
