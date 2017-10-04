using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Gboxt.Common.DataAccess.Schemas;

namespace Agebull.Common.SimpleDesign
{
    [Export(typeof(IAutoRegister))]
    [ExportMetadata("Symbol", '%')]
    public class EasyUiFormCoder : EasyUiCoderBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        protected override string FileSaveConfigName => "File_Aspnet_Form";

        public override string Code()
        {
            if (Entity.FormCloumn <= 0)
                Entity.FormCloumn = 1;
            var ext = Entity.MaxForm ? " isPanel='true'" : $" style='width:{Entity.FormCloumn * 485}px;'";
            string name = Entity.Name.ToLWord();
            var fields = Entity.ClientProperty.Where(p => !p.NoneDetails).ToArray();
            return $@"
<form name='{name}Form' id='{name}Form' method='POST' enctype='multipart/form-data'>{FormHide()}
    <div id='{name}Region' class='formRegion'{ext}>{FormFields(fields)}
    </div>
</form>";
        }

        /// <summary>
        ///     生成Form录入字段界面
        /// </summary>
        private string FormHide()
        {
            StringBuilder code = new StringBuilder();
            foreach (var field in Entity.ClientProperty.Where(p => p.NoneDetails && (p.IsPrimaryKey || p["user_form_hide"] == "True" || !p.IsCompute && !p.IsSystemField)))
            {
                code.Append($@"
    <input name='{field.Name}' type='hidden' />");
            }
            return code.ToString();
        }

        /// <summary>
        ///     生成Form录入字段界面
        /// </summary>
        /// <param name="columns"></param>
        private string FormFields(PropertyConfig[] columns)
        {
            var code = new StringBuilder();
            foreach (var field in columns)
            {
                var caption = field.Caption;
                var description = field.Description;

                if (field.IsLinkKey)
                {
                    var friend = Entity.Properties.FirstOrDefault(p => p.LinkTable == field.LinkTable && p.IsLinkCaption);
                    if (friend != null)
                        caption = friend.Caption;
                    if (friend != null)
                        description = friend.Description;
                }
                FormField(code, field, caption, description ?? field.Caption);
            }
            //foreach (var field in columns.Where(p => p.IsMemo))
            //{
            //    FormField(code, field, field.Caption, field.Description ?? field.Caption);
            //}
            return code.ToString();
        }

        private static void FormField(StringBuilder code, PropertyConfig field, string caption, string description)
        {
            string cssEnd = "";
            int wid = 0;
            if (field.FormCloumnSapn > 1)
            {
                cssEnd = "_" + field.FormCloumnSapn + "c";
                code.Append(@"<br/>");
            }

            code.Append($@"
            <div class='inputField{cssEnd}' id='fr_{field.Name}'>
                <div class='inputRegion' id='ir_{field.Name}'>
                    <div class='inputLabel'>{caption}:</div>");

            PropertyEasyUiModel.CheckField(field);

            if (field.InputType == "editor")
            {
                var css = field.FormCloumnSapn <= 1 ? "inputValue_Rich_S" : "inputValue_Rich";
                code.Append($@"<br/>
                    <div class='inputValue_Rich_b'>
                        <div id='{field.Name}' name='{field.Name}' class='myueditor {css}'></div>
                    </div>");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(field.Prefix))
                {
                    code.Append(field.Prefix);
                }

                string br = null;
                string css;
                string attributes = "";
                if (field.MulitLine)
                {
                    br = "<br/>";
                    css = field.FormCloumnSapn <= 1 ? "inputValue_Memo_S" : "inputValue_Memo";
                }
                else
                {
                    css = "inputValue" + cssEnd + " inputS";
                    if (field.IsMoney)
                    {
                        wid = 120;
                        if (field.FormCloumnSapn > 2)
                            wid += (field.FormCloumnSapn - 1) * 485;
                    }
                    else
                    {
                        int len = 0;
                        if (!string.IsNullOrWhiteSpace(field.Prefix))
                            len = field.Prefix.Length;
                        if (!string.IsNullOrWhiteSpace(field.Suffix))
                            len += field.Suffix.Length;

                        if (len > 0)
                        {
                            if (field.FormCloumnSapn > 2)
                                wid = (field.FormCloumnSapn - 1) * 485;
                            else
                                wid = 480;
                            wid -= len * 12;
                        }
                    }
                    if (wid > 0)
                        attributes += $" style='width:{wid}px'";
                    if (field.IsLinkKey)
                    {
                        var title = field.Parent.Properties.FirstOrDefault(p => p.LinkTable == field.LinkTable && p.IsLinkCaption);
                        if (title != null)
                        {
                            attributes += $" readfield='{title.Name}'";
                        }
                    }
                }


                var options = FieldOptions(field, description);
                code.Append($@"{br}
                    <input id='{field.Name}' name='{field.Name}' class='{css} {field.InputType}'{attributes}
                           data-options=""{options}""/>");
                if (!string.IsNullOrWhiteSpace(field.Suffix))
                    code.Append(field.Suffix);
                if (field.IsMoney)
                {
                    code.Append($@"<label id = 'cm_{field.Name}' style = 'color: red;' ></label>");
                }
            }
            code.Append(@"
                </div>
            </div>");
            //code.Append(
            //    $@"
            //    </div>
            //    <div class='inputHelp' id='hr_{field.Name}'>{
            //        description}</div>
            //</div>");
        }

        private static string FieldOptions(PropertyConfig field, string description)
        {
            StringBuilder options = new StringBuilder();
            options.Append($"prompt:'{description.Replace('\'', ' ')}'");
            if (field.MulitLine)
            {
                options.Append(",multiline:true");
            }
            if (field.IsUserReadOnly)
            {
                options.Append(",readonly:true");
            }
            if (!field.CanEmpty)
            {
                options.Append(",required:true");
            }
            if (!string.IsNullOrWhiteSpace(field.FormOption))
            {
                options.Append("," + field.FormOption);
            }
            if (!string.IsNullOrWhiteSpace(field.ComboBoxUrl))
            {
                options.Append($",url:'{field.ComboBoxUrl}'");
            }
            bool required;
            var validType = EasyUiPageScriptCoder.ValidType(field, out required);
            if (validType.Count > 0)
            {
                options.Append($",validType:[{validType.LinkToString(",")}]");
            }
            return options.ToString();
        }
    }
}