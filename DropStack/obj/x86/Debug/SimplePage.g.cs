﻿#pragma checksum "C:\Users\nicol\OneDrive\Documents\GitHub\dropstack\DropStack\SimplePage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5C0FE9CCA7EC1F0061F453AA96CB4F7ABBAFC10CDAEEBB448FD576A3D2039454"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DropStack
{
    partial class SimplePage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 0.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // SimplePage.xaml line 18
                {
                    this.SimpleBackgroundRectangle = (global::Windows.UI.Xaml.Shapes.Rectangle)(target);
                }
                break;
            case 3: // SimplePage.xaml line 23
                {
                    this.simpleFileListScrollViewer = (global::Windows.UI.Xaml.Controls.ScrollViewer)(target);
                }
                break;
            case 4: // SimplePage.xaml line 77
                {
                    this.FileCommandBar = (global::Windows.UI.Xaml.Controls.CommandBar)(target);
                    ((global::Windows.UI.Xaml.Controls.CommandBar)this.FileCommandBar).PointerExited += this.FileCommandBar_PointerExited;
                    ((global::Windows.UI.Xaml.Controls.CommandBar)this.FileCommandBar).Closing += this.FileCommandBar_Closing;
                    ((global::Windows.UI.Xaml.Controls.CommandBar)this.FileCommandBar).Opening += this.FileCommandBar_Opening;
                }
                break;
            case 5: // SimplePage.xaml line 131
                {
                    this.CommandBarIndicatorPill = (global::Windows.UI.Xaml.Shapes.Rectangle)(target);
                }
                break;
            case 6: // SimplePage.xaml line 148
                {
                    this.CommandBarIndicatorPillHitbox = (global::Windows.UI.Xaml.Shapes.Rectangle)(target);
                    ((global::Windows.UI.Xaml.Shapes.Rectangle)this.CommandBarIndicatorPillHitbox).PointerEntered += this.CommandBarIndicatorPill_PointerEntered;
                }
                break;
            case 7: // SimplePage.xaml line 93
                {
                    this.CopyLastSelectedButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.CopyLastSelectedButton).Click += this.CopyLastSelectedButton_Click;
                }
                break;
            case 8: // SimplePage.xaml line 99
                {
                    this.RefreshButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.RefreshButton).Click += this.RefreshButton_Click;
                }
                break;
            case 9: // SimplePage.xaml line 106
                {
                    this.RevealInExplorerButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.RevealInExplorerButton).Click += this.RevealInExplorerButton_Click;
                }
                break;
            case 10: // SimplePage.xaml line 112
                {
                    this.CopyRecentFileButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.CopyRecentFileButton).Click += this.CopyRecentFileButton_Click;
                }
                break;
            case 11: // SimplePage.xaml line 117
                {
                    this.HideToolbarButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.HideToolbarButton).Click += this.HideToolbarButton_Click;
                }
                break;
            case 12: // SimplePage.xaml line 122
                {
                    this.MakeSimpleDefaultButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.MakeSimpleDefaultButton).Click += this.MakeSimpleDefaultButton_Click;
                }
                break;
            case 13: // SimplePage.xaml line 124
                {
                    this.CloseSimpleModeButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.CloseSimpleModeButton).Click += this.LoadMoreFromSimpleViewButton_Click;
                }
                break;
            case 14: // SimplePage.xaml line 28
                {
                    this.simpleFileListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    ((global::Windows.UI.Xaml.Controls.ListView)this.simpleFileListView).SelectionChanged += this.fileListView_SelectionChanged;
                    ((global::Windows.UI.Xaml.Controls.ListView)this.simpleFileListView).DoubleTapped += this.simpleFileListView_DoubleTapped;
                    ((global::Windows.UI.Xaml.Controls.ListView)this.simpleFileListView).RightTapped += this.fileListView_RightTapped;
                    ((global::Windows.UI.Xaml.Controls.ListView)this.simpleFileListView).DragItemsStarting += this.fileListView_DragItemsStarting;
                }
                break;
            case 15: // SimplePage.xaml line 65
                {
                    this.LoadMoreFromSimpleViewButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.LoadMoreFromSimpleViewButton).Click += this.LoadMoreFromSimpleViewButton_Click;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 0.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

