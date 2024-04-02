﻿#pragma checksum "C:\Users\nicol\OneDrive\Documents\GitHub\DropStack\DropStackWinUI\HelperWindows\CameraScanner.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5F0B6566F14A290FEFB2B7A724136B5955E3E4203AFAD1DED4CB79E5C9428876"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DropStackWinUI.HelperWindows
{
    partial class CameraScanner : 
        global::Microsoft.UI.Xaml.Window, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2403")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Microsoft_UI_Xaml_Media_Animation_DoubleAnimation_To(global::Microsoft.UI.Xaml.Media.Animation.DoubleAnimation obj, global::System.Nullable<global::System.Double> value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Double) global::Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Double), targetNullValue);
                }
                obj.To = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2403")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class CameraScanner_obj1_Bindings :
            global::Microsoft.UI.Xaml.Markup.IDataTemplateComponent,
            global::Microsoft.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Microsoft.UI.Xaml.Markup.IComponentConnector,
            ICameraScanner_Bindings
        {
            private global::DropStackWinUI.HelperWindows.CameraScanner dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Microsoft.UI.Xaml.Media.Animation.DoubleAnimation obj3;
            private global::Microsoft.UI.Xaml.Media.Animation.DoubleAnimation obj4;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj3ToDisabled = false;
            private static bool isobj4ToDisabled = false;

            private CameraScanner_obj1_BindingsTracking bindingsTracking;

            public CameraScanner_obj1_Bindings()
            {
                this.bindingsTracking = new CameraScanner_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 31 && columnNumber == 21)
                {
                    isobj3ToDisabled = true;
                }
                else if (lineNumber == 37 && columnNumber == 21)
                {
                    isobj4ToDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 3: // HelperWindows\CameraScanner.xaml line 27
                        this.obj3 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Media.Animation.DoubleAnimation>(target);
                        break;
                    case 4: // HelperWindows\CameraScanner.xaml line 33
                        this.obj4 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Media.Animation.DoubleAnimation>(target);
                        break;
                    default:
                        break;
                }
            }
                        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2403")]
                        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
                        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target) 
                        {
                            return null;
                        }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
            }

            public void Recycle()
            {
                return;
            }

            // ICameraScanner_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = global::WinRT.CastExtensions.As<global::DropStackWinUI.HelperWindows.CameraScanner>(newDataRoot);
                    return true;
                }
                return false;
            }

            public void Activated(object obj, global::Microsoft.UI.Xaml.WindowActivatedEventArgs data)
            {
                this.Initialize();
            }

            public void Loading(global::Microsoft.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::DropStackWinUI.HelperWindows.CameraScanner obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_CameraScannerViewModel(obj.CameraScannerViewModel, phase);
                    }
                }
            }
            private void Update_CameraScannerViewModel(global::DropStackWinUI.HelperWindows.ViewModel obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_CameraScannerViewModel(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_CameraScannerViewModel_AnimationTargetXPosition(obj.AnimationTargetXPosition, phase);
                        this.Update_CameraScannerViewModel_AnimationTargetYPosition(obj.AnimationTargetYPosition, phase);
                    }
                }
            }
            private void Update_CameraScannerViewModel_AnimationTargetXPosition(global::System.Double obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // HelperWindows\CameraScanner.xaml line 27
                    if (!isobj3ToDisabled)
                    {
                        XamlBindingSetters.Set_Microsoft_UI_Xaml_Media_Animation_DoubleAnimation_To(this.obj3, obj, null);
                    }
                }
            }
            private void Update_CameraScannerViewModel_AnimationTargetYPosition(global::System.Double obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // HelperWindows\CameraScanner.xaml line 33
                    if (!isobj4ToDisabled)
                    {
                        XamlBindingSetters.Set_Microsoft_UI_Xaml_Media_Animation_DoubleAnimation_To(this.obj4, obj, null);
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2403")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class CameraScanner_obj1_BindingsTracking
            {
                private global::System.WeakReference<CameraScanner_obj1_Bindings> weakRefToBindingObj; 

                public CameraScanner_obj1_BindingsTracking(CameraScanner_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<CameraScanner_obj1_Bindings>(obj);
                }

                public CameraScanner_obj1_Bindings TryGetBindingObject()
                {
                    CameraScanner_obj1_Bindings bindingObject = null;
                    if (weakRefToBindingObj != null)
                    {
                        weakRefToBindingObj.TryGetTarget(out bindingObject);
                        if (bindingObject == null)
                        {
                            weakRefToBindingObj = null;
                            ReleaseAllListeners();
                        }
                    }
                    return bindingObject;
                }

                public void ReleaseAllListeners()
                {
                    UpdateChildListeners_CameraScannerViewModel(null);
                }

                public void PropertyChanged_CameraScannerViewModel(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    CameraScanner_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::DropStackWinUI.HelperWindows.ViewModel obj = sender as global::DropStackWinUI.HelperWindows.ViewModel;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                            if (obj != null)
                            {
                                bindings.Update_CameraScannerViewModel_AnimationTargetXPosition(obj.AnimationTargetXPosition, DATA_CHANGED);
                                bindings.Update_CameraScannerViewModel_AnimationTargetYPosition(obj.AnimationTargetYPosition, DATA_CHANGED);
                            }
                        }
                        else
                        {
                            switch (propName)
                            {
                                case "AnimationTargetXPosition":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_CameraScannerViewModel_AnimationTargetXPosition(obj.AnimationTargetXPosition, DATA_CHANGED);
                                    }
                                    break;
                                }
                                case "AnimationTargetYPosition":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_CameraScannerViewModel_AnimationTargetYPosition(obj.AnimationTargetYPosition, DATA_CHANGED);
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                    }
                }
                private global::DropStackWinUI.HelperWindows.ViewModel cache_CameraScannerViewModel = null;
                public void UpdateChildListeners_CameraScannerViewModel(global::DropStackWinUI.HelperWindows.ViewModel obj)
                {
                    if (obj != cache_CameraScannerViewModel)
                    {
                        if (cache_CameraScannerViewModel != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)cache_CameraScannerViewModel).PropertyChanged -= PropertyChanged_CameraScannerViewModel;
                            cache_CameraScannerViewModel = null;
                        }
                        if (obj != null)
                        {
                            cache_CameraScannerViewModel = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_CameraScannerViewModel;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2403")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // HelperWindows\CameraScanner.xaml line 26
                {
                    this.CameraFeedToGalleryAnimation = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Media.Animation.Storyboard>(target);
                }
                break;
            case 5: // HelperWindows\CameraScanner.xaml line 44
                {
                    this.TitleBarRectangle = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Shapes.Rectangle>(target);
                }
                break;
            case 6: // HelperWindows\CameraScanner.xaml line 69
                {
                    this.GalleryGridView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.GridView>(target);
                }
                break;
            case 9: // HelperWindows\CameraScanner.xaml line 61
                {
                    this.BtnInitCamera = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.BtnInitCamera).Click += this.InitCamera_Click;
                }
                break;
            case 10: // HelperWindows\CameraScanner.xaml line 62
                {
                    this.BtnStartPreview = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.BtnStartPreview).Click += this.StartCapturePreview_Click;
                }
                break;
            case 11: // HelperWindows\CameraScanner.xaml line 63
                {
                    this.BtnCapturePhoto = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.BtnCapturePhoto).Click += this.CapturePhoto_Click;
                }
                break;
            case 12: // HelperWindows\CameraScanner.xaml line 64
                {
                    this.BtnStopPreview = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.BtnStopPreview).Click += this.StopCapturePreview_Click;
                }
                break;
            case 13: // HelperWindows\CameraScanner.xaml line 50
                {
                    this.imagePreview = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Image>(target);
                }
                break;
            case 14: // HelperWindows\CameraScanner.xaml line 53
                {
                    this.animationPreview = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Image>(target);
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2403")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // HelperWindows\CameraScanner.xaml line 2
                {                    
                    global::Microsoft.UI.Xaml.Window element1 = (global::Microsoft.UI.Xaml.Window)target;
                    CameraScanner_obj1_Bindings bindings = new CameraScanner_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Activated += bindings.Activated;
                }
                break;
            }
            return returnValue;
        }
    }
}

