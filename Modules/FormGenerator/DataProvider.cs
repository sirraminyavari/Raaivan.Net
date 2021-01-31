using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.FormGenerator
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[FG_" + name + "]"; //'[dbo].' is database owner and 'FG_' is module qualifier
        }

        private static void _parse_form_types(ref IDataReader reader, ref List<FormType> lstForms)
        {
            while (reader.Read())
            {
                try
                {
                    FormType formtype = new FormType();

                    if (!string.IsNullOrEmpty(reader["FormID"].ToString())) formtype.FormID = (Guid)reader["FormID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) formtype.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) formtype.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) formtype.Description = (string)reader["Description"];

                    lstForms.Add(formtype);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_form_elements(ref IDataReader reader, ref List<FormElement> lstElements)
        {
            while (reader.Read())
            {
                try
                {
                    FormElement formElement = new FormElement();

                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) formElement.ElementID = (Guid)reader["ElementID"];
                    if (!string.IsNullOrEmpty(reader["FormID"].ToString())) formElement.FormID = (Guid)reader["FormID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) formElement.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) formElement.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Help"].ToString())) formElement.Help = (string)reader["Help"];
                    if (!string.IsNullOrEmpty(reader["Necessary"].ToString())) formElement.Necessary = (bool)reader["Necessary"];
                    if (!string.IsNullOrEmpty(reader["UniqueValue"].ToString())) formElement.UniqueValue = (bool)reader["UniqueValue"];
                    if (!string.IsNullOrEmpty(reader["SequenceNumber"].ToString())) formElement.SequenceNumber = (int)reader["SequenceNumber"];

                    string strType = string.Empty;
                    if (!string.IsNullOrEmpty(reader["Type"].ToString())) strType = (string)reader["Type"];
                    try { formElement.Type = (FormElementTypes)Enum.Parse(typeof(FormElementTypes), strType); }
                    catch { formElement.Type = null; }

                    if (!string.IsNullOrEmpty(reader["Info"].ToString())) formElement.Info = (string)reader["Info"];
                    if (!string.IsNullOrEmpty(reader["Weight"].ToString())) formElement.Weight = (double)reader["Weight"];

                    if (!string.IsNullOrEmpty(formElement.Info) && formElement.Info[0] != '{')
                    {
                        formElement.Info = "{\"Options\":[" + ProviderUtil.list_to_string<string>(
                            ProviderUtil.get_tags_list(formElement.Info).Select(u => "\"" + Base64.encode(u) + "\"").ToList()) + "]}";
                    }

                    lstElements.Add(formElement);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static Dictionary<string, Guid> _parse_element_ids(ref IDataReader reader)
        {
            Dictionary<string, Guid> dic = new Dictionary<string, Guid>();

            while (reader.Read())
            {
                try
                {
                    string name = string.Empty;
                    Guid? elementId = null;

                    if (!string.IsNullOrEmpty(reader["Name"].ToString()) && 
                        !string.IsNullOrEmpty(reader["ElementID"].ToString())) {
                        name = (string)reader["Name"];
                        elementId = (Guid)reader["ElementID"];

                        if (!string.IsNullOrEmpty(name) && elementId.HasValue) dic[name.ToLower()] = elementId.Value;
                    }
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return dic;
        }

        private static void _parse_element_limits(ref IDataReader reader, ref List<FormElement> lstElements)
        {
            while (reader.Read())
            {
                try
                {
                    FormElement formElement = new FormElement();

                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) formElement.ElementID = (Guid)reader["ElementID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) formElement.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Necessary"].ToString())) formElement.Necessary = (bool)reader["Necessary"];

                    string strType = string.Empty;
                    if (!string.IsNullOrEmpty(reader["Type"].ToString())) strType = (string)reader["Type"];
                    try { formElement.Type = (FormElementTypes)Enum.Parse(typeof(FormElementTypes), strType); }
                    catch { formElement.Type = null; }

                    if (!string.IsNullOrEmpty(reader["Info"].ToString())) formElement.Info = (string)reader["Info"];

                    if (!string.IsNullOrEmpty(formElement.Info) && formElement.Info[0] != '{')
                    {
                        formElement.Info = "{\"Options\":[" + ProviderUtil.list_to_string<string>(
                            ProviderUtil.get_tags_list(formElement.Info).Select(u => "\"" + Base64.encode(u) + "\"").ToList()) + "]}";
                    }

                    lstElements.Add(formElement);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_form_instances(ref IDataReader reader, ref List<FormType> lstForms)
        {
            while (reader.Read())
            {
                try
                {
                    FormType formtype = new FormType();

                    if (!string.IsNullOrEmpty(reader["InstanceID"].ToString())) formtype.InstanceID = (Guid)reader["InstanceID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) formtype.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["FormID"].ToString())) formtype.FormID = (Guid)reader["FormID"];
                    if (!string.IsNullOrEmpty(reader["FormTitle"].ToString())) formtype.Title = (string)reader["FormTitle"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) formtype.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["Filled"].ToString())) formtype.Filled = (bool)reader["Filled"];
                    if (!string.IsNullOrEmpty(reader["FillingDate"].ToString())) formtype.FillingDate = (DateTime)reader["FillingDate"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) formtype.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString())) formtype.Creator.UserName = (string)reader["CreatorUserName"];
                    if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString())) formtype.Creator.FirstName = (string)reader["CreatorFirstName"];
                    if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString())) formtype.Creator.LastName = (string)reader["CreatorLastName"];

                    lstForms.Add(formtype);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_form_instance_elements(ref IDataReader reader, ref List<FormElement> lstElements)
        {
            while (reader.Read())
            {
                try
                {
                    FormElement formElement = new FormElement();

                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) formElement.ElementID = (Guid)reader["ElementID"];
                    if (!string.IsNullOrEmpty(reader["InstanceID"].ToString())) formElement.FormInstanceID = (Guid)reader["InstanceID"];
                    if (!string.IsNullOrEmpty(reader["RefElementID"].ToString())) formElement.RefElementID = (Guid)reader["RefElementID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) formElement.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) formElement.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Help"].ToString())) formElement.Help = (string)reader["Help"];
                    if (!string.IsNullOrEmpty(reader["SequenceNumber"].ToString())) formElement.SequenceNumber = (int)reader["SequenceNumber"];
                    if (!string.IsNullOrEmpty(reader["TextValue"].ToString())) formElement.TextValue = (string)reader["TextValue"];
                    if (!string.IsNullOrEmpty(reader["FloatValue"].ToString())) formElement.FloatValue = (double)reader["FloatValue"];
                    if (!string.IsNullOrEmpty(reader["BitValue"].ToString())) formElement.BitValue = (bool)reader["BitValue"];
                    if (!string.IsNullOrEmpty(reader["DateValue"].ToString())) formElement.DateValue = (DateTime)reader["DateValue"];
                    if (!string.IsNullOrEmpty(reader["Necessary"].ToString())) formElement.Necessary = (bool)reader["Necessary"];
                    if (!string.IsNullOrEmpty(reader["UniqueValue"].ToString())) formElement.UniqueValue = (bool)reader["UniqueValue"];

                    string strType = string.Empty;
                    if (!string.IsNullOrEmpty(reader["Type"].ToString())) strType = (string)reader["Type"];
                    try { formElement.Type = (FormElementTypes)Enum.Parse(typeof(FormElementTypes), strType); }
                    catch { formElement.Type = null; }

                    if (!string.IsNullOrEmpty(reader["Info"].ToString())) formElement.Info = (string)reader["Info"];
                    if (!string.IsNullOrEmpty(reader["Weight"].ToString())) formElement.Weight = (double)reader["Weight"];

                    if (!string.IsNullOrEmpty(formElement.Info) && formElement.Info[0] != '{')
                    {
                        formElement.Info = "{\"Options\":[" + ProviderUtil.list_to_string<string>(
                            ProviderUtil.get_tags_list(formElement.Info).Select(u => "\"" + u + "\"").ToList()) + "]}";
                    }

                    if (!string.IsNullOrEmpty(reader["Filled"].ToString())) formElement.Filled = (bool)reader["Filled"];
                    if (!string.IsNullOrEmpty(reader["EditionsCount"].ToString()))
                        formElement.EditionsCount = (int)reader["EditionsCount"];

                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString()))
                        formElement.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString()))
                        formElement.Creator.UserName = (string)reader["CreatorUserName"];
                    if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString()))
                        formElement.Creator.FirstName = (string)reader["CreatorFirstName"];
                    if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString()))
                        formElement.Creator.LastName = (string)reader["CreatorLastName"];

                    lstElements.Add(formElement);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static Dictionary<Guid, List<SelectedGuidItem>> _parse_selected_guids(ref IDataReader reader)
        {
            Dictionary<Guid, List<SelectedGuidItem>> ret = new Dictionary<Guid, List<SelectedGuidItem>>();

            while (reader.Read())
            {
                try
                {
                    Guid? elementId = null;
                    Guid? id = null;
                    string name = string.Empty;

                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) elementId = (Guid)reader["ElementID"];
                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) id = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) name = (string)reader["Name"];

                    if (!elementId.HasValue || !id.HasValue) continue;

                    if (!ret.ContainsKey(elementId.Value)) ret[elementId.Value] = new List<SelectedGuidItem>();

                    ret[elementId.Value].Add(new SelectedGuidItem(id.Value, name: name, code: string.Empty, type: SelectedGuidItemType.None));
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        private static void _parse_element_changes(ref IDataReader reader, ref List<FormElement> lstElements)
        {
            while (reader.Read())
            {
                try
                {
                    FormElement formElement = new FormElement();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) formElement.ChangeID = (long)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) formElement.ElementID = (Guid)reader["ElementID"];
                    if (!string.IsNullOrEmpty(reader["Info"].ToString())) formElement.Info = (string)reader["Info"];
                    if (!string.IsNullOrEmpty(reader["TextValue"].ToString())) formElement.TextValue = (string)reader["TextValue"];
                    if (!string.IsNullOrEmpty(reader["FloatValue"].ToString())) formElement.FloatValue = (double)reader["FloatValue"];
                    if (!string.IsNullOrEmpty(reader["BitValue"].ToString())) formElement.BitValue = (bool)reader["BitValue"];
                    if (!string.IsNullOrEmpty(reader["DateValue"].ToString())) formElement.DateValue = (DateTime)reader["DateValue"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString())) formElement.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) formElement.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString())) formElement.Creator.UserName = (string)reader["CreatorUserName"];
                    if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString())) formElement.Creator.FirstName = (string)reader["CreatorFirstName"];
                    if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString())) formElement.Creator.LastName = (string)reader["CreatorLastName"];

                    lstElements.Add(formElement);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_form_records(ref IDataReader reader, 
            ref List<FormRecord> records, List<FormElement> elements)
        {
            while (reader.Read())
            {
                try
                {
                    FormRecord rec = new FormRecord();

                    if (string.IsNullOrEmpty(reader["InstanceID"].ToString())) continue;

                    rec.InstanceID = (Guid)reader["InstanceID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) rec.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString()))
                        rec.CreationDate = (DateTime)reader["CreationDate"];

                    foreach (FormElement el in elements)
                    {
                        RecordCell cell = new RecordCell(el.ElementID.Value, string.Empty);

                        if (!string.IsNullOrEmpty(reader[el.ElementID.ToString()].ToString()))
                            cell.Value = reader[el.ElementID.ToString()].ToString();

                        rec.Cells.Add(cell);
                    }

                    records.Add(rec);
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_form_statistics(ref IDataReader reader, ref double weightSum,
            ref double sum, ref double weightedSum, ref double avg, ref double weightedAvg, 
            ref double min, ref double max, ref double var, ref double stDev)
        {
            if (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["WeightSum"].ToString())) weightSum = (double)reader["WeightSum"];
                    if (!string.IsNullOrEmpty(reader["Sum"].ToString())) sum = (double)reader["Sum"];
                    if (!string.IsNullOrEmpty(reader["WeightedSum"].ToString())) weightedSum = (double)reader["WeightedSum"];
                    if (!string.IsNullOrEmpty(reader["Avg"].ToString())) avg = (double)reader["Avg"];
                    if (!string.IsNullOrEmpty(reader["WeightedAvg"].ToString())) weightedAvg = (double)reader["WeightedAvg"];
                    if (!string.IsNullOrEmpty(reader["Min"].ToString())) min = (double)reader["Min"];
                    if (!string.IsNullOrEmpty(reader["Max"].ToString())) max = (double)reader["Max"];
                    if (!string.IsNullOrEmpty(reader["Var"].ToString())) var = (double)reader["Var"];
                    if (!string.IsNullOrEmpty(reader["StDev"].ToString())) stDev = (double)reader["StDev"];
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_polls(ref IDataReader reader, ref List<Poll> polls)
        {
            while (reader.Read())
            {
                try
                {
                    Poll p = new Poll();
                    
                    if (!string.IsNullOrEmpty(reader["PollID"].ToString())) p.PollID = (Guid)reader["PollID"];
                    if (!string.IsNullOrEmpty(reader["IsCopyOfPollID"].ToString()))
                        p.IsCopyOfPollID = (Guid)reader["IsCopyOfPollID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) p.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) p.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["RefName"].ToString())) p.RefName = (string)reader["RefName"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                        p.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["RefDescription"].ToString()))
                        p.RefDescription = (string)reader["RefDescription"];
                    if (!string.IsNullOrEmpty(reader["BeginDate"].ToString())) p.BeginDate = (DateTime)reader["BeginDate"];
                    if (!string.IsNullOrEmpty(reader["FinishDate"].ToString())) p.FinishDate = (DateTime)reader["FinishDate"];
                    if (!string.IsNullOrEmpty(reader["ShowSummary"].ToString())) p.ShowSummary = (bool)reader["ShowSummary"];
                    if (!string.IsNullOrEmpty(reader["HideContributors"].ToString()))
                        p.HideContributors = (bool)reader["HideContributors"];

                    polls.Add(p);
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            long totalCount = (reader.NextResult()) ? ProviderUtil.succeed_long(reader) : 0;

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static void _parse_poll_status(ref IDataReader reader, ref string description,
            ref DateTime? beginDate, ref DateTime? finishDate, ref Guid? instanceId, 
            ref int? elementsCount, ref int? filledElementsCount, ref int? allFilledFormsCount)
        {
            if (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["BeginDate"].ToString())) beginDate = (DateTime)reader["BeginDate"];
                    if (!string.IsNullOrEmpty(reader["FinishDate"].ToString())) finishDate = (DateTime)reader["FinishDate"];
                    if (!string.IsNullOrEmpty(reader["InstanceID"].ToString())) instanceId = (Guid)reader["InstanceID"];
                    if (!string.IsNullOrEmpty(reader["ElementsCount"].ToString()))
                        elementsCount = (int)reader["ElementsCount"];
                    if (!string.IsNullOrEmpty(reader["FilledElementsCount"].ToString()))
                        filledElementsCount = (int)reader["FilledElementsCount"];
                    if (!string.IsNullOrEmpty(reader["AllFilledFormsCount"].ToString()))
                        allFilledFormsCount = (int)reader["AllFilledFormsCount"];
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_poll_abstract(ref IDataReader reader, ref List<PollAbstract> items)
        {
            Dictionary<Guid, PollAbstract> dic = new Dictionary<Guid, PollAbstract>();

            while (reader.Read())
            {
                try
                {
                    Guid? elementId = null;
                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) elementId = (Guid)reader["ElementID"];
                    if (!elementId.HasValue) continue;

                    if (!dic.ContainsKey(elementId.Value)) dic[elementId.Value] = 
                            new PollAbstract() { ElementID = elementId };

                    if (!string.IsNullOrEmpty(reader["TotalValuesCount"].ToString()))
                        dic[elementId.Value].TotalCount = (int)reader["TotalValuesCount"];

                    PollAbstractValue val = new PollAbstractValue();

                    if (string.IsNullOrEmpty(reader["Count"].ToString())) continue;
                    else val.Count = (int)reader["Count"];

                    if (string.IsNullOrEmpty(reader["Value"].ToString())) continue;
                    else {
                        object obj = reader["Value"];
                        Type tp = obj.GetType();

                        if (tp == typeof(String)) val.TextValue = (string)obj;
                        else if (tp == typeof(bool)) val.BitValue = (bool)obj;
                        else if (tp == typeof(double)) val.NumberValue = (double)obj;
                        else continue;
                    }


                    dic[elementId.Value].Values.Add(val);
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    try
                    {
                        Guid? elementId = null;
                        if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) elementId = (Guid)reader["ElementID"];
                        if (!elementId.HasValue || !dic.ContainsKey(elementId.Value)) continue;

                        if (!string.IsNullOrEmpty(reader["Min"].ToString()))
                            dic[elementId.Value].Min = (double)reader["Min"];
                        if (!string.IsNullOrEmpty(reader["Max"].ToString()))
                            dic[elementId.Value].Max = (double)reader["Max"];
                        if (!string.IsNullOrEmpty(reader["Avg"].ToString()))
                            dic[elementId.Value].Avg = (double)reader["Avg"];
                        if (!string.IsNullOrEmpty(reader["Var"].ToString()))
                            dic[elementId.Value].Var = (double)reader["Var"];
                        if (!string.IsNullOrEmpty(reader["StDev"].ToString()))
                            dic[elementId.Value].StDev = (double)reader["StDev"];
                    }
                    catch { }
                }
            }

            if (!reader.IsClosed) reader.Close();

            items = dic.Values.ToList();
        }

        private static void _parse_poll_element_instances(ref IDataReader reader, ref List<FormElement> lstElements)
        {
            while (reader.Read())
            {
                try
                {
                    FormElement formElement = new FormElement();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString()))
                        formElement.Creator.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString()))
                        formElement.Creator.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString()))
                        formElement.Creator.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString()))
                        formElement.Creator.LastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["ElementID"].ToString())) formElement.ElementID = (Guid)reader["ElementID"];
                    if (!string.IsNullOrEmpty(reader["RefElementID"].ToString())) formElement.RefElementID = (Guid)reader["RefElementID"];
                    if (!string.IsNullOrEmpty(reader["TextValue"].ToString())) formElement.TextValue = (string)reader["TextValue"];
                    if (!string.IsNullOrEmpty(reader["FloatValue"].ToString())) formElement.FloatValue = (double)reader["FloatValue"];
                    if (!string.IsNullOrEmpty(reader["BitValue"].ToString())) formElement.BitValue = (bool)reader["BitValue"];
                    if (!string.IsNullOrEmpty(reader["DateValue"].ToString())) formElement.DateValue = (DateTime)reader["DateValue"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString()))
                        formElement.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["LastModificationDate"].ToString()))
                        formElement.LastModificationDate = (DateTime)reader["LastModificationDate"];

                    string strType = string.Empty;
                    if (!string.IsNullOrEmpty(reader["Type"].ToString())) strType = (string)reader["Type"];
                    try { formElement.Type = (FormElementTypes)Enum.Parse(typeof(FormElementTypes), strType); }
                    catch { formElement.Type = null; }
                    
                    lstElements.Add(formElement);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_current_polls_count(ref IDataReader reader, ref int count, ref int doneCount)
        {
            if (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["Count"].ToString())) count = (int)reader["Count"];
                    if (!string.IsNullOrEmpty(reader["DoneCount"].ToString())) doneCount = (int)reader["DoneCount"];
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            if (!reader.IsClosed) reader.Close();
        }
        

        public static bool CreateForm(Guid applicationId, FormType Info)
        {
            string spName = GetFullyQualifiedName("CreateForm");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.FormID, Info.Title, Info.Creator.UserID, Info.CreationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormTitle(Guid applicationId, FormType Info)
        {
            string spName = GetFullyQualifiedName("SetFormTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.FormID, Info.Title, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormName(Guid applicationId, Guid formId, string name, 
            Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("SetFormName");

            try
            {
                if (string.IsNullOrEmpty(name)) name = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    formId, name, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormDescription(Guid applicationId, FormType Info)
        {
            string spName = GetFullyQualifiedName("SetFormDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.FormID, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool ArithmeticDeleteForm(Guid applicationId, Guid formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteForm");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    formId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool RecycleForm(Guid applicationId, Guid formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RecycleForm");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    formId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static void GetForms(Guid applicationId, ref List<FormType> retForms, 
            string searchText, int? count, int? lowerBoundary, bool? hasName, bool? archive)
        {
            string spName = GetFullyQualifiedName("GetForms");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.get_search_text(searchText), count, lowerBoundary, hasName, archive);
                _parse_form_types(ref reader, ref retForms);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetForms(Guid applicationId, ref List<FormType> retForms, ref List<Guid> formIds)
        {
            string spName = GetFullyQualifiedName("GetFormsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref formIds), ',');
                _parse_form_types(ref reader, ref retForms);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static bool AddFormElement(Guid applicationId, FormElement Info, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("AddFormElement");

            try
            {
                if (string.IsNullOrEmpty(Info.Name)) Info.Name = null;
                FormElementTypes type = Info.Type.HasValue ? Info.Type.Value : FormElementTypes.Text;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ElementID, Info.FormID, Info.Title, Info.Name, Info.Help, Info.SequenceNumber, type.ToString(), Info.Info,
                    Info.Creator.UserID, Info.CreationDate), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool ModifyFormElement(Guid applicationId, FormElement Info, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("ModifyFormElement");

            try
            {
                if (string.IsNullOrEmpty(Info.Name)) Info.Name = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ElementID, Info.Title, Info.Name, Info.Help,
                    Info.Info, Info.Weight, Info.LastModifierUserID, Info.LastModificationDate), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetElementsOrder(Guid applicationId, List<Guid> elementIds)
        {
            string spName = GetFullyQualifiedName("SetElementsOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(elementIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormElementNecessity(Guid applicationId, Guid elementId, bool necessity)
        {
            string spName = GetFullyQualifiedName("SetFormElementNecessity");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, elementId, necessity));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormElementUniqueness(Guid applicationId, Guid elementId, bool value)
        {
            string spName = GetFullyQualifiedName("SetFormElementUniqueness");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, elementId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool ArithmeticDeleteFormElement(Guid applicationId, Guid elementId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteFormElement");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    elementId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SaveFormElements(Guid applicationId, Guid formId, 
            string title, string name, string description, List<FormElement> elements, Guid currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Elements
            DataTable elementsTable = new DataTable();
            elementsTable.Columns.Add("ElementID", typeof(Guid));
            elementsTable.Columns.Add("InstanceID", typeof(Guid));
            elementsTable.Columns.Add("RefElementID", typeof(Guid));
            elementsTable.Columns.Add("Title", typeof(string));
            elementsTable.Columns.Add("Name", typeof(string));
            elementsTable.Columns.Add("SequenceNumber", typeof(int));
            elementsTable.Columns.Add("Necessary", typeof(bool));
            elementsTable.Columns.Add("UniqueValue", typeof(bool));
            elementsTable.Columns.Add("Type", typeof(string));
            elementsTable.Columns.Add("Help", typeof(string));
            elementsTable.Columns.Add("Info", typeof(string));
            elementsTable.Columns.Add("Weight", typeof(double));
            elementsTable.Columns.Add("TextValue", typeof(string));
            elementsTable.Columns.Add("FloatValue", typeof(double));
            elementsTable.Columns.Add("BitValue", typeof(bool));
            elementsTable.Columns.Add("DateValue", typeof(DateTime));

            int seq = 1;

            foreach (FormElement _elem in elements)
            {
                string strType = null;
                if (_elem.Type.HasValue) strType = _elem.Type.Value.ToString();

                _elem.SequenceNumber = seq++;

                elementsTable.Rows.Add(_elem.ElementID, Guid.NewGuid(), _elem.RefElementID,
                    PublicMethods.verify_string(_elem.Title), _elem.Name, _elem.SequenceNumber, _elem.Necessary,
                    _elem.UniqueValue, strType, PublicMethods.verify_string(_elem.Help), _elem.Info, _elem.Weight,
                    PublicMethods.verify_string(_elem.TextValue), _elem.FloatValue, _elem.BitValue, _elem.DateValue);
            }

            SqlParameter elementsParam = new SqlParameter("@Elements", SqlDbType.Structured);
            elementsParam.TypeName = "[dbo].[FormElementTableType]";
            elementsParam.Value = elementsTable;
            //end of Elements

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@FormID", formId);
            if(!string.IsNullOrEmpty(title)) cmd.Parameters.AddWithValue("@Title", title);
            if (!string.IsNullOrEmpty(name)) cmd.Parameters.AddWithValue("@Name", name);
            if (!string.IsNullOrEmpty(description)) cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.Add(elementsParam);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("SaveFormElements");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@FormID" + sep + 
                (string.IsNullOrEmpty(title) ? "null" : "@Title") + sep +
                (string.IsNullOrEmpty(name) ? "null" : "@Name") + sep +
                (string.IsNullOrEmpty(description) ? "null" : "@Description") + sep + 
                "@Elements" + sep + "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
            finally { con.Close(); }
        }

        public static void GetFormElements(Guid applicationId, ref List<FormElement> retElements, 
            Guid? formId, Guid? ownerId, FormElementTypes? type)
        {
            string spName = GetFullyQualifiedName("GetFormElements");

            try
            {
                if (formId.HasValue && formId == Guid.Empty) formId = null;
                if (ownerId.HasValue && ownerId == Guid.Empty) ownerId = null;

                string strType = null;
                if (type.HasValue) strType = type.Value.ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, formId, ownerId, type);
                _parse_form_elements(ref reader, ref retElements);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetFormElements(Guid applicationId, ref List<FormElement> retElements, List<Guid> elementIds)
        {
            string spName = GetFullyQualifiedName("GetFormElementsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, string.Join(",", elementIds), ',');
                _parse_form_elements(ref reader, ref retElements);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static Dictionary<string, Guid> GetFormElementIDs(Guid applicationId, Guid formId, List<string> names)
        {
            string spName = GetFullyQualifiedName("GetFormElementIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, formId, string.Join(",", names), ',');
                return _parse_element_ids(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return new Dictionary<string, Guid>();
            }
        }

        public static List<Guid> IsFormElement(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsFormElement");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return new List<Guid>();
            }
        }

        public static bool CreateFormInstance(Guid applicationId, List<FormType> instances, Guid currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Instances
            DataTable instancesTable = new DataTable();
            instancesTable.Columns.Add("InstanceID", typeof(Guid));
            instancesTable.Columns.Add("FormID", typeof(Guid));
            instancesTable.Columns.Add("OwnerID", typeof(Guid));
            instancesTable.Columns.Add("DirectorID", typeof(Guid));
            instancesTable.Columns.Add("Admin", typeof(bool));
            instancesTable.Columns.Add("IsTemporary", typeof(bool));

            foreach (FormType frm in instances)
                instancesTable.Rows.Add(frm.InstanceID, frm.FormID, frm.OwnerID, frm.DirectorID, frm.Admin, frm.IsTemporary);

            SqlParameter instancesParam = new SqlParameter("@Instances", SqlDbType.Structured);
            instancesParam.TypeName = "[dbo].[FormInstanceTableType]";
            instancesParam.Value = instancesTable;
            //end of Instances
            
            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(instancesParam);
            cmd.Parameters.AddWithValue("@CreatorUserID", currentUserId);
            cmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);

            string spName = GetFullyQualifiedName("CreateFormInstance");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Instances" + sep + "@CreatorUserID" + sep + "@CreationDate";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool RemoveFormInstances(Guid applicationId, 
            ref List<Guid> instanceIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveFormInstances");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref instanceIds), ',', currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool RemoveOwnerFormInstances(Guid applicationId,
            Guid ownerId, Guid formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveOwnerFormInstances");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, formId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static void GetOwnerFormInstances(Guid applicationId, 
            ref List<FormType> retInstances, ref List<Guid> ownerIds, Guid? formId, bool? isTemporary, Guid? userId)
        {
            string spName = GetFullyQualifiedName("GetOwnerFormInstances");

            try
            {
                if (formId == Guid.Empty) formId = null;
                if (userId == Guid.Empty) userId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref ownerIds), ',', formId, isTemporary, userId);
                _parse_form_instances(ref reader, ref retInstances);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetFormInstances(Guid applicationId, 
            ref List<FormType> retInstances, ref List<Guid> instanceIds)
        {
            string spName = GetFullyQualifiedName("GetFormInstances");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref instanceIds), ',');
                _parse_form_instances(ref reader, ref retInstances);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static Guid? GetFormInstanceOwnerID(Guid applicationId, Guid instanceIdOrElementId)
        {
            string spName = GetFullyQualifiedName("GetFormInstanceOwnerID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, instanceIdOrElementId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return null;
            }
        }

        public static Guid? GetFormInstanceHierarchyOwnerID(Guid applicationId, Guid instanceId)
        {
            string spName = GetFullyQualifiedName("GetFormInstanceHierarchyOwnerID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, instanceId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return null;
            }
        }

        public static bool ValidateNewName(Guid applicationId, Guid objectId, Guid? formId, string name)
        {
            string spName = GetFullyQualifiedName("ValidateNewName");

            try
            {
                if (!string.IsNullOrEmpty(name)) name = name.Trim().ToLower();

                return string.IsNullOrEmpty(name) || (FGUtilities.is_valid_name(name) &&
                    ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, objectId, formId, name)));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool MeetsUniqueConstraint(Guid applicationId, 
            Guid instanceId, Guid elementId, string textValue, double? floatValue)
        {
            string spName = GetFullyQualifiedName("MeetsUniqueConstraint");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    instanceId, elementId, textValue, floatValue));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SaveFormInstanceElements(Guid applicationId, 
            ref List<FormElement> elements, List<Guid> elementsToClear, Guid currentUserId, ref string errorMessage)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Elements
            DataTable elementsTable = new DataTable();
            elementsTable.Columns.Add("ElementID", typeof(Guid));
            elementsTable.Columns.Add("InstanceID", typeof(Guid));
            elementsTable.Columns.Add("RefElementID", typeof(Guid));
            elementsTable.Columns.Add("Title", typeof(string));
            elementsTable.Columns.Add("Name", typeof(string));
            elementsTable.Columns.Add("SequenceNumber", typeof(int));
            elementsTable.Columns.Add("Necessary", typeof(bool));
            elementsTable.Columns.Add("UniqueValue", typeof(bool));
            elementsTable.Columns.Add("Type", typeof(string));
            elementsTable.Columns.Add("Help", typeof(string));
            elementsTable.Columns.Add("Info", typeof(string));
            elementsTable.Columns.Add("Weight", typeof(double));
            elementsTable.Columns.Add("TextValue", typeof(string));
            elementsTable.Columns.Add("FloatValue", typeof(double));
            elementsTable.Columns.Add("BitValue", typeof(bool));
            elementsTable.Columns.Add("DateValue", typeof(DateTime));

            foreach (FormElement _elem in elements)
            {
                string strType = null;
                if (_elem.Type.HasValue) strType = _elem.Type.Value.ToString();

                elementsTable.Rows.Add(_elem.ElementID, _elem.FormInstanceID, _elem.RefElementID,
                    PublicMethods.verify_string(_elem.Title), _elem.Name, _elem.SequenceNumber, _elem.Necessary,
                    _elem.UniqueValue, strType, PublicMethods.verify_string(_elem.Help), _elem.Info, _elem.Weight,
                    PublicMethods.verify_string(_elem.TextValue), _elem.FloatValue, _elem.BitValue, _elem.DateValue);
            }

            SqlParameter elementsParam = new SqlParameter("@Elements", SqlDbType.Structured);
            elementsParam.TypeName = "[dbo].[FormElementTableType]";
            elementsParam.Value = elementsTable;
            //end of Elements

            //GuidItems
            DataTable guidItemsTable = new DataTable();
            guidItemsTable.Columns.Add("FirstValue", typeof(Guid));
            guidItemsTable.Columns.Add("SecondValue", typeof(Guid));

            List<FormElementTypes> validTypes = 
                new List<FormElementTypes>() { FormElementTypes.Node, FormElementTypes.User, FormElementTypes.MultiLevel };

            elements.Where(u => validTypes.Any(v => v == u.Type)).ToList()
                .ForEach(u => u.GuidItems.Where(i => i.ID.HasValue).ToList()
                .ForEach(x => guidItemsTable.Rows.Add(u.ElementID.Value, x.ID.Value)));

            SqlParameter guidItemsParam = new SqlParameter("@GuidItems", SqlDbType.Structured);
            guidItemsParam.TypeName = "[dbo].[GuidPairTableType]";
            guidItemsParam.Value = guidItemsTable;
            //end of GuidItems

            //Elements to Clear
            if (elementsToClear == null) elementsToClear = new List<Guid>();

            DataTable clearTable = new DataTable();
            clearTable.Columns.Add("Value", typeof(Guid));

            foreach (Guid g in elementsToClear) clearTable.Rows.Add(g);

            SqlParameter clearParam = new SqlParameter("@ElementsToClear", SqlDbType.Structured);
            clearParam.TypeName = "[dbo].[GuidTableType]";
            clearParam.Value = clearTable;
            //end of Elements

            //Files
            List<DocFileInfo> files = new List<DocFileInfo>();
            
            DataTable filesTable = new DataTable();
            filesTable.Columns.Add("FileID", typeof(Guid));
            filesTable.Columns.Add("FileName", typeof(string));
            filesTable.Columns.Add("Extension", typeof(string));
            filesTable.Columns.Add("MIME", typeof(string));
            filesTable.Columns.Add("Size", typeof(long));
            filesTable.Columns.Add("OwnerID", typeof(Guid));
            filesTable.Columns.Add("OwnerType", typeof(string));

            foreach (FormElement el in elements)
            {
                if (el == null || el.AttachedFiles == null) continue;

                foreach (DocFileInfo f in el.AttachedFiles)
                    filesTable.Rows.Add(f.FileID, f.FileName, f.Extension, f.MIME(), f.Size, f.OwnerID, f.OwnerType);
            }

            SqlParameter filesParam = new SqlParameter("@Files", SqlDbType.Structured);
            filesParam.TypeName = "[dbo].[DocFileInfoTableType]";
            filesParam.Value = filesTable;
            //end of Files

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(elementsParam);
            cmd.Parameters.Add(guidItemsParam);
            cmd.Parameters.Add(clearParam);
            cmd.Parameters.Add(filesParam);
            cmd.Parameters.AddWithValue("@CreatorUserID", currentUserId);
            cmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);

            string spName = GetFullyQualifiedName("SaveFormInstanceElements");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Elements" + sep + "@GuidItems" + sep +
                "@ElementsToClear" + sep + "@Files" + sep + "@CreatorUserID" + sep + "@CreationDate";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader(), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
            finally { con.Close(); }
        }

        public static void GetFormInstanceElements(Guid applicationId, ref List<FormElement> retElements,
            List<Guid> instanceIds, ref List<Guid> elementIds, bool? filled)
        {
            string spName = GetFullyQualifiedName("GetFormInstanceElements");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    string.Join(",", instanceIds), filled, string.Join(",", elementIds), ',');
                _parse_form_instance_elements(ref reader, ref retElements);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static Dictionary<Guid, List<SelectedGuidItem>> GetSelectedGuids(Guid applicationId, List<Guid> elementIds)
        {
            string spName = GetFullyQualifiedName("GetSelectedGuids");

            try
            {
                if (elementIds == null || elementIds.Count == 0) return new Dictionary<Guid, List<SelectedGuidItem>>();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref elementIds), ',');
                return _parse_selected_guids(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return new Dictionary<Guid, List<SelectedGuidItem>>();
            }
        }

        public static void GetElementChanges(Guid applicationId,
            ref List<FormElement> retElements, Guid elementId, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetElementChanges");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    elementId, count, lowerBoundary);
                _parse_element_changes(ref reader, ref retElements);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static bool SetFormInstanceAsFilled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetFormInstanceAsFilled");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    instanceId, DateTime.Now, currentUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormInstanceAsNotFilled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetFormInstanceAsNotFilled");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, instanceId, currentUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool IsDirector(Guid applicationId, Guid instanceId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsDirector");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, instanceId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetFormOwner(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetFormOwner");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, formId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool ArithmeticDeleteFormOwner(Guid applicationId, 
            Guid ownerId, Guid formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteFormOwner");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, formId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static FormType GetOwnerForm(Guid applicationId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetOwnerForm");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                List<FormType> forms = new List<FormType>();
                _parse_form_types(ref reader, ref forms);
                return forms.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return null;
            }
        }

        public static Guid? InitializeOwnerFormInstance(Guid applicationId, Guid ownerId, Guid? formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("InitializeOwnerFormInstance");

            try
            {
                Guid? instanceId = ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, formId, currentUserId, DateTime.Now));

                if (instanceId == Guid.Empty) instanceId = null;

                return instanceId;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return null;
            }
        }

        public static bool SetElementLimits(Guid applicationId, 
            Guid ownerId, ref List<Guid> elementIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetElementLimits");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, ProviderUtil.list_to_string<Guid>(ref elementIds), ',', currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static void GetElementLimits(Guid applicationId, ref List<FormElement> retElements, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetElementLimits");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                _parse_element_limits(ref reader, ref retElements);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static bool SetElementLimitNecessity(Guid applicationId, 
            Guid ownerId, Guid elementId, bool necessary, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetElementLimitNecessity");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, elementId, necessary, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool ArithmeticDeleteElementLimit(Guid applicationId, 
            Guid ownerId, Guid elementId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteElementLimit");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, elementId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static void GetCommonFormInstanceIDs(Guid applicationId, 
            ref List<Guid> retIds, Guid ownerId, Guid filledOwnerId, bool hasLimit)
        {
            string spName = GetFullyQualifiedName("GetCommonFormInstanceIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, filledOwnerId, hasLimit);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetFormRecords(Guid applicationId, ref List<FormRecord> retRecords, Guid formId, 
            List<Guid> elementIds, List<Guid> instanceIds, List<Guid> ownerIds, List<FormFilter> filters, 
            int? lowerBoundary, int? count, Guid? sortByElementId, bool? descending)
        {
            if (elementIds == null) elementIds = new List<Guid>();
            if (instanceIds == null) instanceIds = new List<Guid>();
            if (ownerIds == null) ownerIds = new List<Guid>();
            if (filters == null) filters = new List<FormGenerator.FormFilter>();

            List<FormElement> elements = FGController.get_form_elements(applicationId, formId);

            if (elementIds != null && elementIds.Count > 0)
            {
                elementIds = elementIds.Where(u => elements.Any(v => v.ElementID == u)).ToList();

                elements = elements.Where(u => elementIds.Any(v => v == u.ElementID))
                    .OrderBy(x => x.SequenceNumber).ToList();
            }

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            if (sortByElementId == Guid.Empty) sortByElementId = null;

            //Add ElementIDs
            DataTable elementIdsTable = new DataTable();
            elementIdsTable.Columns.Add("Value", typeof(Guid));

            foreach (Guid eId in elementIds)
                elementIdsTable.Rows.Add(eId);

            SqlParameter elementIdsParam = new SqlParameter("@ElementIDs", SqlDbType.Structured);
            elementIdsParam.TypeName = "[dbo].[GuidTableType]";
            elementIdsParam.Value = elementIdsTable;
            //end of Add ElementIDs

            //Add InstanceIDs
            DataTable instanceIdsTable = new DataTable();
            instanceIdsTable.Columns.Add("Value", typeof(Guid));

            foreach (Guid iId in instanceIds)
                instanceIdsTable.Rows.Add(iId);

            SqlParameter instanceIdsParam = new SqlParameter("@InstanceIDs", SqlDbType.Structured);
            instanceIdsParam.TypeName = "[dbo].[GuidTableType]";
            instanceIdsParam.Value = instanceIdsTable;
            //end of Add InstanceIDs

            //Add OwnerIDs
            DataTable ownerIdsTable = new DataTable();
            ownerIdsTable.Columns.Add("Value", typeof(Guid));

            foreach (Guid oId in ownerIds)
                ownerIdsTable.Rows.Add(oId);

            SqlParameter ownerIdsParam = new SqlParameter("@OwnerIDs", SqlDbType.Structured);
            ownerIdsParam.TypeName = "[dbo].[GuidTableType]";
            ownerIdsParam.Value = ownerIdsTable;
            //end of Add OwnerIDs

            //Add Filters
            DataTable filtersTable = new DataTable();
            filtersTable.Columns.Add("ElementID", typeof(Guid));
            filtersTable.Columns.Add("OwnerID", typeof(Guid));
            filtersTable.Columns.Add("Text", typeof(string));
            filtersTable.Columns.Add("TextItems", typeof(string));
            filtersTable.Columns.Add("Or", typeof(bool));
            filtersTable.Columns.Add("Exact", typeof(bool));
            filtersTable.Columns.Add("DateFrom", typeof(DateTime));
            filtersTable.Columns.Add("DateTo", typeof(DateTime));
            filtersTable.Columns.Add("FloatFrom", typeof(double));
            filtersTable.Columns.Add("FloatTo", typeof(double));
            filtersTable.Columns.Add("Bit", typeof(bool));
            filtersTable.Columns.Add("Guid", typeof(Guid));
            filtersTable.Columns.Add("GuidItems", typeof(string));
            filtersTable.Columns.Add("Compulsory", typeof(bool));

            foreach (FormFilter f in filters)
            {
                filtersTable.Rows.Add(f.ElementID, f.OwnerID, f.Text, ProviderUtil.list_to_string<string>(f.TextItems),
                    f.Or, f.Exact, f.DateFrom, f.DateTo, f.FloatFrom, f.FloatTo, f.Bit, f.Guid,
                    ProviderUtil.list_to_string<Guid>(f.GuidItems), f.Compulsory);
            }

            SqlParameter filtersParam = new SqlParameter("@Filters", SqlDbType.Structured);
            filtersParam.TypeName = "[dbo].[FormFilterTableType]";
            filtersParam.Value = filtersTable;
            //end of Add Filters

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@FormID", formId);
            cmd.Parameters.Add(elementIdsParam);
            cmd.Parameters.Add(instanceIdsParam);
            cmd.Parameters.Add(ownerIdsParam);
            cmd.Parameters.Add(filtersParam);
            if(lowerBoundary.HasValue) cmd.Parameters.AddWithValue("@LowerBoundary", lowerBoundary);
            if(count.HasValue) cmd.Parameters.AddWithValue("@Count", count);
            if(sortByElementId.HasValue) cmd.Parameters.AddWithValue("@SortByElementID", sortByElementId);
            if (descending.HasValue) cmd.Parameters.AddWithValue("@DESC", descending);
            
            string spName = GetFullyQualifiedName("GetFormRecords");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@FormID" + sep + "@ElementIDs" + sep +
                "@InstanceIDs" + sep + "@OwnerIDs" + sep + "@Filters" + sep +
                (!lowerBoundary.HasValue ? "null" : "@LowerBoundary") + sep +
                (!count.HasValue ? "null" : "@Count") + sep +
                (!sortByElementId.HasValue ? "null" : "@SortByElementID") + sep +
                (!descending.HasValue ? "null" : "@DESC");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                _parse_form_records(ref reader, ref retRecords, elements);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
            finally { con.Close(); }
        }

        public static void GetFormStatistics(Guid applicationId, Guid? ownerId, Guid? instanceId,
            ref double weightSum, ref double sum, ref double weightedSum, ref double avg, ref double weightedAvg,
            ref double min, ref double max, ref double var, ref double stDev)
        {
            string spName = GetFullyQualifiedName("GetFormStatistics");

            try
            {
                if (ownerId == Guid.Empty) ownerId = null;
                if (instanceId == Guid.Empty) instanceId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId, instanceId);
                _parse_form_statistics(ref reader, ref weightSum, ref sum, ref weightedSum, ref avg, 
                    ref weightedAvg, ref min, ref max, ref var, ref stDev);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static bool ConvertFormToTable(Guid applicationId, Guid formId)
        {
            string spName = GetFullyQualifiedName("ConvertFormToTable");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, formId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        //Polls

        public static void GetPolls(Guid applicationId, ref List<Poll> ret, Guid? isCopyOfPollId, Guid? ownerId,
            bool? archive, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetPolls");

            try
            {
                if (isCopyOfPollId == Guid.Empty) isCopyOfPollId = null;
                if (ownerId == Guid.Empty) ownerId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, isCopyOfPollId, ownerId,
                    archive, ProviderUtil.get_search_text(searchText), count, lowerBoundary);
                totalCount = _parse_polls(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetPolls(Guid applicationId, ref List<Poll> ret, List<Guid> pollIds)
        {
            string spName = GetFullyQualifiedName("GetPollsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, string.Join(",", pollIds), ',');
                _parse_polls(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static bool AddPoll(Guid applicationId, Guid pollId, Guid? copyFromPollId, 
            Guid? ownerId, string name, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddPoll");

            try
            {
                if (string.IsNullOrEmpty(name)) name = null;
                if (copyFromPollId == Guid.Empty) copyFromPollId = null;
                if (ownerId == Guid.Empty) ownerId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    pollId, copyFromPollId, ownerId, name, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static Guid? GetPollInstance(Guid applicationId,
            Guid? pollId, Guid copyFromPollId, Guid? ownerId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("GetPollInstance");

            try
            {
                if (pollId == Guid.Empty) pollId = null;
                if (ownerId == Guid.Empty) ownerId = null;

                Guid? result = ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, copyFromPollId, ownerId, currentUserId, DateTime.Now));

                return result == Guid.Empty ? null : result;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return null;
            }
        }

        public static void GetOwnerPollIDs(Guid applicationId, ref List<Guid> ret, Guid isCopyOfPollId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetOwnerPollIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, isCopyOfPollId, ownerId);
                ProviderUtil.parse_guids(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static bool RenamePoll(Guid applicationId, Guid pollId, string name, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RenamePoll");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, name, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetPollDescription(Guid applicationId, Guid pollId, string description, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPollDescription");

            try
            {
                if (string.IsNullOrEmpty(description)) description = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, description, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetPollBeginDate(Guid applicationId, Guid pollId, DateTime? beginDate, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPollBeginDate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, beginDate, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetPollFinishDate(Guid applicationId, Guid pollId, DateTime? finishDate, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPollFinishDate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, finishDate, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetPollShowSummary(Guid applicationId, Guid pollId, bool showSummary, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPollShowSummary");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, showSummary, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool SetPollHideContributors(Guid applicationId, Guid pollId, bool hideContributors, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPollHideContributors");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, hideContributors, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool RemovePoll(Guid applicationId, Guid pollId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemovePoll");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static bool RecyclePoll(Guid applicationId, Guid pollId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RecyclePoll");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    pollId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return false;
            }
        }

        public static void GetPollStatus(Guid applicationId, Guid? pollId, Guid? isCopyOfPollId, 
            Guid currentUserId, ref string description, ref DateTime? beginDate, ref DateTime? finishDate, 
            ref Guid? instanceId, ref int? elementsCount, ref int? filledElementsCount, ref int? allFilledFormsCount)
        {
            string spName = GetFullyQualifiedName("GetPollStatus");

            try
            {
                if (pollId == Guid.Empty) pollId = null;
                if (isCopyOfPollId == Guid.Empty) isCopyOfPollId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, pollId, isCopyOfPollId, currentUserId);
                _parse_poll_status(ref reader, ref description, ref beginDate, ref finishDate, ref instanceId,
                    ref elementsCount, ref filledElementsCount, ref allFilledFormsCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static Dictionary<Guid, int> GetPollElementsInstanceCount(Guid applicationId, Guid pollId)
        {
            string spName = GetFullyQualifiedName("GetPollElementsInstanceCount");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, pollId);
                return ProviderUtil.parse_items_count(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return new Dictionary<Guid, int>();
            }
        }

        public static void GetPollAbstractText(Guid applicationId, ref List<PollAbstract> ret, 
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetPollAbstractText");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, pollId,
                    string.Join(",", elementIds), ',', count, lowerBoundary);
                _parse_poll_abstract(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetPollAbstractGuid(Guid applicationId, ref List<PollAbstract> ret,
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetPollAbstractGuid");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, pollId,
                    string.Join(",", elementIds), ',', count, lowerBoundary);
                _parse_poll_abstract(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetPollAbstractBool(Guid applicationId, ref List<PollAbstract> ret,
            Guid pollId, List<Guid> elementIds)
        {
            string spName = GetFullyQualifiedName("GetPollAbstractBool");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, pollId,
                    string.Join(",", elementIds), ',');
                _parse_poll_abstract(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetPollAbstractNumber(Guid applicationId, ref List<PollAbstract> ret,
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetPollAbstractNumber");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, pollId,
                    string.Join(",", elementIds), ',', count, lowerBoundary);
                _parse_poll_abstract(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetPollElementInstances(Guid applicationId, ref List<FormElement> ret,
            Guid pollId, Guid elementId, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetPollElementInstances");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    pollId, elementId, count, lowerBoundary);
                _parse_poll_element_instances(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static void GetCurrentPollsCount(Guid applicationId, 
            Guid? currentUserId, ref int count, ref int doneCount)
        {
            string spName = GetFullyQualifiedName("GetCurrentPollsCount");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    currentUserId, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                _parse_current_polls_count(ref reader, ref count, ref doneCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
            }
        }

        public static Dictionary<Guid, bool> GetCurrentPolls(Guid applicationId, 
            Guid? currentUserId, int? count, int? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetCurrentPolls");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    currentUserId, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId), count, lowerBoundary);
                return ProviderUtil.parse_items_status_bool(ref reader, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return new Dictionary<Guid, bool>();
            }
        }

        public static List<Guid> IsPoll(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsPoll");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.FG);
                return new List<Guid>();
            }
        }

        //end of Polls
    }
}
