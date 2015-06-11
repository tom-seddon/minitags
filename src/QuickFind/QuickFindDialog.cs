using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

// <pre>
// http://www.developmentnow.com/g/30_2006_8_0_0_803770/listview-virtual-mode--how-to-ensure-visible.htm
// </pre>
//
// -- suggests handing out repeated references to a single ListViewItem is
// the best way to go. Speed seems acceptable current way so far though.

// TODO might be more convenient if dialog holds settings by reference, as every
// time it's been used the config comes from somewhere external and is saved
// afterwards... :/

namespace QuickFind
{
    public partial class QuickFindDialog : Form
    {
        private int CompareObjects(object a, object b)
        {
            string texta = _handler.GetColumnContents(a, _handler.SearchColumn);
            string textb = _handler.GetColumnContents(b, _handler.SearchColumn);

            return string.Compare(texta, textb, true);//true=ignore case
        }

        private static void MeasureString(Graphics graphics, Font font,
            int index, string text, Dictionary<string, int> stringWidths,
            int[] columnWidths, int[] columnStringLengths, ref int numMeasures)
        {
            // only bother if the string is longer than half the longest
            // (this is a guess of course)
            if (text.Length >= columnStringLengths[index] / 2)
            {
                int width;
                if (stringWidths == null ||
                    !stringWidths.TryGetValue(text, out width))
                {
                    SizeF size = graphics.MeasureString(text, font);

                    width = (int)size.Width;

                    if (stringWidths != null)
                        stringWidths[text] = width;

                    ++numMeasures;
                }

                columnWidths[index] = Math.Max(columnWidths[index], width);

                columnStringLengths[index] = Math.Max(columnStringLengths[index], text.Length);
            }
        }

        public QuickFindDialog(List<object> objects, QuickFindItemHandler handler)
            :
            this(objects, handler, null)
        {
        }

        public QuickFindDialog(List<object> objects, QuickFindItemHandler handler, QuickFindDialogSettings settings)
        {
            InitializeComponent();

            if (settings == null)
                this.Settings = new QuickFindDialogSettings();
            else
                this.Settings = settings.Clone();

            _tags = objects;

            _handler = handler;

            using (Timer tmr = new Timer("sort tag entries"))
                _tags.Sort(CompareObjects);

            _columns = new ColumnHeader[_handler.ColumnNames.Length];
            for (int i = 0; i < _handler.ColumnNames.Length; ++i)
                _columns[i] = _tagsListView.Columns.Add(_handler.ColumnNames[i]);

            _singleListViewItem = null;

            // If no column widths set, or it looks like the settings are
            // wrong for whatever reason, make a guess.
            if (this.Settings.ColumnWidths == null || this.Settings.ColumnWidths.Length != _columns.Length)
            {
                // TODO seems like
                // ColumnHeaderAutoResizeStyle.ColumnContent would be
                // ideal. I guess it didn't work with virtual list views or
                // something. Can't remember.

                _tagsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                // have to make a guess at the column widths...

                Dictionary<string, int> stringWidths = new Dictionary<string, int>();

                using (Timer tmr = new Timer("col widths"))
                {
                    this.Settings.ColumnWidths = new int[_columns.Length];

                    for (int i = 0; i < _columns.Length; ++i)
                        this.Settings.ColumnWidths[i] = _columns[i].Width;

                    int[] columnStringLengths = new int[_tagsListView.Columns.Count];

                    using (Graphics graphics = _tagsListView.CreateGraphics())
                    {
                        string[] subItems = new string[_tagsListView.Columns.Count];

                        int numMeasures = 0;

                        for (int i = 0; i < _tags.Count; ++i)
                        {
                            for (int j = 0; j < _columns.Length; ++j)
                            {
                                MeasureString(graphics, _tagsListView.Font, _columns[j].Index,
                                    _handler.GetColumnContents(_tags[i], j), stringWidths,
                                    this.Settings.ColumnWidths, columnStringLengths, ref numMeasures);
                            }
                        }

                        Trace.WriteLine(String.Format("{0} measures/{1} strings", numMeasures, _tags.Count * 4));
                    }

                    // seems that the column width isn't quite what it implies.
                    // even when set precisely, the item can end up displayed
                    // with ellipses. presumably there's some padding in there,
                    // or something, anyway, whatever, +10 works fine...
                    for (int i = 0; i < this.Settings.ColumnWidths.Length; ++i)
                        this.Settings.ColumnWidths[i] += 10;

                }
            }

            // Apply settings
            if (this.Settings.Placement != null)
            {
                // this is a bit dirty -- but the SetWindowPlacement call
                // will otherwise restore the visibility of the form, which
                // trips an exception in Form.ShowDialog. And calling
                // SetWindowPlacement then switching off the visibility
                // results in some ugly flicker.
                this.Settings.Placement.showCmd = 0;//SW_HIDE

                this.Settings.Placement.Set(this);
            }

            for (int i = 0; i < _columns.Length; ++i)
                _columns[i].Width = this.Settings.ColumnWidths[i];

            //using (Timer tmr = new Timer(" virtual mode"))
            {
                _tagsListView.VirtualListSize = 0;
                _tagsListView.ItemSelectionChanged += HandleItemSelectionChanged;
                _tagsListView.RetrieveVirtualItem += this.HandleRetrieveVirtualItem;
                _tagsListView.VirtualItemsSelectionRangeChanged +=
                    this.HandleVirtualItemsSelectionRangeChanged;
                _tagsListView.VirtualMode = true;
            }

            _editBox.Target = _tagsListView;

            _selectedTag = null;

            _matchingTagIdxs = null;

            RefreshList("");

            //             _tagsListView.Update();
            //             _tagsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            UpdateControls();
        }

