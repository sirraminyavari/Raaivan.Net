using CommonServiceLocator;
using SolrNet;
using SolrNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using SolrNet.Commands.Parameters;
using System.Text.RegularExpressions;
using System.IO;

namespace RaaiVan.Modules.Search
{
    public class SolrDoc
    {
        [SolrUniqueKey("id")]
        public string ID { get; set; }

        [SolrField("type_id")]
        public string TypeID { get; set; }

        [SolrField("type")]
        public string Type { get; set; }

        [SolrField("additional_id")]
        public string AdditionalID { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        [SolrField("description")]
        public string Description { get; set; }

        [SolrField("tags")]
        public string Tags { get; set; }

        [SolrField("deleted")]
        public bool Deleted { get; set; }

        [SolrField("content")]
        public string Content { get; set; }

        [SolrField("file_content")]
        public string FileContent { get; set; }

        [SolrField("no_content")]
        public bool NoContent { get; set; }

        [SolrField("search_doc_type")]
        public string SearchDocType { get; set; }
    }

    public class QueryTerms {
        /*
        List<string> specialChars = new List<string>() {
            "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "\"", "~", "*", "?", ":", "/"
        };
        */

        private List<string> Terms;

        public QueryTerms(string phrase) {
            Terms = QueryTerms.parse(phrase);
        }

        public string get_query(List<KeyValuePair<string, double>> fieldBoosts)
        {
            return string.Join(" ", fieldBoosts.Select(f => f.Key + ":(" + string.Join(" ", Terms) + ")"));
        }

        private static List<string> parse(string phrase)
        {
            List<string> terms = new List<string>();

            if (string.IsNullOrEmpty(phrase)) return terms;

            phrase = " " + PublicMethods.verify_string(phrase) + " ";

            string pattern = "\\s[\\+\\-]?\"[^\"]*\"\\s";

            Dictionary<string, string> quotesDic = new Dictionary<string, string>();

            MatchCollection matches = null;

            while (true)
            {
                matches = string.IsNullOrEmpty(phrase) ? (MatchCollection)null : (new Regex(pattern)).Matches(phrase);

                if (matches == null || matches.Count == 0) break;

                for (int i = 0; i < matches.Count; i++)
                {
                    string key = PublicMethods.random_string(10);
                    quotesDic[key] = matches[i].Value;
                    phrase = phrase.Replace(matches[i].Value.Trim(), " " + key + " ");
                }
            }

            phrase.Split(' ')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => quotesDic.ContainsKey(x) ? quotesDic[x].Trim() : x)
                .ToList()
                .ForEach(x =>
                {
                    string prefix = new[] { "+", "-" }.Where(c => x.StartsWith(c)).FirstOrDefault();

                    if (!string.IsNullOrEmpty(prefix)) x = x.Length > 1 ? x.Substring(1) : string.Empty;

                    if (string.IsNullOrEmpty(x)) return;

                    bool isQuote = x[0] == '"' && x[x.Length - 1] == '"';

                    if (isQuote)
                    {
                        if (x.Length <= 2) return;
                        x = x.Substring(1, x.Length - 2).Trim();
                    }

                    new[] {
                        "+", "-", "&", "|", "!", "(", ")", "{", "}", "[", "]", "^", "\"", "~", ":", "/",
                        "\\", ".", ",", "،", "؛", "؟"
                    }.ToList().ForEach(c => x = x.Replace(c, " "));

                    x = x.Trim();
                    if (string.IsNullOrEmpty(x)) return;

                    isQuote = isQuote && x.IndexOf(" ") > 0;
                    if (isQuote) x = "\"" + x + "\"";

                    if (!string.IsNullOrEmpty(prefix)) x = prefix + x;

                    if (!isQuote && x.IndexOf(" ") > 0)
                        terms.AddRange(QueryTerms.parse(x));
                    else
                    {
                        if (!isQuote && !(new[] { "?", "*" }).Any(c => x.IndexOf(c) >= 0)) x += "~";
                        else if (isQuote) x += "^";

                        terms.Add(x);
                    }
                });

