using IdentityServer.Web.Controllers;
using IdentityServer.Web.Enums;
using IdentityServer.Web.ViewModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.Encodings.Web;

namespace IdentityServer.Web.TagHelpers
{
    public class CustomAlertTagHelper : TagHelper
    {
        public string Message { get; set; }
        public string Priority { get; set; }
        public string ReturnUrl { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Message != string.Empty)
            {
                output.TagName = "div";

                output.AddClass("alert", HtmlEncoder.Default);

                switch (Priority)
                {
                    case "Success":
                        output.AddClass("alert-success", HtmlEncoder.Default);
                        break;
                    case "Information":
                        output.AddClass("alert-information", HtmlEncoder.Default);
                        break;
                    case "Danger":
                        output.AddClass("alert-danger", HtmlEncoder.Default);
                        break;
                    case "Warning":
                        output.AddClass("alert-warning", HtmlEncoder.Default);
                        break;
                    case "Secondary":
                        output.AddClass("alert-secondary", HtmlEncoder.Default);
                        break;
                    default:
                        output.AddClass("alert-primary", HtmlEncoder.Default);
                        break;

                }

                string htmlContent = $"<p> {Message} ";
                htmlContent += $"<a href='{ReturnUrl}' class='alert-link'>Eger sayfada geri donmediyseniz buraya tiklayiniz</a>";
                htmlContent += "</p>";
                htmlContent += "<script>";
                htmlContent += "setTimeout(() => {";
                htmlContent += $"window.location.href = '{ReturnUrl}'";
                htmlContent += "}, 5000)";
                htmlContent += "</script>";

                output.Content.SetHtmlContent(htmlContent);
            }
        }
    }
}
