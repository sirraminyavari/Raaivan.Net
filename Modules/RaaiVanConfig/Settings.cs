using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaaiVan.Modules.RaaiVanConfig
{
    public static class Modules
    {
        private static bool _WorkFlow = true;
        private static bool _FormGenerator = true;
        private static bool _Notifications = true;
        private static bool _SMSEMailNotifier = true;
        private static bool _SocialNetwork = true;
        private static bool _KnowledgeAdmin = true;
        private static bool _Knowledge = true;
        private static bool _Documents = true;
        private static bool _PDFViewer = true;
        private static bool _QA = true;
        private static bool _QAAdmin = true;
        private static bool _Events = false;
        private static bool _Messaging = true;
        private static bool _Chat = true;
        private static bool _Resume = true;
        private static bool _VisualMap = true;
        private static bool _RealTime = true;
        private static bool _Explorer = true;
        private static bool _RestAPI = true;
        private static bool _Recommender = false;

        public static bool WorkFlow(Guid? applicationId = null) { return _WorkFlow; }
        public static bool FormGenerator(Guid? applicationId) { return _FormGenerator; }
        public static bool Notifications(Guid? applicationId) { return _Notifications; }
        public static bool SMSEMailNotifier(Guid? applicationId) { return _SMSEMailNotifier; }
        public static bool SocialNetwork(Guid? applicationId) { return _SocialNetwork; }
        public static bool KnowledgeAdmin(Guid? applicationId) { return _KnowledgeAdmin; }
        public static bool Knowledge(Guid? applicationId) { return _Knowledge; }
        public static bool Documents(Guid? applicationId) { return _Documents; }
        public static bool PDFViewer(Guid? applicationId) { return _PDFViewer; }
        public static bool QA(Guid? applicationId) { return _QA; }
        public static bool QAAdmin(Guid? applicationId) { return _QAAdmin; }
        public static bool Events(Guid? applicationId) { return _Events; }
        public static bool Messaging(Guid? applicationId) { return _Messaging; }
        public static bool Chat(Guid? applicationId) { return _Chat; }
        public static bool Resume(Guid? applicationId) { return _Resume; }
        public static bool VisualMap(Guid? applicationId) { return _VisualMap; }
        public static bool RealTime(Guid? applicationId) { return _RealTime; }
        public static bool Explorer(Guid? applicationId) { return _Explorer; }
        public static bool RestAPI(Guid? applicationId) { return _RestAPI; }
        public static bool Recommender(Guid? applicationId) { return _Recommender; }
    }

    public static class GlobalSettings
    {
        public static int? MaxAllowedActiveUsersCount = null;
        public static DateTime? SystemExpirationDate = null;
        public static List<Guid> TenantIDs = new List<Guid>();
        private static int _MaxTenantsCount = 20;
        public static int MaxTenantsCount
        {
            get { return TenantIDs != null && TenantIDs.Count > 0 ? TenantIDs.Count : _MaxTenantsCount; }
        }

        public static string RefSysID = "";

        //--> Sanaye Shimiyaei Esfahan  --> public static string RefSysID = "D0FF47EF67D401C045203456F22DC9D3"; Old --> "5D1295EF2FC1E946AF41945692D0BA81";
        //--> Shiroudi Esfahan          --> public statis string RefSysID = "D0FF47EF67D401C045203456F22DC9D3"; Old --> "5D1295EF2FC1E946AF41945692D0BA81";
        //--> 7-Tir e Esfahan           --> public static string RefSysID = "591C2F04D9AF231FDE5B394FB8433109"; Old --> "A7E6D41B66AC1B223CA8B6B07B377472";
        //--> Niroo Terans Shiraz       --> public statis string RefSysID = "EE43EEF8E568896B5E88B56CFC2914F4";
        //--> Ghaltak Sazan Sepahan     --> public statis string RefSysID = "1CF8548A40A9D63A071D1DC86E14C8F1";
        //--> Imam Hossein University   --> public static string RefSysID = "7C694E66A3423992BACD5546EBC264E6";
        //--> Barghe Ghazvin            --> public static string RefSysID = "2718B1DD474B62D34F8E14EB2AD85E86"; Old --> "911355B67B74FF5F18A3BF6A360365D8"; Old --> "EA26520755852E1F5EB673747FC94343";
        //--> Fanavaran Parsian         --> public static string RefSysID = "355BEE5246D9F08C50B75FCA484E49BA";
        //--> Iran Khodro               --> public static string RefSysID = "1CB4DBD408D68667D5FC67FC8061C6AB"; Old --> "41370E8FFA3498AE86450D92E750A83E";
        //--> Afta                      --> public static string RefSysID = "A5037653EFBC5F530A9E15A0C505750F";
        //--> Petroshimi Ghadir         --> public static string RefSysID = "2F54046CBD555FF98E262140D9F09693";
        //--> Barghe Bakhtar            --> public static string RefSysID = "E4DC69F6EC4D5155F8826EE326DF1535";
        //--> Sapco                     --> public static string RefSysID = "7C2E611C9879507665AB74864B1DE4F0"; Old --> "17BF7C85A0E0C7C488D8E3057D709921";
        //--> MMTE                      --> public static string RefSysID = "F14E0B202C6B6D0E9E55E41686063BF7"; Old --> "1B9B0B18C85E3C979AE36732859F0763";
        //--> Defa Melli University     --> public static string RefSysID = "A42CAAEFA0B9A5CD88B7617C2A97AB0F"; Old --> "911355B67B74FF5F18A3BF6A360365D8";
        //--> Zamin Shenasi (Naft Khiz) --> public static string RefSysID = "1B9B0B18C85E3C979AE36732859F0763"; Old --> "AA81A04C76A493F325950F10AC4C4CBD";
        //--> Sazman Omran e Tehran     --> public static string RefSysID = "419ECB7D2083B6BFE96F524A986EACBF"; Old --> "02BDF2F8EE2399A7CA9B04AF3D46CF6B"; Old --> "FBF55CB571D8AAC7EA35E7B0583C8A5E"; Old --> "02BDF2F8EE2399A7CA9B04AF3D46CF6B"; Old --> "D547FF02EA7E5CCC8A820DE194EF75BB";
        //--> Barname & Budje Khuzestan --> public static string RefSysID = "1B9B0B18C85E3C979AE36732859F0763";
        //--> Fanasa                    --> public static string RefSysID = "69ADD1410D14155F9B99F87BC2372B06"; Old --> "948BA6E22176DE00C67765CCCE604482";
        //--> Mapna - Pars              --> public static string RefSysID = "5518CE91100C3DE88432074453068B41"; Old --> "41176C506C5CFCB4DC46AD4F6DEB2C23"; Old --> "254011D53EDD4F25640E66F97AF918C9";
        //--> Jang Afzar Sazi           --> public static string RefSysID = "9EAD957C2EEFE81AA27302A20CC8E7A9";
        //--> Shahrdari Esfahan         --> public static string RefSysID = "911355B67B74FF5F18A3BF6A360365D8"; Old --> "7F2CA1D701F0002CCFD0B71697C15476"; Old --> "5D1295EF2FC1E946AF41945692D0BA81"; Old --> "1B9B0B18C85E3C979AE36732859F0763";
        //--> Damavand (ECD)            --> public static string RefSysID = "CD2AF7E1B7F476EA91F7A8D1B95A5F64"; Old --> "2F763F60D042BD2A77050629F6D02C2C";
        //--> Sanad Pardaz              --> public static string RefSysID = "C2F0D6193406E4BAFB6A9A5B0187939F";
        //--> Gas Co. Tehran            --> public static string RefSysID = "F14E0B202C6B6D0E9E55E41686063BF7"; Old --> "0986870509CCFEDFB05EE93AB4EECB24"; Old --> "A42CAAEFA0B9A5CD88B7617C2A97AB0F"; Old --> "C2879E261D83255CBDA6D2F07A5F660F";
        //--> Gas Co. Public            --> public static string RefSysID = "A42CAAEFA0B9A5CD88B7617C2A97AB0F";
        //--> Sa Afta                   --> public static string RefSysID = "9AEC04C8827FD7B3027267A2DB2D3071";
        //--> Fajre Shiraz (Rabani)     --> public static string RefSysID = "911355B67B74FF5F18A3BF6A360365D8";
        //--> Otagh Bazargani Iran      --> public static string RefSysID = "878AC841BF7D29D620EAF014C85AC2CA";
        //--> POGC                      --> public static string RefSysID = "5C71A182CB487E3BA9C966BCA6A21A47"; Old --> "419ECB7D2083B6BFE96F524A986EACBF"; Old --> "48FA9AC73CDCD83B1AA35B5C4A038326"; Old --> "F580E5FAD2EAF9FF908E29B25890B4AC"; Old --> "5518CE91100C3DE88432074453068B41";
        //--> Sherkat Shahrakha Golestan--> public static string RefSysID = "A74A781624E3044CBC19A7E63CF047E7"; Old --> "8265781DF277C6D5F5D8B71AC9E804D9"; Old --> "EEAC0977F76632E4D3C1E92F5DF29556"; Old --> "540B830848FDF691036863F6E4D13A22";
        //--> Pajuheshkade Shimiai Sasad--> public static string RefSysID = "1B9B0B18C85E3C979AE36732859F0763";
        //--> Shahid Ghoddusi           --> public static string RefSysID = "5D1295EF2FC1E946AF41945692D0BA81"; Old --> "66F789BAF5BA85538D6BCB02DAC13DF6"; Old --> "1B9B0B18C85E3C979AE36732859F0763";
        //--> Mohit Zist e Gorgan       --> public static string RefSysID = "7C2E611C9879507665AB74864B1DE4F0"; Old --> "8265781DF277C6D5F5D8B71AC9E804D9"; Old --> "C9D2F402CDCC1AC23DDABCDB63E6CABE";
        //--> Sadefa                    --> public static string RefSysID = "911355B67B74FF5F18A3BF6A360365D8"; Old --> "0B8E21EE3DFF62EE0162E14D509F082B"; Old --> "E260B95ADDF5332A4445093329D2EC12";
        //--> Pajouheshkade Khodroi     --> public static string RefSysID = "41176C506C5CFCB4DC46AD4F6DEB2C23"; Old --> "9CBECA3D18EF6CB7C2602D124764608F";
        //--> Mokhaberat e Iran         --> public static string RefSysID = "946A8E92BC5C1185E99D0D97F3924809"; Old --> "1CB4DBD408D68667D5FC67FC8061C6AB"; Old --> "5D1295EF2FC1E946AF41945692D0BA81"; Old --> "5518CE91100C3DE88432074453068B41";
        //--> Sarkhun o Gheshm          --> public static string RefSysID = "981C69146F098EA746F3EF973A90D69D"; Old --> "E25D456216E67FA68FBE0D6914A55C0E"; Old --> "7AFDFD4238D2048B9C260883BE366F23";
        //--> Hesa                      --> public static string RefSysID = "69ADD1410D14155F9B99F87BC2372B06"; Old --> "93A32FD537E8BE4AC15174ECD8CA1459"; Old --> "F5BCA953AF85F86103F9135F1B4AA6B9";
        //--> Optic Server              --> public static string RefSysID = "ED194FDC831A12E512D24401B84B1A1E"; Old --> "34EE6E8F703325ED995AB435AA6B0333"; Old --> "624D1139AD87B9A8B2070C8235CB5CAF"; Old --> "C9B536EABF001FE76283FF76F7CFB9CC"; ServerURL: 'http://www.danesh.com:3456'
        //--> Optic Laptop              --> public static string RefSysID = "8DD2A736027DF0E4804312BB4376B4AE";
        //--> Shahrdari Mash'had Server --> public static string RefSysID = "DF75B5B39D2158690C4E7B6F81E3D4A9"; Old --> "C2879E261D83255CBDA6D2F07A5F660F"; Old --> "254011D53EDD4F25640E66F97AF918C9"; Old --> "41176C506C5CFCB4DC46AD4F6DEB2C23"; Old --> "6C0CF33FE77A511504CB85A1698DB704";
        //--> Foulad Alliajy            --> public static string RefSysID = "DEB6DD4B4650090298EC4C8895CD339C"; Old --> "0690AC56C666A3EBC4D7C449D0FBD497"; Old --> "73192175747A53096D0772F15E5E3CBD"; Old --> "1F0CDF686A8682574BD1CAC81A57EC99"; Old --> "D0E758D8C21D320AED4147C8E9B003A7";
        //--> Eghtesad Novin Bank       --> public static string RefSysID = "FC5AD7342E682DFD16757330CD0CD5BE"; Old --> "A42CAAEFA0B9A5CD88B7617C2A97AB0F"; Old --> "F1EBE2EC003D3FFF2DF70628D27E1C3F"; Old --> "B0F674DBA39C6288E43A1201BA8058DE"; Old --> "7CFA5E5C6559A281CEB2EE899278E722";
        //--> Shahid Moghadame Yazd     --> public static string RefSysID = "1A741D55921AC4DC10370E747DA3BF52";
        //--> Sagha                     --> public static string RefSysID = "CD2AF7E1B7F476EA91F7A8D1B95A5F64"; Old --> "CD2AF7E1B7F476EA91F7A8D1B95A5F64"; Old --> "A90B0DB697C29391F12F73E33F6B7E6F"; Old --> "69add1410d14155f9b99f87bc2372b06"; Old --> "0B8E21EE3DFF62EE0162E14D509F082B";
        //--> InKnowTex Server          --> public static string RefSysID = "568B8FA5CDFD8A2623BDA1D8AB7B7B34"; Old --> "BB2A8F3D266E89AEC57AC475E1A9C9F1";
        //--> Vahid Eslami Laptop       --> public static string RefSysID = "568B8FA5CDFD8A2623BDA1D8AB7B7B34";
        //--> Ramin Laptop              --> public static string RefSysID = "BD9B047AD0236C4ED9AF706C1318E96F";
        //--> Hossein Laptop            --> public static string RefSysID = "0B5A7DE4E6D123FDD555712175B9CE92";
        //--> Bolouri Laptop            --> public static string RefSysID = "86587F22015204DBC72B3E0AAD38E9D9";
        //--> Khashayar Laptop          --> public static string RefSysID = "84BB4B3E60D26671BC943A327ED18A67";
        //--> Khashayar New Laptop      --> public static string RefSysID = "FC986991E4C54C70E9796660D53EA9B4";
        //--> Aghaye Taghichian Laptop  --> public static string RefSysID = "7746D2B912AFDB4134FA4E7A563715CC";
        //--> Khanome Mirshahi Laptop   --> public static string RefSysID = "451C365F0086B33F1718F6AD81DE715F";
        //--> Mokhtari PC               --> public static string RefSysID = "568B8FA5CDFD8A2623BDA1D8AB7B7B34";
        //--> Banki2 - PersianTools     --> public static string RefSysID = "C9D2F402CDCC1AC23DDABCDB63E6CABE";
        //--> KMAcademy.ir              --> public static string RefSysID = "C9D2F402CDCC1AC23DDABCDB63E6CABE";
        //--> Foroush Laptop            --> public static string RefSysID = "568B8FA5CDFD8A2623BDA1D8AB7B7B34";
        //--> GamificationClub          --> public static string RefSysID = "911355B67B74FF5F18A3BF6A360365D8";
        //--> Pour Barzegar Laptop      --> public static string RefSysID = "19CFDCE9CF80A4C6A689FC0FA43EE245";
        //--> Poshtibani PC             --> public static string RefSysID = "568B8FA5CDFD8A2623BDA1D8AB7B7B34";
        //--> Farhad Alizedeh           --> public static string RefSysID = "4490271C3A5B5266BC7F4CB7DB1CA378";
        //--> Ghasem Rezaei             --> public static string RefSysID = "DDAD20D253AC99AC6965FAF317AEA721";
        //--> Shabake Moshaveran        --> public static string RefSysID = "1E4A1B03D1B6CD8A174A826F76E009F4";
        //--> Ahmad Morshedi Laptop     --> public static string RefSysID = "7CFA5E5C6559A281CEB2EE899278E722";
        //--> Project Laptop            --> public static string RefSysID = "19CFDCE9CF80A4C6A689FC0FA43EE245";
        //--> Sorkhpour - DE-KMS.de     --> public static string RefSysID = "44012F38D0ED4F18949B681AD6D3788F";


        /* TenantIDs */
        //--> Shiroudi Esfahan          --> "82ADAE1C-BE46-44E8-A74E-6F2107A0F845";
        //--> Shimiaee Esfahan          --> "22546D79-00CC-474D-8D65-5782C336E37E";

        /*
        private static void __tids()
        {
            List<Guid> ShahrdariMashhad = new List<Guid>()
            {
                Guid.Parse("08C72552-4F2C-473F-B3B0-C2DACF8CD6A9"),
                Guid.Parse("564A76D6-ED2F-402C-A640-633B6CD69561"),
                Guid.Parse("035AD428-599A-478D-A73D-D77363CC19CF"),
                Guid.Parse("6C25DDBD-1F79-4E30-9D8B-43E84A7912BF"),
                Guid.Parse("53A73BDF-ABB1-488F-8CF3-D28A8C63C8DA"),
                Guid.Parse("5272D346-2C8B-4DB8-82D7-CFF42707A735"),
                Guid.Parse("E0DF511E-C68C-4FA5-B179-FDC64471A47E"),
                Guid.Parse("C0D719CC-AE2E-445B-AEB3-C857A6F88FC8")
            };
        }
        */
        /* end of TenantIDs */
    }

    public enum Edition
    {
        Custom,
        Fandogh
    }
}