            return terms.Distinct().Where(t => !string.IsNullOrEmpty(t)).ToList();
        }
    }

    public static class SolrAPI
    {
        //private static string ZOOKEEPER_CONNECTION_STRING = "192.168.43.190:2181/solr";

        private static string SOLR_CONNECTION_STRING
        {
            get { return !RaaiVanSettings.Solr.Enabled ? string.Empty : RaaiVanSettings.Solr.URL + "/" + RaaiVanSettings.Solr.CollectionName; }
        }

        private static bool Inited = false;

        /*
        private static void init_zookeeper(System.Action callback)
        {
            if (Inited_Zookeeper)
            {
                callback();
                return;
            }

            Inited_Zookeeper = true;

            SolrNet.Cloud.Startup.InitAsync<SolrDoc>(new SolrCloudStateProvider("192.168.43.190:2181/solr"), "rv_docs", isPostConnection: true)
                .ContinueWith((t) => callback());
        }
        */

        private static void init()
        {
            if (Inited || !RaaiVanSettings.Solr.Enabled) return;
            Inited = true;
            
            try { SolrNet.Startup.Init<SolrDoc>(SOLR_CONNECTION_STRING); }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, "SolrConnection", ex, ModuleIdentifier.SRCH);
            }
        }

        private static ISolrOperations<SolrDoc> get_solr_operator() {
            init();

            try { return ServiceLocator.Current.GetInstance<ISolrOperations<SolrDoc>>(); }
            catch (Exception ex) {
                LogController.save_error_log(null, null, "GetSolrOperatorInstance", ex, ModuleIdentifier.SRCH);
                return null;
            }
        }

        public static bool add(Guid applicationId, List<SearchDoc> documents) {
            try
            {
                ISolrOperations<SolrDoc> solr = get_solr_operator();
                solr.AddRange(documents.Select(d => d.toSolrDoc(applicationId)));
                ResponseHeader response = solr.Commit();

                return response.Status == 0;
            }
            catch (Exception ex) {
                LogController.save_error_log(null, null, "SolrAddDocuments", ex, ModuleIdentifier.SRCH);
                return false;
            }
        }

        public static bool delete(Guid applicationId, SearchDoc document)
        {
            try
            {
                ISolrOperations<SolrDoc> solr = get_solr_operator();
                solr.Delete(document.toSolrDoc(applicationId));
                ResponseHeader response = solr.Commit();

                return response.Status == 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, "SolrRemoveDocument", ex, ModuleIdentifier.SRCH);
                return false;
            }
        }

        public static List<SolrDoc> search(Guid applicationId, string phrase, int count, int lowerBoundary)
        {
            ISolrOperations<SolrDoc> solr = get_solr_operator();

            QueryTerms searchTerms = new QueryTerms(phrase);

            List<KeyValuePair<string, double>> fieldBoosts = new List<KeyValuePair<string, double>>() {
                new KeyValuePair<string, double>("title", 1)
            };
            
            SolrQueryResults<SolrDoc> results = solr.Query(searchTerms.get_query(fieldBoosts), new QueryOptions()
            {
                Rows = count,
                StartOrCursor = new StartOrCursor.Start(Math.Max(0, lowerBoundary - 1)),
                Highlight = new HighlightingParameters() { Fields = new[] { "title", "description", "tags", "content", "file_content" } },
                ExtraParams = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("_route_", applicationId.ToString() + "!")
                }
            });

            return results.ToList();
        }

        public static string extract_file_content(string phrase, int count, int lowerBoundary)
        {
            ISolrOperations<SolrDoc> solr = get_solr_operator();
            
            using (FileStream file = File.OpenRead("")) {
                ExtractResponse response = solr.Extract(new ExtractParameters(file, "12") {
                    ExtractOnly = true,
                    ExtractFormat = ExtractFormat.Text
                });

                return response.Content;
            }
        }
    }
}
