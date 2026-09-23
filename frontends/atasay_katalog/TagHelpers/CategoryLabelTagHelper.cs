using Microsoft.AspNetCore.Razor.TagHelpers;

namespace atasay_katalog.TagHelpers;

[HtmlTargetElement("category-label")]
public sealed class CategoryLabelTagHelper : TagHelper
{
    public string Name { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        if (string.Equals(Name.Trim(), "SET", StringComparison.OrdinalIgnoreCase))
        {
            output.Attributes.SetAttribute("translate", "no");
            output.Attributes.SetAttribute("class", "notranslate");
            output.Content.SetContent("SET");
        }
        else output.Content.SetContent(Name);
    }
}
