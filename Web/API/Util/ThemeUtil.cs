using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web
{
    public class ThemeUtil
    {
        private static Dictionary<string, Dictionary<string, string>> ThemeContent =
            new Dictionary<string, Dictionary<string, string>>();

        private static string TemplateContent;

        private static Dictionary<string, string> get_theme_content(Guid? applicationId, string name)
        {
            if (!string.IsNullOrEmpty(name)) name = name.Trim().ToLower();

            if (string.IsNullOrEmpty(name)) return null;
            else if (ThemeContent.ContainsKey(name)) return ThemeContent[name];

            string thm = new DocFileInfo()
            {
                FileName = name,
                Extension = "css",
                FolderName = FolderNames.Themes
            }.get_text_content(applicationId);

            Dictionary<string, string> vars = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(thm)) return null;

            Expressions.get_matches_string(thm, Expressions.Patterns.CSSVariable)
                .Where(v => !string.IsNullOrEmpty(v)).ToList()
                .ForEach(v =>
                {
                    List<string> pair = v.Replace(";", "").Split(':').ToList();
                    if (pair.Count == 2 && !string.IsNullOrEmpty(pair[0].Trim()) && !string.IsNullOrEmpty(pair[1].Trim()))
                        vars[pair[0].Trim()] = pair[1].Trim();
                });

            ThemeContent[name] = vars;

            return vars;
        }

        private static string get_template()
        {
            if (!string.IsNullOrEmpty(TemplateContent)) return TemplateContent;

            string path = PublicMethods.map_path(PublicConsts.GlobalCSS);
            if (File.Exists(path)) TemplateContent = File.ReadAllText(path);

            return TemplateContent;
        }

        public static string get_theme(Guid? applicationId, string name)
        {
            string template = get_template();
            Dictionary<string, string> dic = get_theme_content(applicationId, name);

            if (string.IsNullOrEmpty(template) || dic == null) return string.Empty;
                
            dic.Keys.ToList().ForEach(k => {
                template = template.Replace("var(" + k + ")", dic[k]);
            });

            return template;
        }
    }
}