using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Wiki
{
    public static class WikiController
    {
        public static bool add_title(Guid applicationId, WikiTitle Info, bool? accept)
        {
            return DataProvider.AddTitle(applicationId, Info, accept);
        }

        public static bool modify_title(Guid applicationId, WikiTitle Info, bool? accept)
        {
            return DataProvider.ModifyTitle(applicationId, Info, accept);
        }

        public static bool remove_title(Guid applicationId, Guid titleId, Guid lastModifierUserId)
        {
            return DataProvider.ArithmeticDeleteTitle(applicationId, titleId, lastModifierUserId);
        }

        public static bool recycle_title(Guid applicationId, Guid titleId, Guid lastModifierUserId)
        {
            return DataProvider.RecycleTitle(applicationId, titleId, lastModifierUserId);
        }

        public static bool set_titles_order(Guid applicationId, List<Guid> titleIds)
        {
            return DataProvider.SetTitlesOrder(applicationId, titleIds);
        }

        public static List<WikiTitle> get_titles(Guid applicationId, 
            Guid ownerId, bool? isAdmin, Guid? currentUserId, bool deleted = false)
        {
            List<WikiTitle> retList = new List<WikiTitle>();
            DataProvider.GetTitles(applicationId, ref retList, ownerId, isAdmin, currentUserId, deleted);
            return retList;
        }

        public static List<WikiTitle> get_titles(Guid applicationId, ref List<Guid> titleIds, Guid currentUserId)
        {
            List<WikiTitle> retList = new List<WikiTitle>();
            DataProvider.GetTitles(applicationId, ref retList, ref titleIds, currentUserId);
            return retList;
        }

        public static WikiTitle get_title(Guid applicationId, Guid titleId, Guid currentUserId)
        {
            List<Guid> _tIds = new List<Guid>();
            _tIds.Add(titleId);
            return get_titles(applicationId, ref _tIds, currentUserId).FirstOrDefault();
        }

        public static bool has_title(Guid applicationId, Guid ownerId, Guid? currentUserId)
        {
            return DataProvider.HasTitle(applicationId, ownerId, currentUserId);
        }

        public static bool add_paragraph(Guid applicationId, Paragraph Info, bool? sendToAdmins, bool? hasAdmin, 
            List<Guid> adminUserIds, ref List<Dashboard> dashboards)
        {
            return DataProvider.AddParagraph(applicationId, Info, sendToAdmins, hasAdmin, adminUserIds, ref dashboards);
        }

        public static bool modify_paragraph(Guid applicationId, Paragraph Info, Guid? changeId2Accept, 
            bool? hasAdmin, List<Guid> adminUserIds, ref List<Dashboard> dashboards, 
            bool? citationNeeded = null, bool? apply = null, bool? accept = null)
        {
            return DataProvider.ModifyParagraph(applicationId, Info, changeId2Accept, hasAdmin, adminUserIds, 
                ref dashboards, citationNeeded, apply, accept);
        }

        public static bool remove_paragraph(Guid applicationId, Guid paragraphId, Guid lastModifierUserId)
        {
            return DataProvider.ArithmeticDeleteParagraph(applicationId, paragraphId, lastModifierUserId);
        }

        public static bool recycle_paragraph(Guid applicationId, Guid paragraphId, Guid lastModifierUserId)
        {
            return DataProvider.RecycleParagraph(applicationId, paragraphId, lastModifierUserId);
        }

        public static bool set_paragraphs_order(Guid applicationId, List<Guid> paragraphIds)
        {
            return DataProvider.SetParagraphsOrder(applicationId, paragraphIds);
        }

        public static List<Paragraph> get_paragraphs(Guid applicationId, ref List<Guid> paragraphIds, Guid currentUserId)
        {
            List<Paragraph> retList = new List<Paragraph>();
            DataProvider.GetParagraphs(applicationId, ref retList, ref paragraphIds, currentUserId);
            return retList;
        }

        public static Paragraph get_paragraph(Guid applicationId, Guid paragraphId, Guid currentUserId)
        {
            List<Guid> _pIds = new List<Guid>();
            _pIds.Add(paragraphId);
            return get_paragraphs(applicationId, ref _pIds, currentUserId).FirstOrDefault();
        }

        public static List<Paragraph> get_title_paragraphs(Guid applicationId, ref List<Guid> titleIds, 
            bool? isAdmin, Guid? currentUserId, bool removed)
        {
            List<Paragraph> retList = new List<Paragraph>();
            DataProvider.GetTitleParagraphs(applicationId, ref retList, ref titleIds, isAdmin, currentUserId, removed);
            return retList;
        }

        public static List<Paragraph> get_title_paragraphs(Guid applicationId, List<Guid> titleIds, 
            bool? isAdmin, Guid? currentUserId, bool removed)
        {
            return get_title_paragraphs(applicationId, ref titleIds, isAdmin, currentUserId, removed);
        }

        public static List<Paragraph> get_title_paragraphs(Guid applicationId, 
            Guid titleId, bool? isAdmin, Guid? currentUserId, bool removed)
        {
            List<Guid> _pIds = new List<Guid>();
            _pIds.Add(titleId);
            return get_title_paragraphs(applicationId, ref _pIds, isAdmin, currentUserId, removed);
        }

        public static bool has_paragraph(Guid applicationId, Guid titleOrOwnerId, Guid? currentUserId)
        {
            return DataProvider.HasParagraph(applicationId, titleOrOwnerId, currentUserId);
        }

        public static List<Paragraph> get_dashboard_paragraphs(Guid applicationId, Guid userId)
        {
            List<Paragraph> retList = new List<Paragraph>();
            DataProvider.GetDashboardParagraphs(applicationId, ref retList, userId);
            return retList;
        }

        public static List<User> get_paragraph_related_users(Guid applicationId, Guid paragraphId)
        {
            List<User> retList = new List<User>();
            DataProvider.GetParagraphRelatedUsers(applicationId, ref retList, paragraphId);
            return retList;
        }

        public static bool reject_change(Guid applicationId, Guid changeId, Guid evaluatorUserId)
        {
            return DataProvider.RejectChange(applicationId, changeId, evaluatorUserId);
        }

        public static bool accept_change(Guid applicationId, Guid changeId, Guid evaluatorUserId)
        {
            return DataProvider.AcceptChange(applicationId, changeId, evaluatorUserId);
        }

        public static bool remove_change(Guid applicationId, Guid changeId)
        {
            return DataProvider.ArithmeticDeleteChange(applicationId, changeId);
        }

        public static List<Change> get_changes(Guid applicationId, ref List<Guid> ChangeIDs)
        {
            List<Change> retList = new List<Change>();
            DataProvider.GetChanges(applicationId, ref retList, ref ChangeIDs);
            return retList;
        }

        public static Change get_change(Guid applicationId, Guid ChangeID)
        {
            List<Guid> _cIds = new List<Guid>();
            _cIds.Add(ChangeID);
            return get_changes(applicationId, ref _cIds).FirstOrDefault();
        }

        public static Change get_last_pending_change(Guid applicationId, Guid paragraphId, Guid userId)
        {
            return DataProvider.GetLastPendingChange(applicationId, paragraphId, userId);
        }

        public static List<Change> get_changes(Guid applicationId, ref List<Guid> paragraphIds, Guid? creatorUserId, 
            WikiStatuses? status, bool? applied)
        {
            List<Change> retList = new List<Change>();
            DataProvider.GetChanges(applicationId, ref retList, ref paragraphIds, creatorUserId, status, applied);
            return retList;
        }

        public static List<Change> get_changes(Guid applicationId, Guid paragraphId, Guid? creatorUserId,
            WikiStatuses? status, bool? applied)
        {
            List<Guid> ids = new List<Guid>();
            ids.Add(paragraphId);
            return get_changes(applicationId, ref ids, creatorUserId, status, applied);
        }

        public static List<Guid> get_changed_wiki_owner_ids(Guid applicationId, ref List<Guid> ownerIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetChangedWikiOwnerIDs(applicationId, ref retList, ref ownerIds);
            return retList;
        }

        public static List<Guid> get_changed_wiki_owner_ids(Guid applicationId, List<Guid> ownerIds)
        {
            return get_changed_wiki_owner_ids(applicationId, ref ownerIds);
        }

        private static void _get_wiki_owner(Guid applicationId, Guid id, ref Guid ownerId, ref WikiOwnerType ownerType)
        {
            DataProvider.GetWikiOwner(applicationId, ref ownerId, ref ownerType, id);
        }

        public static Guid get_wiki_owner(Guid applicationId, Guid id, ref WikiOwnerType ownerType)
        {
            Guid ownerId = Guid.Empty;
            _get_wiki_owner(applicationId, id, ref ownerId, ref ownerType);
            return ownerId;
        }

        public static Guid get_wiki_owner(Guid applicationId, Guid id)
        {
            Guid ownerId = Guid.Empty;
            WikiOwnerType ownerType = WikiOwnerType.NotSet;
            _get_wiki_owner(applicationId, id, ref ownerId, ref ownerType);
            return ownerId;
        }

        public static WikiOwnerType get_wiki_owner(Guid applicationId, Guid id, ref Guid ownerId)
        {
            WikiOwnerType ownerType = WikiOwnerType.NotSet;
            _get_wiki_owner(applicationId, id, ref ownerId, ref ownerType);
            return ownerType;
        }

        public static WikiOwnerType get_wiki_owner_type(Guid applicationId, Guid id)
        {
            Guid ownerId = Guid.Empty;
            WikiOwnerType ownerType = WikiOwnerType.NotSet;
            _get_wiki_owner(applicationId, id, ref ownerId, ref ownerType);
            return ownerType;
        }

        public static string get_wiki_content(Guid applicationId, Guid ownerId)
        {
            return DataProvider.GetWikiContent(applicationId, ownerId);
        }

        public static int get_titles_count(Guid applicationId, 
            Guid ownerId, bool? isAdmin, Guid? currentUserId, bool? removed)
        {
            return DataProvider.GetTitlesCount(applicationId, ownerId, isAdmin, currentUserId, removed);
        }

        public static Dictionary<Guid, int> get_paragraphs_count(Guid applicationId, List<Guid> titleIds,
            bool? isAdmin, Guid? currentUserId, bool? removed)
        {
            return DataProvider.GetParagraphsCount(applicationId, titleIds, isAdmin, currentUserId, removed);
        }

        public static int get_paragraphs_count(Guid applicationId, 
            Guid titleId, bool? isAdmin, Guid? currentUserId, bool? removed)
        {
            Dictionary<Guid, int> dic = DataProvider.GetParagraphsCount(applicationId,
                new List<Guid>() { titleId }, isAdmin, currentUserId, removed);
            return dic.ContainsKey(titleId) ? dic[titleId] : 0;
        }

        public static Dictionary<Guid, int> get_changes_count(Guid applicationId, List<Guid> paragraphIds, bool? applied)
        {
            return DataProvider.GetChangesCount(applicationId, paragraphIds, applied);
        }

        public static int get_changes_count(Guid applicationId, Guid paragraphId, bool? applied)
        {
            Dictionary<Guid, int> dic = DataProvider.GetChangesCount(applicationId,
                new List<Guid>() { paragraphId }, applied);
            return dic.ContainsKey(paragraphId) ? dic[paragraphId] : 0;
        }

        public static DateTime? last_modification_date(Guid applicationId, Guid ownerId)
        {
            return DataProvider.LastModificationDate(applicationId, ownerId);
        }

        public static List<KeyValuePair<Guid, int>> wiki_authors(Guid applicationId, Guid ownerId)
        {
            return DataProvider.WikiAuthors(applicationId, ownerId);
        }
    }
}
