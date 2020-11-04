using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Search
{
    public enum SearchDocType
    {
        All,
        Node,
        NodeType,
        Question,
        File,
        User
    }

    public class SearchDoc
    {
        public Guid ID;
        public Guid? TypeID;
        public string Type;
        public string AdditionalID;
        public string Title;
        public string Description;
        public string Tags;
        public bool? Deleted;
        public string Content;
        public string FileContent;
        public bool? AccessIsDenied;
        public SearchDocType SearchDocType;
        public DocFileInfo FileInfo;

        public bool NoContent
        {
            get
            {
                return string.IsNullOrEmpty(Tags) && string.IsNullOrEmpty(Description) &&
                    string.IsNullOrEmpty(Content) && string.IsNullOrEmpty(FileContent) &&
                    SearchDocType != SearchDocType.User && SearchDocType != SearchDocType.NodeType;
            }
        }


        public static float BoostMax = (float)22;
        public static float BoostMin = (float)0.01;
        public static float BoostThreshold = (float)8;
        public static float BoostStep = (float)2;

        public static float BoostAdditinalID = (float)BoostMax;
        public static float BoostTitle = (float)BoostMax;
        public static float BoostTags = (float)(BoostTitle - (5 * BoostStep));
        public static float BoostDescription = (float)(BoostTags - BoostStep);
        public static float BoostContent = (float)(BoostDescription - BoostStep);
        public static float BoostFileContent = (float)(BoostContent - BoostStep);
        public static float BoostDocType = (float)BoostMin;
        public static float BoostTypeID = (float)BoostMin;
        public static float BoostType = (float)BoostMin;
        public static float BoostNoContent = (float)BoostMax;

        public SearchDoc()
        {
            FileInfo = new DocFileInfo();
        }

        public Document ToDocument(Guid applicationId)
        {
            try
            {
                Type = PublicMethods.verify_string(Type);
                AdditionalID = PublicMethods.verify_string(AdditionalID);
                Title = PublicMethods.verify_string(Title);
                Description = PublicMethods.verify_string(Description);
                Tags = PublicMethods.verify_string(Tags);
                Content = PublicMethods.verify_string(Content);
                FileContent = PublicMethods.verify_string(FileContent);

                Type = PublicMethods.verify_string(Type);
                Field f;
                Lucene.Net.Documents.Document postDocument = new Lucene.Net.Documents.Document();

                f = new Field("ID", ID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
                postDocument.Add(f);

                f = new Field("TypeID", TypeID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
                f.Boost = BoostTypeID;
                postDocument.Add(f);

                if (!string.IsNullOrEmpty(Type))
                {
                    f = new Field("Type", Type, Field.Store.YES,
                       Field.Index.NOT_ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostType;
                    postDocument.Add(f);
                }

                if (!string.IsNullOrEmpty(AdditionalID))
                {
                    f = new Field("AdditionalID", AdditionalID,
                         Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostAdditinalID;
                    postDocument.Add(f);
                }

                if (!string.IsNullOrEmpty(Title))
                {
                    f = new Field("Title", Title,
                       Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostTitle;
                    postDocument.Add(f);
                }

                if (!string.IsNullOrEmpty(Description))
                {
                    f = new Field("Description", Description,
                       Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostDescription;
                    postDocument.Add(f);
                }

                if (!string.IsNullOrEmpty(Tags))
                {
                    f = new Field("Tags", Tags.ToString(),
                       Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostTags;
                    postDocument.Add(f);
                }

                f = new Field("Deleted", Deleted.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
                postDocument.Add(f);

                if (!string.IsNullOrEmpty(Content))
                {
                    f = new Field("Content", Content.ToString(),
                       Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostContent;
                    postDocument.Add(f);
                }

                if (!string.IsNullOrEmpty(FileContent))
                {
                    f = new Field("FileContent", FileContent.ToString(),
                       Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    f.Boost = BoostFileContent;
                    postDocument.Add(f);
                }

                f = new Field("NoContent", NoContent.ToString().ToLower(),
                    Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                f.Boost = BoostNoContent;
                postDocument.Add(f);

                f = new Field("SearchDocType", SearchDocType.ToString(), Field.Store.YES, Field.Index.ANALYZED,
                    Field.TermVector.WITH_POSITIONS_OFFSETS);
                f.Boost = BoostDocType;
                postDocument.Add(f);

                return postDocument;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "ConvertToLuceneDocument", ex, ModuleIdentifier.SRCH);
                return null;
            }
        }

        public static SearchDoc ToSearchDoc(Document doc)
        {
            SearchDoc retSD = new SearchDoc();
            
            switch (doc.GetField("SearchDocType").StringValue)
            {
                case "Node":
                    if (doc.GetField("ID") != null) retSD.ID = Guid.Parse(doc.GetField("ID").StringValue);
                    if (doc.GetField("Deleted") != null) retSD.Deleted = Convert.ToBoolean(doc.GetField("Deleted").StringValue);
                    if (doc.GetField("TypeID") != null) retSD.TypeID = Guid.Parse(doc.GetField("TypeID").StringValue);
                    if (doc.GetField("Type") != null) retSD.Type = doc.GetField("Type").StringValue;
                    if (doc.GetField("AdditionalID") != null) retSD.AdditionalID = doc.GetField("AdditionalID").StringValue;
                    if (doc.GetField("Title") != null) retSD.Title = doc.GetField("Title").StringValue;
                    if (doc.GetField("Description") != null) retSD.Description = doc.GetField("Description").StringValue;
                    if (doc.GetField("Tags") != null) retSD.Tags = doc.GetField("Tags").StringValue;
                    if (doc.GetField("Content") != null) retSD.Content = doc.GetField("Content").StringValue;
                    if (doc.GetField("FileContent") != null) retSD.FileContent = doc.GetField("FileContent").StringValue;
                    retSD.SearchDocType = SearchDocType.Node;
                    break;
                case "NodeType":
                    if (doc.GetField("ID") != null) retSD.ID = Guid.Parse(doc.GetField("ID").StringValue);
                    if (doc.GetField("Deleted") != null) retSD.Deleted = Convert.ToBoolean(doc.GetField("Deleted").StringValue);
                    if (doc.GetField("Title") != null) retSD.Title = doc.GetField("Title").StringValue;
                    if (doc.GetField("Description") != null) retSD.Description = doc.GetField("Description").StringValue;
                    retSD.SearchDocType = SearchDocType.NodeType;
                    break;
                case "Question":
                    if (doc.GetField("ID") != null) retSD.ID = Guid.Parse(doc.GetField("ID").StringValue);
                    if (doc.GetField("Deleted") != null) retSD.Deleted = Convert.ToBoolean(doc.GetField("Deleted").StringValue);
                    if (doc.GetField("Title") != null) retSD.Title = doc.GetField("Title").StringValue;
                    if (doc.GetField("Description") != null) retSD.Description = doc.GetField("Description").StringValue;
                    if (doc.GetField("Content") != null) retSD.Content = doc.GetField("Content").StringValue;
                    retSD.SearchDocType = SearchDocType.Question;
                    break;
                case "File":
                    if (doc.GetField("ID") != null) retSD.ID = Guid.Parse(doc.GetField("ID").StringValue);
                    if (doc.GetField("Type") != null) retSD.Type = doc.GetField("Type").StringValue;
                    if (doc.GetField("Title") != null) retSD.Title = doc.GetField("Title").StringValue;
                    if (doc.GetField("FileContent") != null) retSD.FileContent = doc.GetField("FileContent").StringValue;
                    retSD.SearchDocType = SearchDocType.File;
                    break;
                case "User":
                    if (doc.GetField("ID") != null) retSD.ID = Guid.Parse(doc.GetField("ID").StringValue);
                    if (doc.GetField("Deleted") != null) retSD.Deleted = Convert.ToBoolean(doc.GetField("Deleted").StringValue);
                    if (doc.GetField("AdditionalID") != null) retSD.AdditionalID = doc.GetField("AdditionalID").StringValue;
                    if (doc.GetField("Title") != null) retSD.Title = doc.GetField("Title").StringValue;
                    retSD.SearchDocType = SearchDocType.User;
                    break;
            }

            return retSD;
        }

        public static SearchDoc ToSearchDoc(Guid id, Guid? typeID, string content, string additionalID, bool deleted, string type,
            SearchDocType docType, string title = null, string tags = null, string description = null, string fileContect = null)
        {
            SearchDoc sd = new SearchDoc();

            sd.ID = id;
            sd.Deleted = deleted;
            sd.TypeID = typeID;
            sd.Type = type;
            sd.AdditionalID = additionalID;
            sd.Title = title;
            sd.Description = description;
            sd.Tags = tags;
            sd.Content = content;
            sd.FileContent = fileContect;
            sd.SearchDocType = docType;

            return sd;
        }

        public static SearchDoc ToSearchDoc(SolrDoc doc) {
            try
            {
                SearchDocType docType = SearchDocType.All;

                string docId = doc.get_main_id();

                if (!Enum.TryParse<SearchDocType>(doc.SearchDocType, out docType)) return null;

                return ToSearchDoc(Guid.Parse(docId),
                    string.IsNullOrEmpty(doc.TypeID) ? (Guid?)null : Guid.Parse(doc.TypeID),
                    doc.Content, doc.AdditionalID, doc.Deleted, doc.Type, docType,
                    doc.Title, doc.Tags, doc.Description, doc.FileContent);
            }
            catch { return null; }
        }

        public SolrDoc toSolrDoc(Guid applicationId) {
            return new SolrDoc()
            {
                ID = applicationId.ToString() + "!" + ID.ToString(),
                TypeID = !TypeID.HasValue ? null : TypeID.ToString(),
                Type = PublicMethods.verify_string(Type),
                AdditionalID = PublicMethods.verify_string(AdditionalID),
                Title = PublicMethods.verify_string(Title),
                Description = PublicMethods.verify_string(Description),
                Tags = PublicMethods.verify_string(Tags),
                Content = PublicMethods.verify_string(Content),
                FileContent = PublicMethods.verify_string(FileContent),
                Deleted = Deleted.HasValue && Deleted.Value,
                NoContent = NoContent,
                SearchDocType = SearchDocType.ToString()
            };
        }

        public string toJson(Guid applicationId, bool exact) {
            string iconUrl = string.Empty;

            switch (SearchDocType)
            {
                case SearchDocType.Node:
                    iconUrl = DocumentUtilities.get_icon_url(applicationId, ID, DefaultIconTypes.Node, TypeID);
                    break;
                case SearchDocType.User:
                    iconUrl = DocumentUtilities.get_personal_image_address(applicationId, ID);
                    break;
                case SearchDocType.File:
                    iconUrl = DocumentUtilities.get_icon_url(applicationId, Type);
                    break;
            }

            return "{\"ID\":\"" + ID.ToString() + "\"" +
                ",\"ItemType\":\"" + SearchDocType.ToString() + "\"" +
                ",\"Type\":\"" + Base64.encode(Type) + "\"" +
                ",\"AdditionalID\":\"" + Base64.encode(AdditionalID) + "\"" +
                ",\"IconURL\":\"" + iconUrl + "\"" +
                ",\"Title\":\"" + Base64.encode(Title) + "\"" +
                ",\"Description\":\"" + Base64.encode(Description) + "\"" +
                ",\"Exact\":" + exact.ToString().ToLower() +
                ",\"AccessIsDenied\":" + (AccessIsDenied.HasValue && AccessIsDenied.Value).ToString().ToLower() +
                (FileInfo == null || !FileInfo.OwnerNodeID.HasValue ? string.Empty :
                    ",\"FileOwnerNode\":{\"NodeID\":\"" + FileInfo.OwnerNodeID.ToString() + "\"" +
                        ",\"Name\":\"" + Base64.encode(FileInfo.OwnerNodeName) + "\"" +
                        ",\"NodeType\":\"" + Base64.encode(FileInfo.OwnerNodeType) + "\"" +
                    "}"
                ) +
                "}";
        }
    }
}
