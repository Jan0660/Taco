using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Taco
{
    public static class EtherScanApi
    {
        private record CacheEntry(EtherScanRates Rates, DateTime CachedTime);

        private static Dictionary<string, CacheEntry> _cache = new();
        private static HttpClient _httpClient = new();
        private static EtherScanRates _ethRatesCache;
        private static TimeSpan _ethRatesCacheKeepTime = TimeSpan.FromMinutes(20);

        public static async Task<EtherScanRates> GetRates(string currency = "ETH")
        {
            var res = JsonConvert.DeserializeObject<EtherScanRatesResponse>(
                    await _httpClient.GetStringAsync("https://api.coinbase.com/v2/exchange-rates?currency=" + currency))
                !.Data.Rates;
            _cache[currency] = new(res, DateTime.Now);
            _ethRatesCache = res;
            return res;
        }


        public static async Task<EtherScanRates> GetRatesCached(string currency = "ETH")
        {
            if (_cache.TryGetValue(currency, out var entry))
            {
                return entry.CachedTime + _ethRatesCacheKeepTime < DateTime.Now ? await GetRates() : _ethRatesCache;
            }

            return await GetRates(currency);
        }

        private class EtherScanRatesResponse
        {
            [JsonProperty("data")] public EtherScanRatesResponseData Data;
        }

        private class EtherScanRatesResponseData
        {
            [JsonProperty("currency")] public string Currency;
            [JsonProperty("rates")] public EtherScanRates Rates;
        }
    }

    public class BalanceResponse
    {
        [JsonProperty("result")] public string Result;
    }

    // ReSharper disable InconsistentNaming
    public class EtherScanRates
    {
        public decimal AED;
        public decimal AFN;
        public decimal ALL;
        public decimal AMD;
        public decimal ANG;
        public decimal AOA;
        public decimal ARS;
        public decimal AUD;
        public decimal AWG;
        public decimal AZN;
        public decimal BAM;
        public decimal BBD;
        public decimal BDT;
        public decimal BGN;
        public decimal BHD;
        public decimal BIF;
        public decimal BMD;
        public decimal BND;
        public decimal BOB;
        public decimal BRL;
        public decimal BSD;
        public decimal BTN;
        public decimal BWP;
        public decimal BYN;
        public decimal BYR;
        public decimal BZD;
        public decimal CAD;
        public decimal CDF;
        public decimal CHF;
        public decimal CLF;
        public decimal CLP;
        public decimal CNY;
        public decimal COP;
        public decimal CRC;
        public decimal CUC;
        public decimal CVE;
        public decimal CZK;
        public decimal DJF;
        public decimal DKK;
        public decimal DOP;
        public decimal DZD;
        public decimal EGP;
        public decimal ERN;
        public decimal ETB;
        public decimal EUR;
        public decimal FJD;
        public decimal FKP;
        public decimal GBP;
        public decimal GEL;
        public decimal GHS;
        public decimal GIP;
        public decimal GMD;
        public decimal GNF;
        public decimal GTQ;
        public decimal GYD;
        public decimal HKD;
        public decimal HNL;
        public decimal HRK;
        public decimal HTG;
        public decimal HUF;
        public decimal IDR;
        public decimal ILS;
        public decimal INR;
        public decimal IQD;
        public decimal ISK;
        public decimal JMD;
        public decimal JOD;
        public decimal JPY;
        public decimal KES;
        public decimal KGS;
        public decimal KHR;
        public decimal KMF;
        public decimal KRW;
        public decimal KWD;
        public decimal KYD;
        public decimal KZT;
        public decimal LAK;
        public decimal LBP;
        public decimal LKR;
        public decimal LRD;
        public decimal LSL;
        public decimal LYD;
        public decimal MAD;
        public decimal MDL;
        public decimal MGA;
        public decimal MKD;
        public decimal MMK;
        public decimal MNT;
        public decimal MOP;
        public decimal MRO;
        public decimal MUR;
        public decimal MVR;
        public decimal MWK;
        public decimal MXN;
        public decimal MYR;
        public decimal MZN;
        public decimal NAD;
        public decimal NGN;
        public decimal NIO;
        public decimal NOK;
        public decimal NPR;
        public decimal NZD;
        public decimal OMR;
        public decimal PAB;
        public decimal PEN;
        public decimal PGK;
        public decimal PHP;
        public decimal PKR;
        public decimal PLN;
        public decimal PYG;
        public decimal QAR;
        public decimal RON;
        public decimal RSD;
        public decimal RUB;
        public decimal RWF;
        public decimal SAR;
        public decimal SBD;
        public decimal SCR;
        public decimal SEK;
        public decimal SHP;
        public decimal SLL;
        public decimal SOS;
        public decimal SRD;
        public decimal SSP;
        public decimal STD;
        public decimal SVC;
        public decimal SZL;
        public decimal THB;
        public decimal TJS;
        public decimal TMT;
        public decimal TND;
        public decimal TOP;
        public decimal TRY;
        public decimal TTD;
        public decimal TWD;
        public decimal TZS;
        public decimal UAH;
        public decimal UGX;
        public decimal UYU;
        public decimal UZS;
        public decimal VES;
        public decimal VND;
        public decimal VUV;
        public decimal WST;
        public decimal XAF;
        public decimal XAG;
        public decimal XAU;
        public decimal XCD;
        public decimal XDR;
        public decimal XOF;
        public decimal XPD;
        public decimal XPF;
        public decimal XPT;
        public decimal YER;
        public decimal ZAR;
        public decimal ZMW;
        public decimal JEP;
        public decimal GGP;
        public decimal IMP;
        public decimal GBX;
        public decimal CNH;
        public decimal MTL;
        public decimal ZWL;
        public decimal SGD;
        public decimal USD;
        public decimal BTC;
        public decimal BCH;
        public decimal BSV;
        public decimal ETH;
        public decimal ETH2;
        public decimal ETC;
        public decimal LTC;
        public decimal ZRX;
        public decimal USDC;
        public decimal BAT;
        public decimal MANA;
        public decimal KNC;
        public decimal LINK;
        public decimal DNT;
        public decimal MKR;
        public decimal CVC;
        public decimal OMG;
        public decimal DAI;
        public decimal ZEC;
        public decimal REP;
        public decimal XLM;
        public decimal EOS;
        public decimal XTZ;
        public decimal ALGO;
        public decimal DASH;
        public decimal ATOM;
        public decimal OXT;
        public decimal COMP;
        public decimal ENJ;
        public decimal BAND;
        public decimal NMR;
        public decimal CGLD;
        public decimal UMA;
        public decimal LRC;
        public decimal YFI;
        public decimal UNI;
        public decimal BAL;
        public decimal REN;
        public decimal WBTC;
        public decimal NU;
        public decimal FIL;
        public decimal AAVE;
        public decimal BNT;
        public decimal GRT;
        public decimal SNX;
        public decimal STORJ;
        public decimal SUSHI;
        public decimal MATIC;
        public decimal SKL;
        public decimal ADA;
        public decimal ANKR;
        public decimal CRV;
        public decimal NKN;

        public decimal OGN;
//public string 1INCH;
    }
    // ReSharper restore InconsistentNaming
}