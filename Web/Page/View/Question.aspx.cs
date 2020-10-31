using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class Question : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                if (!RaaiVanSettings.AllowNotAuthenticatedUsers(paramsContainer.ApplicationID) && !paramsContainer.IsAuthenticated)
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                if (!paramsContainer.ApplicationID.HasValue) return;

                if (!Modules.RaaiVanConfig.Modules.QA(paramsContainer.Tenant.Id)) Response.Redirect(PublicConsts.HomePage);

                Guid currentUserId = paramsContainer.CurrentUserID.HasValue ? paramsContainer.CurrentUserID.Value : Guid.Empty;

                Guid questionId = Guid.Empty;
                Guid.TryParse(Request.Params["id"], out questionId);
                if(questionId == Guid.Empty) Guid.TryParse(Request.Params["QuestionID"], out questionId);

                if (questionId != Guid.Empty)
                {
                    initialJson.Value = "{\"QuestionID\":\"" + questionId.ToString() + "\"}";

                    //register question page visitor
                    if (Session[questionId.ToString()] == null)
                    {
                        Session[questionId.ToString()] = true;
                        UsersController.register_item_visit(paramsContainer.Tenant.Id,
                            questionId, currentUserId, DateTime.Now, VisitItemTypes.Question);
                    }
                    //end of question page visitor registration
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "QuestionViewPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}