        private void HandleVirtualItemsSelectionRangeChanged(object sender,
            ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            // doesn't actually seem necessary to handle this?
        }

        private void HandleItemSelectionChanged(object sender,
            ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                _selectedTag = e.Item.Tag;//_matchingItems[e.ItemIndex]
                _tagSelected = true;
            }

            UpdateControls();
        }

        private void HandleRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            object tag = _tags[_matchingTagIdxs[e.ItemIndex]];

            if (_singleListViewItem != null)
            {
                e.Item = _singleListViewItem;

                for (int i = 0; i < _columns.Length; ++i)
                    e.Item.SubItems[i].Text = _handler.GetColumnContents(tag, i);
            }
            else
            {
                e.Item = new ListViewItem(_handler.GetColumnContents(tag, 0));

                for (int i = 1; i < _columns.Length; ++i)
                    e.Item.SubItems.Add(_handler.GetColumnContents(tag, i));
            }

            e.Item.Tag = tag;
        }

        private void DownUp(Control target, Keys key)
        {
            Message m = new Message();

            m.Msg = (int)WM.WM_KEYDOWN;
            WindowsMessages.ForwardKeyMessageWithNewKey(_tagsListView, m, key);

            m.Msg = (int)WM.WM_KEYUP;
            WindowsMessages.ForwardKeyMessageWithNewKey(_tagsListView, m, key);
        }

        private void RefreshList(string spec)
        {
            //long refreshListStart = DateTime.Now.Ticks;

            string[] parts = spec.Split(' ');

            int selectedItemIdx = -1;

            _tagSelected = false;

            _tagsListView.VirtualListSize = 0;

            _matchingTagIdxs = new List<int>();

            for (int i = 0; i < _tags.Count; ++i)
            {
                object tag = _tags[i];
                string tagSearchStr = _handler.GetColumnContents(tag, _handler.SearchColumn);

                int idx = 0;
                foreach (string part in parts)
                {
                    idx = tagSearchStr.IndexOf(part, StringComparison.OrdinalIgnoreCase);
                    if (idx < 0)
                        break;
                }

                if (idx >= 0)
                {
                    if (object.ReferenceEquals(tag, _selectedTag))
                        selectedItemIdx = _matchingTagIdxs.Count;

                    _matchingTagIdxs.Add(i);
                }
            }

            _tagsListView.VirtualListSize = _matchingTagIdxs.Count;

            // if there's no selected tag, select the first item. this gets
            // things started.
            if (selectedItemIdx < 0)
                selectedItemIdx = 0;

            // seems to have to change the Selected property this way in
            // virtual mode.
            //
            // see ListView.ListViewNativeItemCollection.get_Item: it only
            // sets the item's index when the item is retrieved. then see
            // ListViewItem.set_Selection: it uses the index to change the
            // selectedness of the corresponding item in the actual list
            // view.
            if (selectedItemIdx >= 0 && selectedItemIdx < _tagsListView.VirtualListSize)
            {
                _tagsListView.EnsureVisible(selectedItemIdx);

                _tagsListView.Items[selectedItemIdx].Selected = true;
                _tagsListView.Items[selectedItemIdx].Focused = true;
            }

            long refreshListEnd = DateTime.Now.Ticks;

            UpdateControls();
        }

        private void UpdateControls()
        {
            _okButton.Enabled = _tagSelected;
        }

        private ColumnHeader[] _columns;
        private List<object> _tags;
        private List<int> _matchingTagIdxs;
        private object _selectedTag;
        private bool _tagSelected;
        private ListViewItem _singleListViewItem;
        private QuickFindItemHandler _handler;

        public object SelectedItem
        {
            get
            {
                return _selectedTag;
            }
        }

        private void _editBox_TextChanged(object sender, EventArgs e)
        {
            Timer.output = "";

            using (Timer tmr = new Timer("refresh"))
                RefreshList(_editBox.Text);

            this.Text = Timer.output;
        }

        private bool IsListViewKey(Keys key)
        {
            switch (key)
            {
            case Keys.Up:
            case Keys.Down:
            case Keys.PageUp:
            case Keys.PageDown:
                return true;

            default:
                return false;
            }
        }

        private void _cancelButton_Click(object sender, EventArgs e)
        {
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void _tagsListView_ItemActivate(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public readonly QuickFindDialogSettings Settings;

        private void QuickFindDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Settings.Placement = WindowPlacement.GetWindowPlacement(this);

            this.Settings.ColumnWidths = new int[_columns.Length];
            for (int i = 0; i < _columns.Length; ++i)
                this.Settings.ColumnWidths[i] = _columns[i].Width;
        }

        private void QuickFindDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }

    /// <summary>
    /// Serializable class for quick find dialog settings. Ignore or store off, whatever.
    /// </summary>
    public class QuickFindDialogSettings
    {
        public WindowPlacement Placement = null;

        public int[] ColumnWidths = null;

        public QuickFindDialogSettings Clone()
        {
            QuickFindDialogSettings clone = new QuickFindDialogSettings();

            clone.Placement = this.Placement;

            if (this.ColumnWidths == null)
                clone.ColumnWidths = null;
            else
                clone.ColumnWidths = this.ColumnWidths.Clone() as int[];

            return clone;
        }
    }
}