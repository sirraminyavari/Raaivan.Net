using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.FormGenerator
{
    public class FGController
    {
        public static bool create_form(Guid applicationId, FormType Info)
        {
            return DataProvider.CreateForm(applicationId, Info);
        }

        public static bool set_form_title(Guid applicationId, FormType Info)
        {
            return DataProvider.SetFormTitle(applicationId, Info);
        }

        public static bool set_form_name(Guid applicationId, Guid formId, string name, 
            Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.SetFormName(applicationId, formId, name, currentUserId, ref errorMessage);
        }

        public static bool set_form_description(Guid applicationId, FormType Info)
        {
            return DataProvider.SetFormDescription(applicationId, Info);
        }

        public static bool remove_form(Guid applicationId, Guid formId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteForm(applicationId, formId, currentUserId);
        }

        public static bool recycle_form(Guid applicationId, Guid formId, Guid currentUserId)
        {
            return DataProvider.RecycleForm(applicationId, formId, currentUserId);
        }

        public static List<FormType> get_forms(Guid applicationId, string searchText = null, 
            int? count = null, int? lowerBoundary = null, bool? hasName = null, bool? archive = null)
        {
            List<FormType> retList = new List<FormType>();
            DataProvider.GetForms(applicationId, ref retList, searchText, count, lowerBoundary, hasName, archive);
            return retList;
        }
        
        public static List<FormType> get_forms(Guid applicationId, ref List<Guid> formIds)
        {
            List<FormType> retList = new List<FormType>();
            DataProvider.GetForms(applicationId, ref retList, ref formIds);
            return retList;
        }

        public static FormType get_form(Guid applicationId, Guid formId)
        {
            List<Guid> _fIds = new List<Guid>();
            _fIds.Add(formId);
            return get_forms(applicationId, ref _fIds).FirstOrDefault();
        }

        public static bool add_form_element(Guid applicationId, FormElement Info, ref string errorMessage)
        {
            return DataProvider.AddFormElement(applicationId, Info, ref errorMessage);
        }

        public static bool modify_form_element(Guid applicationId, FormElement Info, ref string errorMessage)
        {
            return DataProvider.ModifyFormElement(applicationId, Info, ref errorMessage);
        }

        public static bool set_elements_order(Guid applicationId, List<Guid> elementIds)
        {
            return DataProvider.SetElementsOrder(applicationId, elementIds);
        }

        public static bool set_form_element_necessity(Guid applicationId, Guid elementId, bool necessity)
        {
            return DataProvider.SetFormElementNecessity(applicationId, elementId, necessity);
        }

        public static bool set_form_element_uniqueness(Guid applicationId, Guid elementId, bool value)
        {
            return DataProvider.SetFormElementUniqueness(applicationId, elementId, value);
        }

        public static bool remove_form_element(Guid applicationId, Guid elementId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteFormElement(applicationId, elementId, currentUserId);
        }

        public static bool save_form_elements(Guid applicationId, Guid formId,
            string title, string name, string description, List<FormElement> elements, Guid currentUserId)
        {
            return DataProvider.SaveFormElements(applicationId, formId, title, name, description, elements, currentUserId);
        }

        public static List<FormElement> get_form_elements(Guid applicationId, 
            Guid? formId, Guid? ownerId = null, FormElementTypes? type = null)
        {
            List<FormElement> retList = new List<FormElement>();
            DataProvider.GetFormElements(applicationId, ref retList, formId, ownerId, type);
            return retList;
        }

        public static List<FormElement> get_form_elements(Guid applicationId, List<Guid> elementIds)
        {
            List<FormElement> retList = new List<FormElement>();
            DataProvider.GetFormElements(applicationId, ref retList, elementIds);
            return retList;
        }

        public static FormElement get_form_element(Guid applicationId, Guid elementId)
        {
            return get_form_elements(applicationId, new List<Guid>() { elementId }).FirstOrDefault();
        }

        public static Dictionary<string, Guid> get_form_element_ids(Guid applicationId, Guid formId, List<string> names)
        {
            return DataProvider.GetFormElementIDs(applicationId, formId, names);
        }

        public static List<Guid> is_form_element(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.IsFormElement(applicationId, ids);
        }

        public static bool is_form_element(Guid applicationId, Guid id)
        {
            return is_form_element(applicationId, new List<Guid>() { id }).Count > 0;
        }

        public static bool create_form_instances(Guid applicationId, List<FormType> instances, Guid currentUserId)
        {
            return DataProvider.CreateFormInstance(applicationId, instances, currentUserId);
        }

        public static bool create_form_instance(Guid applicationId, FormType Info)
        {
            return Info.Creator.UserID.HasValue &&
                create_form_instances(applicationId, new List<FormType>() { Info }, Info.Creator.UserID.Value);
        }

        public static bool remove_form_instances(Guid applicationId, ref List<Guid> instanceIds, Guid currentUserId)
        {
            return DataProvider.RemoveFormInstances(applicationId, ref instanceIds, currentUserId);
        }

        public static bool remove_form_instance(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            List<Guid> _iIds = new List<Guid>();
            _iIds.Add(instanceId);
            return remove_form_instances(applicationId, ref _iIds, currentUserId);
        }

        public static bool remove_owner_form_instances(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            return DataProvider.RemoveOwnerFormInstances(applicationId, ownerId, formId, currentUserId);
        }

        public static List<FormType> get_owner_form_instances(Guid applicationId, 
            List<Guid> ownerIds, Guid? formId = null, bool? isTemporary = null, Guid? userId = null)
        {
            List<FormType> retList = new List<FormType>();

            PublicMethods.split_list<Guid>(ownerIds, 200, ids =>
            {
                List<FormType> newList = new List<FormType>();

                DataProvider.GetOwnerFormInstances(applicationId, ref newList, ref ids, formId, isTemporary, userId);

                if (newList.Count > 0) retList.AddRange(newList);
            });
            
            return retList;
        }

        public static List<FormType> get_owner_form_instances(Guid applicationId, Guid ownerId, 
            Guid? formId = null, bool? isTemporary = null, Guid? userId = null)
        {
            return get_owner_form_instances(applicationId, new List<Guid>() { ownerId }, formId, isTemporary, userId);
        }

        public static List<FormType> get_form_instances(Guid applicationId, ref List<Guid> instanceIds)
        {
            List<FormType> retList = new List<FormType>();
            DataProvider.GetFormInstances(applicationId, ref retList, ref instanceIds);
            return retList;
        }

        public static FormType get_form_instance(Guid applicationId, Guid instanceId)
        {
            List<Guid> _iIds = new List<Guid>();
            _iIds.Add(instanceId);
            return get_form_instances(applicationId, ref _iIds).FirstOrDefault();
        }

        public static Guid? get_form_instance_owner_id(Guid applicationId, Guid instanceIdOrElementId)
        {
            return DataProvider.GetFormInstanceOwnerID(applicationId, instanceIdOrElementId);
        }

        public static Guid? get_form_instance_hierarchy_owner_id(Guid applicationId, Guid instanceId)
        {
            return DataProvider.GetFormInstanceHierarchyOwnerID(applicationId, instanceId);
        }

        public static bool validate_new_name(Guid applicationId, Guid objectId, Guid? formId, string name)
        {
            return DataProvider.ValidateNewName(applicationId, objectId, formId, name);
        }

        public static bool meets_unique_constraint(Guid applicationId,
            Guid instanceId, Guid elementId, string textValue, double? floatValue)
        {
            return DataProvider.MeetsUniqueConstraint(applicationId, instanceId, elementId, textValue, floatValue);
        }

        public static bool save_form_instance_elements(Guid applicationId, 
            ref List<FormElement> elements, List<Guid> elementsToClear, Guid currentUserId, ref string errorMessage)
        {
            if (elements.Any(u => !string.IsNullOrEmpty(u.Info) && u.Info.Length > 3900))
            {
                errorMessage = Messages.MaxAllowedInputLengthExceeded.ToString();
                return false;
            }

            return DataProvider.SaveFormInstanceElements(applicationId, ref elements, 
                elementsToClear, currentUserId, ref errorMessage);
        }

        public static bool save_form_instance_elements(Guid applicationId,
            ref List<FormElement> elements, List<Guid> elementsToClear, Guid currentUserId)
        {
            string errorMessage = string.Empty;

            return save_form_instance_elements(applicationId, ref elements,
                elementsToClear, currentUserId, ref errorMessage);
        }

        public static bool save_form_instance_element(Guid applicationId, FormElement element, Guid currentUserId)
        {
            List<FormElement> elems = new List<FormElement>();
            elems.Add(element);
            return save_form_instance_elements(applicationId, ref elems, new List<Guid>(), currentUserId);
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId, 
            List<Guid> instanceIds, List<Guid> elementIds, bool? filled = null)
        {
            List<FormElement> retList = new List<FormElement>();

            PublicMethods.split_list<Guid>(instanceIds, 200, ids =>
            {
                List<FormElement> newList = new List<FormElement>();

                DataProvider.GetFormInstanceElements(applicationId, ref newList, ids, ref elementIds, filled);

                if (newList.Count > 0) retList.AddRange(newList);
            });
            
            return retList;
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId,
            Guid instanceId, List<Guid> elementIds, bool? filled = null)
        {
            return get_form_instance_elements(applicationId, new List<Guid>() { instanceId }, elementIds, filled);
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId, 
            List<Guid> instanceIds, bool? filled = null)
        {
            return get_form_instance_elements(applicationId, instanceIds, new List<Guid>(), filled);
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId,
            Guid instanceId, bool? filled = null)
        {
            return get_form_instance_elements(applicationId, new List<Guid>() { instanceId }, new List<Guid>(), filled);
        }

        public static Dictionary<Guid, List<SelectedGuidItem>> get_selected_guids(Guid applicationId, List<Guid> elementIds)
        {
            return DataProvider.GetSelectedGuids(applicationId, elementIds);
        }

        public static List<FormElement> get_element_changes(Guid applicationId, 
            Guid elementId, int? count, int? lowerBoundary)
        {
            List<FormElement> lst = new List<FormGenerator.FormElement>();
            DataProvider.GetElementChanges(applicationId, ref lst, elementId, count, lowerBoundary);
            return lst;
        }

        public static bool set_form_instance_as_filled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return DataProvider.SetFormInstanceAsFilled(applicationId, instanceId, currentUserId);
        }

        public static bool set_form_instance_as_not_filled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return DataProvider.SetFormInstanceAsNotFilled(applicationId, instanceId, currentUserId);
        }

        public static bool is_director(Guid applicationId, Guid instanceId, Guid userId)
        {
            return DataProvider.IsDirector(applicationId, instanceId, userId);
        }

        public static bool set_form_owner(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            return DataProvider.SetFormOwner(applicationId, ownerId, formId, currentUserId);
        }

        public static bool remove_form_owner(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteFormOwner(applicationId, ownerId, formId, currentUserId);
        }

        public static FormType get_owner_form(Guid applicationId, Guid ownerId)
        {
            return DataProvider.GetOwnerForm(applicationId, ownerId);
        }

        public static Guid? initialize_owner_form_instance(Guid applicationId, Guid ownerId, Guid currentUserId)
        {
            return DataProvider.InitializeOwnerFormInstance(applicationId, ownerId, currentUserId);
        }

        public static bool set_element_limits(Guid applicationId, 
            Guid ownerId, ref List<Guid> elementIds, Guid currentUserId)
        {
            return DataProvider.SetElementLimits(applicationId, ownerId, ref elementIds, currentUserId);
        }

        public static List<FormElement> get_element_limits(Guid applicationId, Guid ownerId)
        {
            List<FormElement> items = new List<FormElement>();
            DataProvider.GetElementLimits(applicationId, ref items, ownerId);
            return items;
        }

        public static bool set_element_limit_necessity(Guid applicationId, 
            Guid ownerId, Guid elementId, bool necessary, Guid currentUserId)
        {
            return DataProvider.SetElementLimitNecessity(applicationId, ownerId, elementId, necessary, currentUserId);
        }

        public static bool remove_element_limit(Guid applicationId, Guid ownerId, Guid elementId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteElementLimit(applicationId, ownerId, elementId, currentUserId);
        }

        public static List<Guid> get_common_form_instance_ids(Guid applicationId, 
            Guid ownerId, Guid filledOwnerId, bool hasLimit)
        {
            List<Guid> ids = new List<Guid>();
            DataProvider.GetCommonFormInstanceIDs(applicationId, ref ids, ownerId, filledOwnerId, hasLimit);
            return ids;
        }

        public static List<FormRecord> get_form_records(Guid applicationId, Guid formId,
            List<Guid> elementIds, List<Guid> instanceIds, List<Guid> ownerIds, List<FormFilter> filters,
            int? lowerBoundary, int? count, Guid? sortByElementId, bool? descending)
        {
            List<FormRecord> retList = new List<FormRecord>();
            DataProvider.GetFormRecords(applicationId, ref retList, formId, elementIds, instanceIds, ownerIds,
                filters, lowerBoundary, count, sortByElementId, descending);
            return retList;
        }

        public static List<FormRecord> get_form_records(Guid applicationId, Guid formId, List<FormFilter> filters, 
            int? lowerBoundary = null, int? count = null, Guid? sortByElementId = null, bool? descending = false)
        {
            return get_form_records(applicationId, formId, new List<Guid>(), new List<Guid>(), new List<Guid>(),
                filters, lowerBoundary, count, sortByElementId, descending);
        }

        public static List<FormRecord> get_form_records(Guid applicationId, Guid formId,
            int? lowerBoundary = null, int? count = 20, Guid? sortByElementId = null, bool? descending = false)
        {
            return get_form_records(applicationId, formId, new List<Guid>(), new List<Guid>(), new List<Guid>(),
                new List<FormFilter>(), lowerBoundary, count, sortByElementId, descending);
        }

        public static void get_form_statistics(Guid applicationId, Guid? ownerId, Guid? instanceId,
            ref double weightSum, ref double sum, ref double weightedSum, ref double avg, ref double weightedAvg,
            ref double min, ref double max, ref double var, ref double stDev)
        {
            DataProvider.GetFormStatistics(applicationId, ownerId, instanceId, ref weightSum, ref sum,
                ref weightedSum, ref avg, ref weightedAvg, ref min, ref max, ref var, ref stDev);
        }

        public static bool convert_form_to_table(Guid applicationId, Guid formId)
        {
            return DataProvider.ConvertFormToTable(applicationId, formId);
        }

        //Polls

        public static List<Poll> get_polls(Guid applicationId, Guid? isCopyOfPollId, Guid? ownerId,
            bool? archive, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Poll> lst = new List<FormGenerator.Poll>();
            DataProvider.GetPolls(applicationId, ref lst, 
                isCopyOfPollId, ownerId, archive, searchText, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Poll> get_polls(Guid applicationId, List<Guid> pollIds)
        {
            List<Poll> lst = new List<FormGenerator.Poll>();
            DataProvider.GetPolls(applicationId, ref lst, pollIds);
            return lst;
        }

        public static Poll get_poll(Guid applicationId, Guid pollId)
        {
            return get_polls(applicationId, new List<Guid>() { pollId }).FirstOrDefault();
        }

        public static bool add_poll(Guid applicationId, 
            Guid pollId, Guid? copyFromPollId, Guid? ownerId, string name, Guid currentUserId)
        {
            return DataProvider.AddPoll(applicationId, pollId, copyFromPollId, ownerId, name, currentUserId);
        }

        public static Guid? get_poll_instance(Guid applicationId,
            Guid? pollId, Guid copyFromPollId, Guid? ownerId, Guid currentUserId)
        {
            return DataProvider.GetPollInstance(applicationId, pollId, copyFromPollId, ownerId, currentUserId);
        }

        public static List<Guid> get_owner_poll_ids(Guid applicationId, Guid isCopyOfPollId, Guid ownerId)
        {
            List<Guid> lst = new List<Guid>();
            DataProvider.GetOwnerPollIDs(applicationId, ref lst, isCopyOfPollId, ownerId);
            return lst;
        }

        public static bool rename_poll(Guid applicationId, Guid pollId, string name, Guid currentUserId)
        {
            return DataProvider.RenamePoll(applicationId, pollId, name, currentUserId);
        }

        public static bool set_poll_description(Guid applicationId, Guid pollId, string description, Guid currentUserId)
        {
            return DataProvider.SetPollDescription(applicationId, pollId, description, currentUserId);
        }

        public static bool set_poll_begin_date(Guid applicationId, Guid pollId, DateTime? beginDate, Guid currentUserId)
        {
            return DataProvider.SetPollBeginDate(applicationId, pollId, beginDate, currentUserId);
        }

        public static bool set_poll_finish_date(Guid applicationId, Guid pollId, DateTime? finishDate, Guid currentUserId)
        {
            return DataProvider.SetPollFinishDate(applicationId, pollId, finishDate, currentUserId);
        }

        public static bool set_poll_show_summary(Guid applicationId, Guid pollId, bool showSummary, Guid currentUserId)
        {
            return DataProvider.SetPollShowSummary(applicationId, pollId, showSummary, currentUserId);
        }

        public static bool set_poll_hide_contributors(Guid applicationId, 
            Guid pollId, bool hideContributors, Guid currentUserId)
        {
            return DataProvider.SetPollHideContributors(applicationId, pollId, hideContributors, currentUserId);
        }

        public static bool remove_poll(Guid applicationId, Guid pollId, Guid currentUserId)
        {
            return DataProvider.RemovePoll(applicationId, pollId, currentUserId);
        }

        public static bool recycle_poll(Guid applicationId, Guid pollId, Guid currentUserId)
        {
            return DataProvider.RecyclePoll(applicationId, pollId, currentUserId);
        }

        public static void get_poll_status(Guid applicationId, Guid? pollId, Guid? isCopyOfPollId, 
            Guid currentUserId, ref string description, ref DateTime? beginDate, ref DateTime? finishDate, 
            ref Guid? instanceId, ref int? elementsCount, ref int? filledElementsCount, ref int? allFilledFormsCount)
        {
            DataProvider.GetPollStatus(applicationId, pollId, isCopyOfPollId, currentUserId, 
                ref description, ref beginDate, ref finishDate, ref instanceId, 
                ref elementsCount, ref filledElementsCount, ref allFilledFormsCount);
        }

        public static Dictionary<Guid, int> get_poll_elements_instance_count(Guid applicationId, Guid pollId)
        {
            return DataProvider.GetPollElementsInstanceCount(applicationId, pollId);
        }

        public static List<PollAbstract> get_poll_abstract_text(Guid applicationId,
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            List<PollAbstract> lst = new List<FormGenerator.PollAbstract>();
            DataProvider.GetPollAbstractText(applicationId, ref lst, pollId, elementIds, count, lowerBoundary);
            return lst;
        }

        public static List<PollAbstract> get_poll_abstract_guid(Guid applicationId, 
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            List<PollAbstract> lst = new List<FormGenerator.PollAbstract>();
            DataProvider.GetPollAbstractGuid(applicationId, ref lst, pollId, elementIds, count, lowerBoundary);
            return lst;
        }

        public static List<PollAbstract> get_poll_abstract_bool(Guid applicationId, Guid pollId, List<Guid> elementIds)
        {
            List<PollAbstract> lst = new List<FormGenerator.PollAbstract>();
            DataProvider.GetPollAbstractBool(applicationId, ref lst, pollId, elementIds);
            return lst;
        }

        public static List<PollAbstract> get_poll_abstract_number(Guid applicationId, 
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            List<PollAbstract> lst = new List<FormGenerator.PollAbstract>();
            DataProvider.GetPollAbstractNumber(applicationId, ref lst, pollId, elementIds, count, lowerBoundary);
            return lst;
        }

        public static List<FormElement> get_poll_element_instances(Guid applicationId, 
            Guid pollId, Guid elementId, int? count, int? lowerBoundary)
        {
            List<FormElement> lst = new List<FormGenerator.FormElement>();
            DataProvider.GetPollElementInstances(applicationId, ref lst, pollId, elementId, count, lowerBoundary);
            return lst;
        }

        public static void get_current_polls_count(Guid applicationId,
            Guid? currentUserId, ref int count, ref int doneCount)
        {
            DataProvider.GetCurrentPollsCount(applicationId, currentUserId, ref count, ref doneCount);
        }

        public static Dictionary<Guid, bool> get_current_polls(Guid applicationId, 
            Guid? currentUserId, int? count, int? lowerBoundary, ref long totalCount)
        {
            return DataProvider.GetCurrentPolls(applicationId, currentUserId, count, lowerBoundary, ref totalCount);
        }

        public static List<Guid> is_poll(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.IsPoll(applicationId, ids);
        }

        public static bool is_poll(Guid applicationId, Guid id)
        {
            return is_poll(applicationId, new List<Guid>() { id }).Count > 0;
        }

        //end of Polls
    }
}
    