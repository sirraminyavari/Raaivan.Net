﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using System.Text.RegularExpressions;

namespace RaaiVan.Modules.FormGenerator
{
    public enum FormElementTypes
    {
        Binary,
        Checkbox,
        Date,
        File,
        Form,
        MultiLevel,
        Node,
        Numeric,
        Select,
        Separator,
        Text,
        User
    }

    public enum PollOwnerType {
        None,
        WorkFlow
    }

    public static class FGUtilities
    {
        public static List<FormElement> get_form_elements(string strElements)
        {
            try
            {
                List<FormElement> retList = new List<FormElement>();

                if (string.IsNullOrEmpty(strElements)) return retList;

                Dictionary<string, object> dic = PublicMethods.fromJSON(Base64.decode(strElements));

                if (!dic.ContainsKey("Elements") || dic["Elements"].GetType() != typeof(ArrayList)) return retList;

                foreach (Dictionary<string, object> elem in (ArrayList)dic["Elements"])
                {
                    FormElement newElement = new FormElement()
                    {
                        ElementID = !elem.ContainsKey("ElementID") ? null :
                            PublicMethods.parse_guid((string)elem["ElementID"]),
                        FormInstanceID = !elem.ContainsKey("InstanceID") ? null :
                            PublicMethods.parse_guid((string)elem["InstanceID"]),
                        Title = !elem.ContainsKey("Title") ? null : Base64.decode((string)elem["Title"]),
                        Filled = elem.ContainsKey("Filled") && 
                            PublicMethods.parse_bool(elem["Filled"].ToString(), false).Value,
                        SequenceNumber = -1,
                        Info = !elem.ContainsKey("Info") ? null : Base64.decode((string)elem["Info"])
                    };

                    if (elem.ContainsKey("RefElementID") && !string.IsNullOrEmpty(elem["RefElementID"].ToString()))
                        newElement.RefElementID = PublicMethods.parse_guid((string)elem["RefElementID"]);
                    if (elem.ContainsKey("SequenceNumber") && !string.IsNullOrEmpty(elem["SequenceNumber"].ToString()))
                        newElement.SequenceNumber = PublicMethods.parse_int(elem["SequenceNumber"].ToString(), -1);
                    if (elem.ContainsKey("Name") && !string.IsNullOrEmpty(elem["Name"].ToString()))
                        newElement.Name = elem["Name"].ToString();

                    string textValue = !elem.ContainsKey("TextValue") ? null : Base64.decode((string)elem["TextValue"]);
                    if (!string.IsNullOrEmpty(textValue)) newElement.TextValue = textValue;
                    if (elem.ContainsKey("FloatValue") && !string.IsNullOrEmpty(elem["FloatValue"].ToString()))
                        newElement.FloatValue = PublicMethods.parse_double(elem["FloatValue"].ToString());
                    if (elem.ContainsKey("BitValue") && !string.IsNullOrEmpty(elem["BitValue"].ToString()))
                        newElement.BitValue = PublicMethods.parse_bool(elem["BitValue"].ToString());
                    if (elem.ContainsKey("DateValue"))
                        newElement.DateValue = PublicMethods.parse_date((string)elem["DateValue"]);
                    if (elem.ContainsKey("Files") && !string.IsNullOrEmpty(elem["Files"].ToString()))
                        newElement.AttachedFiles = DocumentUtilities.get_files_info((string)elem["Files"]);

                    if (elem.ContainsKey("Type"))
                    {
                        try { newElement.Type = (FormElementTypes)Enum.Parse(typeof(FormElementTypes), (string)elem["Type"]); }
                        catch { newElement.Type = null; }
                    }

                    if (elem.ContainsKey("GuidItems") && elem["GuidItems"].GetType() == typeof(ArrayList))
                    {
                        SelectedGuidItemType tp = SelectedGuidItemType.None;
                        if (newElement.Type == FormElementTypes.Node) tp = SelectedGuidItemType.Node;
                        else if (newElement.Type == FormElementTypes.User) tp = SelectedGuidItemType.User;

                        foreach (Dictionary<string, object> d in (ArrayList)elem["GuidItems"])
                            newElement.GuidItems.Add(new SelectedGuidItem(Guid.Parse((string)d["ID"]), code: string.Empty,
                                name: Base64.decode((string)d["Name"]), type: tp));
                    }

                    if (newElement.ElementID.HasValue && (!newElement.Filled.HasValue || !newElement.Filled.Value))
                    {
                        newElement.RefElementID = newElement.ElementID;
                        newElement.ElementID = Guid.NewGuid();
                    }

                    if (newElement.AttachedFiles != null)
                    {
                        for (var i = 0; i < newElement.AttachedFiles.Count; ++i)
                        {
                            newElement.AttachedFiles[i].OwnerID = newElement.ElementID;
                            newElement.AttachedFiles[i].OwnerType = FileOwnerTypes.FormElement;
                        }
                    }

                    retList.Add(newElement);
                }

                return retList;
            }
            catch { return new List<FormElement>(); }
        }

