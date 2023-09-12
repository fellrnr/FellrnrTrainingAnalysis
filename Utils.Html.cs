using System.Text;


//From https://stackoverflow.com/questions/36475679/dynamically-create-html-table-in-c-sharp
namespace FellrnrTrainingAnalysis.Utils
{
    public static class Html
    {
        public class Table : HtmlBase, IDisposable
        {
            public Table(StringBuilder sb, string tags = "", string id = "") : base(sb)
            {
                Append("<table");
                AddOptionalAttributes(tags, id);
            }

            public void StartHead(string tags = "", string id = "")
            {
                Append("<thead");
                AddOptionalAttributes(tags, id);
            }

            public void EndHead()
            {
                Append("</thead>");
            }

            public void StartFoot(string tags = "", string id = "")
            {
                Append("<tfoot");
                AddOptionalAttributes(tags, id);
            }

            public void EndFoot()
            {
                Append("</tfoot>");
            }

            public void StartBody(string tags = "", string id = "")
            {
                Append("<tbody");
                AddOptionalAttributes(tags, id);
            }

            public void EndBody()
            {
                Append("</tbody>");
            }

            public void Dispose()
            {
                Append("</table>");
            }

            public Row AddRow(string tags = "", string id = "")
            {
                return new Row(GetBuilder(), tags, id);
            }
        }

        public class Row : HtmlBase, IDisposable
        {
            public Row(StringBuilder sb, string tags = "", string id = "") : base(sb)
            {
                Append("<tr");
                AddOptionalAttributes(tags, id);
            }
            public void Dispose()
            {
                Append("</tr>");
            }
            public void AddCell(string innerText, string tags = "", string id = "", string colSpan = "")
            {
                Append("<td");
                AddOptionalAttributes(tags, id, colSpan);
                Append(innerText);
                Append("</td>");
            }
        }

        public abstract class HtmlBase
        {
            private StringBuilder _sb;

            protected HtmlBase(StringBuilder sb)
            {
                _sb = sb;
            }

            public StringBuilder GetBuilder()
            {
                return _sb;
            }

            protected void Append(string toAppend)
            {
                _sb.Append(toAppend);
            }

            protected void AddOptionalAttributes(string tags = "", string id = "", string colSpan = "")
            {

                if (!string.IsNullOrEmpty(id))
                {
                    _sb.Append($" id=\"{id}\"");
                }
                if (!string.IsNullOrEmpty(tags))
                {
                    _sb.Append($" {tags}");
                }
                if (!string.IsNullOrEmpty(colSpan))
                {
                    _sb.Append($" colspan=\"{colSpan}\"");
                }
                _sb.Append(">");
            }
        }
    }
}
