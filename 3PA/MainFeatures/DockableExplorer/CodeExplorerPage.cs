﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Controls;
using YamuiFramework.Fonts;
using YamuiFramework.Themes;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace _3PA.MainFeatures.DockableExplorer {
    public partial class CodeExplorerPage : YamuiPage {

        #region fields

        private const string EmptyListString = "Nothing to display!";

        /// <summary>
        /// Tracks toggle state of the expand/collapse
        /// </summary>
        private bool _isExpanded;

        /// <summary>
        /// tracks if we want to display the "normal" list, with folders and stuff, or the
        /// unsorted list, which is the list in code order
        /// </summary>
        private bool _displayUnSorted;

        private string _filterText;

        /// <summary>
        /// Use alternative back color... or not
        /// </summary>
        public bool UseAlternativeBackColor {
            set { ovlTree.UseAlternatingBackColors = value; }
        }

        private Dictionary<string, bool> _expandedBranches = new Dictionary<string, bool>();
        private int _topItemIndex;

        // list of items to display in the tree
        private static List<CodeExplorerItem> _unsortedItems = new List<CodeExplorerItem>();
        private static List<CodeExplorerItem> _rootItems = new List<CodeExplorerItem>();
        private static List<CodeExplorerItem> _items = new List<CodeExplorerItem>();

        // holds the display order of the ExplorerType
        private static List<int> _explorerBranchTypePriority;

        /// <summary>
        /// returns the ranking of each ExplorerType, helps sorting them as we wish
        /// </summary>
        public static List<int> GetPriorityList {
            get {
                if (_explorerBranchTypePriority != null) return _explorerBranchTypePriority;
                _explorerBranchTypePriority = new List<int>();
                var temp = Config.Instance.CodeExplorerPriorityList.Split(',').Select(Int32.Parse).ToList();
                for (int i = 0; i < Enum.GetNames(typeof(CompletionType)).Length; i++)
                    _explorerBranchTypePriority.Add(temp.IndexOf(i));
                return _explorerBranchTypePriority;
            }
        }

        #endregion

        #region constructor

        public CodeExplorerPage() {
            InitializeComponent();

            // Can the given object be expanded?
            ovlTree.CanExpandGetter = x => ((CodeExplorerItem)x).HasChildren;

            // What objects should belong underneath the given model object?
            ovlTree.ChildrenGetter = delegate(object x) {
                var obj = (CodeExplorerItem) x;
                return (obj != null && obj.HasChildren) ? obj.Items : null;
            };

            // Image getter
            DisplayText.ImageGetter += ImageGetter;
            
            // Style the control
            StyleOvlTree();

            buttonCleanText.BackGrndImage = ImageResources.eraser;
            buttonExpandRetract.BackGrndImage = ImageResources.collapse;
            buttonRefresh.BackGrndImage = ImageResources.refresh;
            buttonSort.BackGrndImage = ImageResources.numerical_sorting_12;
            _isExpanded = true;

            // Register to events
            buttonCleanText.ButtonPressed += buttonCleanText_Click;
            buttonExpandRetract.ButtonPressed += buttonExpandRetract_Click;
            textBoxFilter.TextChanged += textBoxFilter_TextChanged;
            buttonRefresh.ButtonPressed += buttonRefresh_Click;
            buttonSort.ButtonPressed += buttonSort_Click;
            ovlTree.Click += OvlTreeOnClick;
            ovlTree.KeyDown += OvlTreeOnKeyDown;

            // decorate rows
            ovlTree.UseCellFormatEvents = true;
            ovlTree.FormatCell += FastOlvOnFormatCell;

            // tooltips
            toolTipHtml.SetToolTip(buttonExpandRetract, "Toggle <b>Expand/Collapse</b>");
            toolTipHtml.SetToolTip(buttonCleanText, "<b>Clean</b> the current text filter");
            toolTipHtml.SetToolTip(buttonRefresh, "Click to <b>Refresh</b> the tree");
            toolTipHtml.SetToolTip(buttonSort, "Toggle <b>Categories/Code order sorting</b>");

            // problems with the width of the column, set here
            DisplayText.Width = ovlTree.Width - 17;
            ovlTree.SizeChanged += (sender, args) => {
                DisplayText.Width = ovlTree.Width - 17;
                ovlTree.Invalidate();
            };
        }

        #endregion

        #region cell formatting
        /// <summary>
        /// Image getter for object rows
        /// </summary>
        /// <param name="rowObject"></param>
        /// <returns></returns>
        private static object ImageGetter(object rowObject) {
            var obj = (CodeExplorerItem)rowObject;
            if (obj == null) return ImageResources.Error;
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject((obj.IconType > 0) ? obj.IconType.ToString() : obj.Branch.ToString());
            return tryImg ?? ImageResources.Error;
        }

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void FastOlvOnFormatCell(object sender, FormatCellEventArgs args) {
            CodeExplorerItem obj = (CodeExplorerItem) args.Model;

            // currently selected block
            if (!obj.IsNotBlock && obj.DisplayText.EqualsCi(ParserHandler.GetCarretLineOwnerName)) {
                RowBorderDecoration rbd = new RowBorderDecoration {
                    FillBrush = new SolidBrush(Color.FromArgb(50, ThemeManager.Current.AutoCompletionFocusBackColor)),
                    BorderPen = new Pen(Color.FromArgb(128, ThemeManager.Current.AutoCompletionFocusForeColor), 1),
                    BoundsPadding = new Size(-2, 0),
                    CornerRounding = 6.0f
                };
                args.SubItem.Decoration = rbd;
            }

            // display the flags
            int offset = -5;
            foreach (var name in Enum.GetNames(typeof(CodeExplorerFlag))) {
                CodeExplorerFlag flag = (CodeExplorerFlag)Enum.Parse(typeof(CodeExplorerFlag), name);
                if (flag == 0) continue;
                if (!obj.Flag.HasFlag(flag)) continue;
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                if (tryImg == null) continue;
                ImageDecoration decoration = new ImageDecoration(tryImg, 100, ContentAlignment.MiddleRight) {
                    Offset = new Size(offset, 0)
                };
                if (args.SubItem.Decoration == null)
                    args.SubItem.Decoration = decoration;
                else
                    args.SubItem.Decorations.Add(decoration);
                offset -= 20;
            }

            // display the sub string
            if (offset < -5) offset -= 5;
            if (!string.IsNullOrEmpty(obj.SubString)) {
                TextDecoration decoration = new TextDecoration(obj.SubString, 100) {
                    Alignment = ContentAlignment.MiddleRight,
                    Offset = new Size(offset, 0),
                    Font = FontManager.GetFont(FontStyle.Bold, 11),
                    TextColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor,
                    CornerRounding = 1f,
                    Rotation = 0,
                    BorderWidth = 1,
                    BorderColor = ThemeManager.Current.AutoCompletionNormalSubTypeForeColor
                };
                args.SubItem.Decorations.Add(decoration);
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Used when the user update the positon of the carret, to reflect the current location with the mouse
        /// </summary>
        public void Redraw() {
            ovlTree.Invalidate();
        }

        /// <summary>
        /// This method uses the items found by the parser to update the code explorer tree
        /// </summary>
        public void UpdateTreeData(bool forceUpdate = false) {
            // Fetch items found by the parser
            var prevUnsortedItems = _unsortedItems.ToList();
            _unsortedItems = ParserHandler.GetParsedExplorerItemsList();

            // we set an index for each item, this will be useful to find the updated GoToLine when we click on an item
            int iIndex = 0;
            _unsortedItems.ForEach(item => item.Index = iIndex++);

            // dont update the tree if every item is stricly the same..
            if (!forceUpdate && prevUnsortedItems.Count == _unsortedItems.Count) {
                bool allEquals = true;
                for (int i = 0; i < _unsortedItems.Count; i++) {
                    if (!prevUnsortedItems[i].DisplayText.Equals(_unsortedItems[i].DisplayText)) allEquals = false;
                }
                if (allEquals) return;
            }

            _items = ParserHandler.GetParsedExplorerItemsList();
            _items.Sort(new ExplorerObjectSortingClass());

            // init branches
            _rootItems.Clear();

            if (_displayUnSorted) {
                _rootItems = new List<CodeExplorerItem>() {
                    new CodeExplorerItem() {
                        DisplayText = "Everything in code order",
                        Branch = CodeExplorerBranch.EverythingInCodeOrder,
                        HasChildren = true
                    }
                };
            } else {
                // add root items first
                foreach (var item in _items.Where(item => item.Level == 0)) {
                    _rootItems.Add(item);
                }

                // for each distinct type of items, create a branch (if the branchType isn't already in the root list!)
                foreach (var type in _items.Select(x => x.Branch).Distinct()) {
                    if (_rootItems.Find(item => item.Branch == type) != null) continue;
                    _rootItems.Add(new CodeExplorerItem() {
                        DisplayText = ((CodeExplorerTypeAttr)type.GetAttributes()).DisplayText,
                        Branch = type,
                        HasChildren = true,
                        Level = 0
                    });
                }

                // For each duplicated item (same branchType and same displayText), we create a new branch
                var iItem = 0;
                while (iItem < _items.Count) {

                    var iIdentical = iItem + 1;
                    CodeExplorerFlag flags = 0;

                    // while we match identical items
                    while (iIdentical < _items.Count 
                        && _items[iItem].Branch == _items[iIdentical].Branch
                        && _items[iItem].IconType == _items[iIdentical].IconType
                        && _items[iItem].DisplayText.EqualsCi(_items[iIdentical].DisplayText)) {
                        _items[iIdentical].Level = 2;
                        flags = flags | _items[iIdentical].Flag;
                        iIdentical++;
                    }
                    // if we found identical item, we create a branch for them
                    if (iIdentical > iItem + 1) {
                        _items[iItem].Level = 2;
                        _items.Insert(iItem, new CodeExplorerItem() {
                            DisplayText = _items[iItem].DisplayText,
                            Branch = _items[iItem].Branch,
                            IconType = _items[iItem].IconType,
                            HasChildren = true,
                            SubString = "x" + (iIdentical - iItem),
                            Level = 1,
                            IsNotBlock = _items[iItem].IsNotBlock,
                            Flag = flags
                        });
                        iItem = iIdentical + 1;
                    } else
                        iItem++;
                }

                // sort root items
                _rootItems = _rootItems.OrderBy(item => GetPriorityList[(int)item.Branch]).ToList();           
            }

            RememberExpandedItems();
            ovlTree.Roots = _rootItems;
            ovlTree.RefreshObjects(_rootItems);
            SetRememberedExpandedItems();
            ReapplyFilter();
        }

        /// <summary>
        /// Call this before updating the list of items to remember which branch is expanded
        /// </summary>
        public void RememberExpandedItems() {
            foreach (var root in ovlTree.Roots) {
                var branch = root as CodeExplorerItem;
                if (branch == null || !branch.HasChildren) continue;
                if (!_expandedBranches.ContainsKey(branch.DisplayText))
                    _expandedBranches.Add(branch.DisplayText, ovlTree.IsExpanded(root));
                else
                    _expandedBranches[branch.DisplayText] = ovlTree.IsExpanded(root);
            }

            _topItemIndex = ovlTree.TopItemIndex;
        }

        /// <summary>
        /// Call this after updating the list of items to set the remembered expanded branches
        /// </summary>
        public void SetRememberedExpandedItems() {
            foreach (var root in ovlTree.Roots) {
                var branch = root as CodeExplorerItem;
                if (branch == null || !branch.HasChildren) continue;
                if (_expandedBranches.ContainsKey(branch.DisplayText)) {
                    if (_expandedBranches[branch.DisplayText])
                        ovlTree.Expand(root);
                    else
                        ovlTree.Collapse(root);
                } else {
                    _expandedBranches.Add(branch.DisplayText, true);
                    ovlTree.Expand(root);
                }
            }

            ovlTree.TopItemIndex = _topItemIndex > 0 ? Math.Min(_topItemIndex, ovlTree.FilteredObjects.OfType<CodeExplorerItem>().Count()) : 0;
        }

        /// <summary>
        /// Static method used by items to return the list of their children
        /// </summary>
        /// <returns></returns>
        public static List<CodeExplorerItem> GetItemsFor(CodeExplorerItem item) {
            if (item.Branch == CodeExplorerBranch.EverythingInCodeOrder)
                return _unsortedItems;

            // returns the list of children items
            switch (item.Level) {
                case 0:
                    return _items.Where(expItem => expItem.Branch == item.Branch && expItem.Level == 1).ToList();
                case 1:
                    return _items.Where(expItem => expItem.Branch == item.Branch && expItem.Level == 2 && expItem.DisplayText.EqualsCi(item.DisplayText)).ToList();
            }

            return new List<CodeExplorerItem>();
        }

        /// <summary>
        /// Apply thememanager theme to the treeview
        /// </summary>
        public void StyleOvlTree() {
            // Style the control
            ovlTree.OwnerDraw = true;
            ovlTree.Font = FontManager.GetLabelFont(LabelFunction.AutoCompletion);
            ovlTree.BackColor = ThemeManager.Current.AutoCompletionNormalBackColor;
            ovlTree.AlternateRowBackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
            ovlTree.ForeColor = ThemeManager.Current.AutoCompletionNormalForeColor;
            ovlTree.HighlightBackgroundColor = ThemeManager.Current.AutoCompletionFocusBackColor;
            ovlTree.HighlightForegroundColor = ThemeManager.Current.AutoCompletionFocusForeColor;
            ovlTree.UnfocusedHighlightBackgroundColor = ovlTree.HighlightBackgroundColor;
            ovlTree.UnfocusedHighlightForegroundColor = ovlTree.HighlightForegroundColor;

            // Decorate and configure hot item
            ovlTree.UseHotItem = true;
            ovlTree.HotItemStyle = new HotItemStyle {
                BackColor = ThemeManager.Current.AutoCompletionHoverBackColor,
                ForeColor = ThemeManager.Current.AutoCompletionHoverForeColor
            };

            // overlay of empty list :
            ovlTree.EmptyListMsg = EmptyListString;
            TextOverlay textOverlay = ovlTree.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BackColor = ThemeManager.Current.AutoCompletionNormalAlternateBackColor;
                textOverlay.BorderColor = ThemeManager.Current.AutoCompletionNormalForeColor;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }

            CleanFilter();
        }
        #endregion

        #region events
        /// <summary>
        /// On key down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool OnKeyDown(Keys key) {
            bool handled = true;
            // down and up change the selection
            if (key == Keys.Up) {
                if (ovlTree.SelectedIndex > 0)
                    ovlTree.SelectedIndex--;
                else
                    ovlTree.SelectedIndex = (ovlTree.FilteredObjects.OfType<CodeExplorerItem>().Count() - 1);
                ovlTree.EnsureVisible(ovlTree.SelectedIndex);
            } else if (key == Keys.Down) {
                if (ovlTree.SelectedIndex < (ovlTree.FilteredObjects.OfType<CodeExplorerItem>().Count() - 1))
                    ovlTree.SelectedIndex++;
                else
                    ovlTree.SelectedIndex = 0;
                ovlTree.EnsureVisible(ovlTree.SelectedIndex);

                // enter and tab accept the current selection
            } else if (key == Keys.Enter) {
                ActivateSelection();

                // else, any other key needs to be analysed by Npp
            } else {
                handled = false;
            }
            return handled;
        }

        /// <summary>
        /// When the user double click an item on the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OvlTreeOnClick(object sender, EventArgs eventArgs) {
            ActivateSelection();
        }

        /// <summary>
        /// Activates the current selection
        /// </summary>
        private void ActivateSelection() {
            // find currently selected item
            var selection = (CodeExplorerItem)ovlTree.SelectedObject;
            if (selection == null) return;
            // Branch clicked : expand/retract
            if (selection.HasChildren) {
                if (ovlTree.IsExpanded(selection))
                    ovlTree.Collapse(selection);
                else
                    ovlTree.Expand(selection);
                Npp.GrabFocus();
            } else {
                // Item clicked : go to line
                var realSelection = _unsortedItems.Find(item => item.Index == selection.Index);
                Npp.GoToLine(realSelection.GoToLine);
                ovlTree.Invalidate();
            }
        }

        private void OvlTreeOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            OnKeyDown(keyEventArgs.KeyCode);
            ovlTree.RefreshSelectedObjects();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e) {
            // first char input? we remember the expanded/retracted branches
            if (!string.IsNullOrWhiteSpace(textBoxFilter.Text) && textBoxFilter.Text.Length == 1) {
                RememberExpandedItems();
                ovlTree.ExpandAll();
            }

            ApplyFilter();
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            CleanFilter();
            _unsortedItems.Clear();
            AutoComplete.ParseCurrentDocument(true);
            Npp.GrabFocus();
        }

        private void buttonSort_Click(object sender, EventArgs e) {
            _displayUnSorted = !_displayUnSorted;
            CleanFilter();
            UpdateTreeData(true);
            buttonSort.BackGrndImage = _displayUnSorted ? ImageResources.clear_filters : ImageResources.numerical_sorting_12;
            buttonSort.Invalidate();
            Npp.GrabFocus();
        }

        private void buttonCleanText_Click(object sender, EventArgs e) {
            CleanFilter();
            textBoxFilter.Invalidate();
            Npp.GrabFocus();
        }

        private void buttonExpandRetract_Click(object sender, EventArgs e) {
            _isExpanded = !_isExpanded;
            if (_isExpanded)
                ovlTree.ExpandAll();
            else
                ovlTree.CollapseAll();
            buttonExpandRetract.BackGrndImage = _isExpanded ? ImageResources.collapse : ImageResources.expand;
            buttonExpandRetract.Invalidate();
            Npp.GrabFocus();
        }

        
        #endregion

        #region filter

        public void ReapplyFilter() {
            if (string.IsNullOrWhiteSpace(textBoxFilter.Text)) return;
            string curFilter = textBoxFilter.Text;
            CleanFilter();
            textBoxFilter.Text = curFilter;
        }

        /// <summary>
        /// apply text filter (from textbox)
        /// </summary>
        private void ApplyFilter() {
            if (string.IsNullOrWhiteSpace(textBoxFilter.Text)) {
                CleanFilter();
                return;
            }

            // filter the tree..
            _filterText = textBoxFilter.Text.ToLower();
            ovlTree.ModelFilter = new ModelFilter(FilterPredicate);
            ovlTree.TreeColumnRenderer = new CustomTreeRenderer(_filterText);
        }

        /// <summary>
        /// Filter predicate
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool FilterPredicate(object o) {
            var obj = (CodeExplorerItem) o;
            return (!obj.HasChildren && obj.DisplayText.ToLower().FullyMatchFilter(_filterText));
        }

        /// <summary>
        /// Update the renderer (the filter)
        /// </summary>
        private void CleanFilter() {
            ovlTree.TreeColumnRenderer = new CustomTreeRenderer("");
            textBoxFilter.Text = "";
            ovlTree.ModelFilter = null;
            SetRememberedExpandedItems();
        }
        #endregion
    }

    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    public class ExplorerObjectSortingClass : IComparer<CodeExplorerItem> {
        public int Compare(CodeExplorerItem x, CodeExplorerItem y) {
            // compare first by BranchType
            int compare = x.Branch.CompareTo(y.Branch);
            if (compare != 0) return compare;
            // compare by type
            compare = x.IconType.CompareTo(y.IconType);
            if (compare != 0) return compare;
            // sort by display text
            compare = string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
            if (compare != 0) return compare;
            return x.GoToLine.CompareTo(y.GoToLine);
        }
    }
}