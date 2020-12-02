﻿using CommonServiceLocator;
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
using SolrNet.Impl;

namespace RaaiVan.Modules.Search
{
    public class SolrDoc
    {
        [SolrUniqueKey("id")]
        public string ID { get; set; }

        [SolrField("search_doc_type")]
        public string SearchDocType { get; set; }

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

        [SolrField("content")]
        public string Content { get; set; }

        [SolrField("file_content")]
        public string FileContent { get; set; }

        [SolrField("no_content")]
        public bool NoContent { get; set; }

        [SolrField("deleted")]
        public bool Deleted { get; set; }

        public string get_main_id()
        {
            return (!string.IsNullOrEmpty(ID) && ID.LastIndexOf("!") > 0) ? ID.Substring(ID.LastIndexOf("!") + 1) : ID;
        }
    }

    public class QueryTerms
    {
        /*
        List<string> specialChars = new List<string>() {
            "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "\"", "~", "*", "?", ":", "/"
        };
        */

        private List<string> Terms;

        public QueryTerms(string phrase)
        {
            Terms = QueryTerms.parse(phrase);
        }

        public string get_query(List<KeyValuePair<string, double>> fieldBoosts,
            List<SearchDocType> docTypes, List<Guid> typeIds, List<string> types, bool forceHasContent)
        {
            List<string> constraints = new List<string>();

            if (docTypes != null && docTypes.Count > 0)
                constraints.Add("search_doc_type:(" + string.Join(" ", docTypes.Distinct()) + ")");

            if (typeIds != null && typeIds.Count > 0)
                constraints.Add("type_id:(" + string.Join(" ", typeIds.Distinct().Select(i => i.ToString())) + ")");

            if (types != null && types.Count > 0)
                constraints.Add("type:(" + string.Join(" ", types.Distinct().Select(i => i.ToString())) + ")");

            if (forceHasContent) constraints.Add("!no_content:true");

            string query = string.Join(" ", fieldBoosts.Select(f =>
            {
                double boostBase = f.Value;
                bool noBoost = boostBase == 0;
                if (boostBase <= 0) boostBase = 1;

                return f.Key + ":(" + string.Join(" ", Terms.Select(t =>
                {
                    t += t.IndexOf(" ") > 0 || noBoost ? "" : "^" + (Math.Truncate(boostBase * 100) / 100).ToString();
                    boostBase = boostBase * 0.8;
                    if (boostBase <= 0.4) boostBase = 0.4;
                    return t;
                })) + ")";
            })).Trim();

            return constraints.Count == 0 ? query :
                string.Join(" AND ", constraints) + (string.IsNullOrEmpty(query) ? string.Empty : " AND (" + query + ")");
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
                        if (!isQuote && !(new[] { "?", "*" }).Any(c => x.IndexOf(c) >= 0)) x += "~1";
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

            try
            {
                SolrConnection conn = new SolrConnection(SOLR_CONNECTION_STRING);

                if (!string.IsNullOrEmpty(RaaiVanSettings.Solr.Username) && !string.IsNullOrEmpty(RaaiVanSettings.Solr.Password))
                {
                    conn.HttpWebRequestFactory = new HttpWebAdapters.BasicAuthHttpWebRequestFactory(
                        RaaiVanSettings.Solr.Username, RaaiVanSettings.Solr.Password);
                }

                SolrNet.Startup.Init<SolrDoc>(conn);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, "SolrConnection", ex, ModuleIdentifier.SRCH);
            }
        }

        private static ISolrOperations<SolrDoc> get_solr_operator()
        {
            init();

            try { return ServiceLocator.Current.GetInstance<ISolrOperations<SolrDoc>>(); }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, "GetSolrOperatorInstance", ex, ModuleIdentifier.SRCH);
                return null;
            }
        }

        public static bool add(Guid applicationId, List<SearchDoc> documents)
        {
            try
            {
                ISolrOperations<SolrDoc> solr = get_solr_operator();
                solr.AddRange(documents.Select(d => d.toSolrDoc(applicationId)));
                ResponseHeader response = solr.Commit();

                return response.Status == 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, "SolrAddDocuments", ex, ModuleIdentifier.SRCH);
                return false;
            }
        }

        public static bool delete(Guid applicationId, List<SearchDoc> documents)
        {
            try
            {
                ISolrOperations<SolrDoc> solr = get_solr_operator();
                documents.ForEach(d => solr.Delete(d.toSolrDoc(applicationId)));
                ResponseHeader response = solr.Commit();

                return response.Status == 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, "SolrRemoveDocument", ex, ModuleIdentifier.SRCH);
                return false;
            }
        }

        public static List<SolrDoc> search(Guid applicationId, string phrase, List<SearchDocType> docTypes, List<Guid> typeIds,
            List<string> types, bool additionalId, bool title, bool description, bool tags, bool content, bool fileContent,
            bool forceHasContent, int count, int lowerBoundary, bool highlight, ref int totalCount)
        {
            ISolrOperations<SolrDoc> solr = get_solr_operator();

            QueryTerms searchTerms = new QueryTerms(phrase);

            List<KeyValuePair<string, double>> fieldBoosts = new List<KeyValuePair<string, double>>();

            docTypes = (docTypes == null ? new List<SearchDocType>() : docTypes.Where(d => d != SearchDocType.All)).Distinct().ToList();

            if (title) fieldBoosts.Add(new KeyValuePair<string, double>("title", 5));
            if (tags) fieldBoosts.Add(new KeyValuePair<string, double>("tags", 4));
            if (description) fieldBoosts.Add(new KeyValuePair<string, double>("description", 3));
            if (content) fieldBoosts.Add(new KeyValuePair<string, double>("content", 2));
            if (fileContent) fieldBoosts.Add(new KeyValuePair<string, double>("file_content", 1));
            if (additionalId) fieldBoosts.Add(new KeyValuePair<string, double>("additional_id", 0));

            string query = searchTerms.get_query(fieldBoosts, docTypes, typeIds, types, forceHasContent);

            QueryOptions queryOptions = new QueryOptions()
            {
                Rows = count + (count / 2),
                StartOrCursor = new StartOrCursor.Start(Math.Max(0, lowerBoundary)),
                ExtraParams = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("_route_", applicationId.ToString() + "!")
                },
                Fields = new[] { "id", "search_doc_type", "type_id", "type", "additional_id", "title", "no_content", "deleted" }
            };

            if (highlight) queryOptions.Highlight = new HighlightingParameters()
            {
                Fields = fieldBoosts.Where(b => b.Key != "additional_id").Select(b => b.Key).ToArray()
            };

            SolrQueryResults<SolrDoc> results = solr.Query(query, queryOptions);

            totalCount = results.NumFound;

            if (highlight) {
                results.Where(d => results.Highlights.ContainsKey(d.ID)).ToList().ForEach(doc => {
                    HighlightedSnippets snippets = results.Highlights[doc.ID];
                    doc.Description = string.Join(" ", snippets.Values.Select(v => string.Join(" ", v)))
                        .Replace("<em>", "<b>").Replace("</em>", "</b>");
                });
            }

            return results.ToList();
        }

        public static string extract_file_content(Guid applicationId, DocFileInfo file)
        {
            ISolrOperations<SolrDoc> solr = get_solr_operator();

            using (Stream content = new MemoryStream(file.toByteArray(applicationId)))
            {
                ExtractResponse response = solr.Extract(new ExtractParameters(content,
                    PublicMethods.get_random_number().ToString(), PublicMethods.random_string(10))
                {
                    ExtractOnly = true,
                    ExtractFormat = ExtractFormat.Text
                });

                return response.Content;
            }
        }
    }
}
