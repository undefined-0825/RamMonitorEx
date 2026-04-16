namespace RamMonitorEx.Controls.RamMonitorView
{
    public class ValueCommentRow : IValueViewRow
    {
        public ValueRowType RowType => ValueRowType.Comment;
        public string CommentText { get; set; }

        public ValueCommentRow(string commentText)
        {
            CommentText = commentText;
        }
    }
}
