using System;
namespace BLEP.Droid.ListView
{
    public class DataItem : IListItem
    {
        public DataItem(string title, string subtitle)
        {
            Text = title;
            SubTitle = subtitle;
        }

        public string SubTitle { get; }

        public string Text { get; set; }

        public ListItemType GetListItemType()
        {
            return ListItemType.DataItem;
        }
    }

    public class StatusHeaderListItem : IListItem
    {
        public StatusHeaderListItem(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public ListItemType GetListItemType()
        {
            return ListItemType.Status;
        }
    }

    public class HeaderListItem : IListItem
    {
        public HeaderListItem(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public ListItemType GetListItemType()
        {
            return ListItemType.Header;
        }
    }
}