        public static bool set_element_value(string value, ref FormElement element)
        {
            if (string.IsNullOrEmpty(value)) value = string.Empty;
            FormElementTypes type = element.Type.HasValue ? element.Type.Value : FormElementTypes.Text;

            switch (type)
            {
                case FormElementTypes.Text:
                case FormElementTypes.Select:
                case FormElementTypes.Checkbox:
                    element.TextValue = value;
                    return !string.IsNullOrEmpty(element.TextValue);
                case FormElementTypes.Binary:
                    element.BitValue = value.ToLower() == "true";
                    return true;
                case FormElementTypes.Date:
                    if (value.ToLower() == "now")
                        element.DateValue = DateTime.Now;
                    else
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(value, out dt)) element.DateValue = dt;
                    }
                    return element.DateValue.HasValue;
                case FormElementTypes.Node:
                case FormElementTypes.User:
                    SelectedGuidItemType tp = type == FormElementTypes.Node ? SelectedGuidItemType.Node : SelectedGuidItemType.User;
                    Guid id = Guid.Empty;
                    if (Guid.TryParse(value, out id) && id != Guid.Empty)
                        element.GuidItems.Add(new SelectedGuidItem(id, string.Empty, string.Empty, tp));
                    return id != Guid.Empty;
                case FormElementTypes.Numeric:
                    float flt = 0;
                    if (float.TryParse(value, out flt)) element.FloatValue = flt;
                    return element.FloatValue.HasValue;
            }

            return false;
        }

        public static List<FormFilter> get_filters_from_json(Dictionary<string, object> dic,
            Guid? ownerId = null, Guid? formId = null, Guid? applicationId = null)
        {
            List<FormFilter> retFilters = new List<FormFilter>();

            Dictionary<string, Guid> namesDic = new Dictionary<string, Guid>();

            if (formId.HasValue && applicationId.HasValue)
            {
                Guid tempId = Guid.Empty;
                List<String> fieldNames = dic.Keys.Where(i => !Guid.TryParse(i, out tempId)).ToList();
                namesDic = FGController.get_form_element_ids(applicationId.Value, formId.Value, fieldNames);
            }
            
            foreach (string id in dic.Keys)
            {
                try
                {
                    Guid elementId = namesDic.ContainsKey(id.ToLower()) ? namesDic[id.ToLower()] : Guid.Empty;

                    if ((elementId == Guid.Empty && !Guid.TryParse(id, out elementId)) ||
                        (dic[id] != null && dic[id].GetType() != typeof(Dictionary<string, object>))) continue;

                    Dictionary<string, object> elemDic = (Dictionary<string, object>)dic[id];

                    FormElementTypes type = FormElementTypes.Text;
                    //if (!Enum.TryParse<FormElementTypes>(elemDic.ContainsKey("Type") ?
                    //    elemDic["Type"].ToString() : string.Empty, out type)) continue;
                    if (!Enum.TryParse<FormElementTypes>(PublicMethods.get_dic_value(elemDic, "Type", string.Empty), out type))
                        type = FormElementTypes.Text;
                    
                    if (type == FormElementTypes.Form)
                    {
                        List<FormFilter> subFilters = get_filters_from_json(elemDic, elementId);

                        if (subFilters != null && subFilters.Count > 0)
                        {
                            retFilters.Add(new FormFilter() { ElementID = elementId, OwnerID = ownerId }); //just to say that this field has filters
                            retFilters.AddRange(subFilters);
                        }

                        continue;
                    }

                    FormFilter filter = new FormFilter() { ElementID = elementId, OwnerID = ownerId };

                    filter.Exact = PublicMethods.parse_bool(PublicMethods.get_dic_value(elemDic, "Exact"));
                    filter.Or = PublicMethods.parse_bool(PublicMethods.get_dic_value(elemDic, "Or"));
                    filter.Bit = PublicMethods.parse_bool(PublicMethods.get_dic_value(elemDic, "Bit"));
                    filter.FloatFrom = PublicMethods.parse_double(PublicMethods.get_dic_value(elemDic, "FloatFrom"));
                    filter.FloatTo = PublicMethods.parse_double(PublicMethods.get_dic_value(elemDic, "FloatTo"));
                    filter.DateFrom = PublicMethods.parse_date(PublicMethods.get_dic_value(elemDic, "DateFrom"));
                    filter.DateTo = PublicMethods.parse_date(PublicMethods.get_dic_value(elemDic, "DateTo"), 1);
                    filter.Compulsory = PublicMethods.parse_bool(PublicMethods.get_dic_value(elemDic, "Compulsory"));

                    if (elemDic.ContainsKey("TextItems") && elemDic["TextItems"] != null && 
                        elemDic["TextItems"].GetType() == typeof(ArrayList))
                    {
                        ArrayList lst = (ArrayList)elemDic["TextItems"];
                        foreach (object o in lst)
                            if (o != null) filter.TextItems.Add(Base64.decode(o.ToString()));
                    }

                    if (elemDic.ContainsKey("GuidItems") && elemDic["GuidItems"] != null && 
                        elemDic["GuidItems"].GetType() == typeof(ArrayList))
                    {
                        ArrayList lst = (ArrayList)elemDic["GuidItems"];
                        foreach (object o in lst)
                        {
                            Guid val = Guid.Empty;
                            if (Guid.TryParse(o.ToString(), out val)) filter.GuidItems.Add(val);
                        }
                    }

                    retFilters.Add(filter);
                }
                catch { }
            }

            return retFilters;
        }

        public static List<FormFilter> get_filters_from_json(string jsonInput,
            Guid? ownerId = null, Guid? formId = null, Guid? applicationId = null) {
            return get_filters_from_json(PublicMethods.fromJSON(jsonInput), ownerId, formId, applicationId);
        }

        public static bool is_valid_name(string formOrElementName) {
            try
            {
                return string.IsNullOrEmpty(formOrElementName) || 
                    new Regex("^[a-zA-Z0-9_]{1,100}$").IsMatch(formOrElementName);
            }
            catch { return false; }
        }
    }

    public enum SelectedGuidItemType
    {
        None,
        Node,
        User
    }

    public class SelectedGuidItem
    {
        public Guid? ID;
        public string Code;
        public string Name;
        public SelectedGuidItemType Type;

        public SelectedGuidItem()
        {
            Type = SelectedGuidItemType.None;
        }

        public SelectedGuidItem(Guid id, string code, string name, SelectedGuidItemType type)
        {
            ID = id;
            Code = code;
            Name = name;
            Type = type;
        }

        public string toJson()
        {
            return "{\"ID\":\"" + (ID.HasValue ? ID.ToString() : string.Empty) + "\"" +
                ",\"Code\":\"" + Base64.encode(Code) + "\"" +
                ",\"Name\":\"" + Base64.encode(Name) + "\"" +
                (Type == SelectedGuidItemType.Node ? ",\"NodeID\":\"" + (ID.HasValue ? ID.ToString() : string.Empty) + "\"" : string.Empty) +
                (Type == SelectedGuidItemType.Node ? ",\"AdditionalID\":\"" + Base64.encode(Code) + "\"" : string.Empty) +
                (Type == SelectedGuidItemType.User ? ",\"UserID\":\"" + (ID.HasValue ? ID.ToString() : string.Empty) + "\"" : string.Empty) +
                (Type == SelectedGuidItemType.User ? ",\"UserName\":\"" + Base64.encode(Code) + "\"" : string.Empty) +
                (Type == SelectedGuidItemType.User ? ",\"FullName\":\"" + Base64.encode(Name) + "\"" : string.Empty) +
                "}";
        }
    }

    public class FormElement : ICloneable
    {
        public Guid? ElementID;
        public Guid? FormID;
        public Guid? FormInstanceID;
        public Guid? RefElementID;
        public long? ChangeID;
        public string Title;
        public string Name;
        public string Help;
        public bool? Necessary;
        public bool? UniqueValue;
        public int? SequenceNumber;
        public FormElementTypes? Type;
        public string Info;
        public double? Weight;
        public string TextValue;
        public double? FloatValue;
        public bool? BitValue;
        public DateTime? DateValue;
        public List<SelectedGuidItem> GuidItems;
        public bool? Filled;
        public User Creator;
        public DateTime? CreationDate;
        public Guid? LastModifierUserID;
        public DateTime? LastModificationDate;
        public List<DocFileInfo> AttachedFiles;
        public List<FormType> TableContent;
        public int? EditionsCount;
        
        public FormElement()
        {
            GuidItems = new List<SelectedGuidItem>();
            Creator = new User();
            AttachedFiles = new List<DocFileInfo>();
            TableContent = new List<FormGenerator.FormType>();
        }

        public object Clone() {
            return this.MemberwiseClone();
        }

        public Guid? GetRefElementID() {
            return RefElementID.HasValue ? RefElementID : ElementID;
        }

        public string toString(Guid applicationId)
        {
            string ret = string.Empty;

            switch (Type) {
                case FormElementTypes.Text:
                case FormElementTypes.Select:
                case FormElementTypes.Checkbox:
                case FormElementTypes.MultiLevel:
                case FormElementTypes.Node:
                case FormElementTypes.User:
                case FormElementTypes.Binary:
                    ret = TextValue;
                    break;
                case FormElementTypes.Date:
                    ret = PublicMethods.get_local_date(DateValue);
                    break;
                case FormElementTypes.Numeric:
                    ret = FloatValue.ToString();
                    break;
                case FormElementTypes.File:
                case FormElementTypes.Form:
                case FormElementTypes.Separator:
                    break;
            }

            return PublicMethods.markup2plaintext(applicationId, ret);
        }

        public string toJson(Guid applicationId, bool persianDate = false, bool includeRefElementId = false)
        {

            string strElementId = string.Empty;

            if (includeRefElementId)
                strElementId = ElementID.HasValue ? ElementID.Value.ToString() : string.Empty;
            else
                strElementId = (RefElementID.HasValue ? RefElementID : ElementID).ToString();

            return "{\"ElementID\":\"" + strElementId + "\"" +
                ",\"InstanceID\":\"" + (FormInstanceID.HasValue ? FormInstanceID.ToString() : string.Empty) + "\"" +
                (!includeRefElementId ? string.Empty :
                    ",\"RefElementID\":\"" + (RefElementID.HasValue ? RefElementID.ToString() : string.Empty) + "\""
                ) +
                ",\"FormID\":\"" + (FormID.HasValue ? FormID.ToString() : string.Empty) + "\"" +
                (!ChangeID.HasValue ? string.Empty : ",\"ChangeID\":\"" + ChangeID.Value.ToString() + "\"") +
                ",\"Title\":\"" + Base64.encode(Title) + "\"" +
                ",\"Name\":\"" + Base64.encode(Name) + "\"" +
                ",\"Help\":\"" + Base64.encode(Help) + "\"" +
                ",\"Type\":\"" + (Type.HasValue ? Type.ToString() : FormElementTypes.Text.ToString()) + "\"" +
                ",\"Necessary\":" + (Necessary.HasValue && Necessary.Value).ToString().ToLower() +
                ",\"UniqueValue\":" + (UniqueValue.HasValue && UniqueValue.Value).ToString().ToLower() +
                ",\"SequenceNumber\":" + (SequenceNumber.HasValue ? SequenceNumber.Value : 0).ToString() +
                ",\"Info\":\"" + Base64.encode(Info) + "\"" +
                (!Weight.HasValue ? string.Empty : ",\"Weight\":" + Weight.ToString()) +
                ",\"TextValue\":\"" + Base64.encode(TextValue) + "\"" +
                ",\"DateValue\":" + (!DateValue.HasValue ? "null" : "\"" + (persianDate ?
                    PublicMethods.get_local_date(DateValue.Value) : DateValue.ToString()) + "\"") +
                ",\"DateValue_Jalali\":" + (!DateValue.HasValue ? "null" : "\"" +
                    PublicMethods.get_local_date(DateValue.Value) + "\"") +
                ",\"FloatValue\":" + (!FloatValue.HasValue ? "null" : FloatValue.Value.ToString()) +
                ",\"BitValue\":" + (!BitValue.HasValue ? "null" : BitValue.Value.ToString().ToLower()) +
                ",\"GuidItems\":[" + string.Join(",", GuidItems.Select(u => u.toJson())) + "]" +
                ",\"SelectedItems\":[" + string.Join(",", GuidItems.Select(u => u.toJson())) + "]" +
                ",\"Filled\":" + (Filled.HasValue && Filled.Value).ToString().ToLower() +
                (!Creator.UserID.HasValue || string.IsNullOrEmpty(Creator.UserName) ? string.Empty :
                    ",\"Creator\":{\"UserID\":\"" + Creator.UserID.ToString() + "\"" +
                        ",\"UserName\":\"" + Base64.encode(Creator.UserName) + "\"" +
                        ",\"FirstName\":\"" + Base64.encode(Creator.FirstName) + "\"" +
                        ",\"LastName\":\"" + Base64.encode(Creator.LastName) + "\"" +
                        ",\"ProfileImageURL\":\"" + DocumentUtilities.get_personal_image_address(applicationId,
                            Creator.UserID.Value) + "\"" +
                    "}"
                ) +
                ",\"CreationDate\":\"" + (CreationDate.HasValue ?
                    PublicMethods.get_local_date(CreationDate.Value, true) : string.Empty) + "\"" +
                ",\"LastModificationDate\":\"" + (LastModificationDate.HasValue ?
                    PublicMethods.get_local_date(LastModificationDate.Value, true) : string.Empty) + "\"" +
                (AttachedFiles == null || AttachedFiles.Count == 0 ? string.Empty :
                    ",\"Files\":" + DocumentUtilities.get_files_json(applicationId, AttachedFiles, true)
                ) +
                ",\"EditionsCount\":" + (!EditionsCount.HasValue ? 0 : EditionsCount.Value).ToString() +
                "}";
        }
    }
    
    public class FormType : ICloneable
    {
        public Guid? FormID;
        public Guid? InstanceID;
        public Guid? OwnerID;
        public Guid? DirectorID;
        public bool? Admin;
        public string Title;
        public string Name;
        public string Description;
        public User Creator;
        public bool? Filled;
        public bool?IsTemporary;
        public DateTime? FillingDate;
        public DateTime? CreationDate;
        public Guid? LastModifierUserID;
        public DateTime? LastModificationDate;
        public List<FormElement> Elements;

        public FormType()
        {
            Elements = new List<FormElement>();
            Creator = new User();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string toJson() {
            return "{\"InstanceID\":\"" + (!InstanceID.HasValue ? string.Empty : InstanceID.Value.ToString()) + "\"" +
                ",\"FormID\":\"" + (!FormID.HasValue ? string.Empty : FormID.Value.ToString()) + "\"" +
                ",\"OwnerID\":\"" + (OwnerID.HasValue ? OwnerID.Value.ToString() : string.Empty) + "\"" +
                ",\"DirectorID\":\"" + (DirectorID.HasValue ? DirectorID.Value.ToString() : string.Empty) + "\"" +
                ",\"IsTemporary\":" + (IsTemporary.HasValue && IsTemporary.Value).ToString().ToLower() +
                ",\"CreationDate\":\"" + (CreationDate.HasValue ? CreationDate.Value.ToString() : string.Empty) + "\"" +
                ",\"CreationDate_Jalali\":\"" + (!CreationDate.HasValue ? string.Empty :
                    PublicMethods.get_local_date(CreationDate.Value)) + "\"" +
                "}";
        }
    }

    public class FormFilter
    {
        public Guid? OwnerID;
        public Guid? ElementID;
        public string Text;
        public List<string> TextItems;
        public bool? Or;
        public bool? Exact;
        public DateTime? DateFrom;
        public DateTime? DateTo;
        public double? FloatFrom;
        public double? FloatTo;
        public bool? Bit;
        public Guid? Guid;
        public List<Guid> GuidItems;
        public bool? Compulsory;

        public FormFilter()
        {
            TextItems = new List<string>();
            GuidItems = new List<System.Guid>();
        }
    }

    public class RecordCell
    {
        public Guid? ElementID;
        public string Value;

        public RecordCell(Guid elementId, string value)
        {
            ElementID = elementId;
            Value = value;
        }
    }

    public class FormRecord
    {
        public Guid? InstanceID;
        public Guid? OwnerID;
        public DateTime? CreationDate;
        public List<RecordCell> Cells;

        public FormRecord()
        {
            Cells = new List<RecordCell>();
        }

        public string toJSON() {
            return "{\"InstanceID\":\"" + (InstanceID.HasValue ? InstanceID.Value.ToString() : string.Empty) + "\"" +
                ",\"OwnerID\":\"" + (OwnerID.HasValue ? OwnerID.Value.ToString() : string.Empty) + "\"" +
                ",\"CreationDate\":\"" + (CreationDate.HasValue ? CreationDate.Value.ToString() : string.Empty) + "\"" +
                ",\"CreationDate_Jalali\":\"" + (CreationDate.HasValue ? 
                    PublicMethods.get_local_date(CreationDate.Value) : string.Empty) + "\"" +
                "," + string.Join(",", Cells.Where(x => x.ElementID.HasValue)
                    .Select(u => "\"" + u.ElementID.ToString() + "\":\"" + Base64.encode(u.Value) + "\"")) + "}";
        }
    }

    public class Poll
    {
        public Guid? PollID;
        public Guid? IsCopyOfPollID;
        public Guid? OwnerID;
        public string Name;
        public string RefName;
        public string Description;
        public string RefDescription;
        public DateTime? BeginDate;
        public DateTime? FinishDate;
        public bool? ShowSummary;
        public bool? HideContributors;

        public string toJson(bool? done = null)
        {
            return "{\"PollID\":\"" + (!PollID.HasValue ? string.Empty : PollID.ToString()) + "\"" +
                (!IsCopyOfPollID.HasValue ? string.Empty :
                    ",\"IsCopyOfPollID\":\"" + IsCopyOfPollID.ToString() + "\"") +
                (!OwnerID.HasValue ? string.Empty : ",\"OwnerID\":\"" + OwnerID.ToString() + "\"") +
                ",\"Name\":\"" + Base64.encode(Name) + "\"" +
                ",\"RefName\":\"" + Base64.encode(RefName) + "\"" +
                ",\"Description\":\"" + Base64.encode(Description) + "\"" +
                ",\"RefDescription\":\"" + Base64.encode(RefDescription) + "\"" +
                ",\"BeginDate\":\"" + (!BeginDate.HasValue ? string.Empty :
                    PublicMethods.get_local_date(BeginDate.Value)) + "\"" +
                ",\"FinishDate\":\"" + (!FinishDate.HasValue ? string.Empty :
                    PublicMethods.get_local_date(FinishDate.Value)) + "\"" +
                ",\"ShowSummary\":" + (ShowSummary.HasValue && ShowSummary.Value).ToString().ToLower() +
                (!done.HasValue ? string.Empty : ",\"Done\":" + done.Value.ToString().ToLower()) +
                ",\"HideContributors\":" + (HideContributors.HasValue && HideContributors.Value).ToString().ToLower() +
                "}";
        }
    }

    public class PollAbstractValue
    {
        public string TextValue;
        public double? NumberValue;
        public bool? BitValue;
        public int? Count;

        public bool hasValue()
        {
            return !string.IsNullOrEmpty(TextValue) || NumberValue.HasValue || BitValue.HasValue;
        }

        public string toString()
        {
            if (!string.IsNullOrEmpty(TextValue)) return TextValue;
            else if (NumberValue.HasValue) return NumberValue.Value.ToString();
            else if (BitValue.HasValue) return BitValue.Value.ToString().ToLower();
            else return string.Empty;
        }

        public string toJSONString()
        {
            if (!string.IsNullOrEmpty(TextValue)) return "\"" + Base64.encode(TextValue) + "\"";
            else if (NumberValue.HasValue) return NumberValue.Value.ToString();
            else if (BitValue.HasValue) return BitValue.Value.ToString().ToLower();
            else return "\"\"";
        }
    }

    public class PollAbstract
    {
        public int? TotalCount;
        public Guid? ElementID;
        public List<PollAbstractValue> Values;
        public double? Min;
        public double? Max;
        public double? Avg;
        public double? Var;
        public double? StDev;

        public PollAbstract() {
            Values = new List<PollAbstractValue>();
        }
    }
}
