using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;

namespace HMIControl.VisualStudio.Design
{
    class ControlAdornerProvider : PrimarySelectionAdornerProvider
    {
        private AdornerPanel _panel = new AdornerPanel();
        LinkableControl designObject;
        ControlAdorner adorner;
        Panel designCanvas;
        LinkPin hitPin;
        bool isdrag = false;

        protected override void Activate(ModelItem item)
        {
            designObject = item.GetCurrentValue() as LinkableControl;//获得被点击的控件对象         

            if (designObject != null)
            {
                designCanvas = designObject.Parent as Panel;
                DesignerView v = DesignerView.FromContext(Context);
                adorner = new ControlAdorner(designObject);
                adorner.RenderTransform = new ScaleTransform(v.ZoomLevel, v.ZoomLevel);

                foreach (var pin in adorner.Children)
                {
                    pin.MouseLeftButtonDown += new MouseButtonEventHandler(Pin_MouseLeftButtonDown);//按下左键选中hitPin，开始拖动
                    pin.MouseMove += new MouseEventHandler(Pin_MouseMove);//移动鼠标，开始找寻目标连接节点
                }

                AdornerPanel.SetAdornerHorizontalAlignment(adorner, AdornerHorizontalAlignment.Stretch);
                AdornerPanel.SetAdornerVerticalAlignment(adorner, AdornerVerticalAlignment.Stretch);
                _panel.Children.Add(adorner);

                Adorners.Add(_panel);
                v.ZoomLevelChanged += (s, e) => { adorner.RenderTransform = new ScaleTransform(v.ZoomLevel, v.ZoomLevel); };
            }

            base.Activate(item);
        }

        protected override void Deactivate()
        {
            base.Deactivate();
        }

        void Pin_MouseLeftButtonDown(object s, MouseButtonEventArgs e)
        {
            hitPin = s as LinkPin; isdrag = true; e.Handled = true;
        }

        void Pin_MouseMove(object s, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) isdrag = false;

            if (isdrag && hitPin != null && designCanvas != null)
            {
                PinAdorner pinAdorner = new PinAdorner(designCanvas, hitPin);
                pinAdorner.MouseUp += new MouseButtonEventHandler(pinAdorner_MouseUp);
                DesignerView v = DesignerView.FromContext(Context);
                double zoom = v.ZoomLevel;
                pinAdorner.RenderTransform = new ScaleTransform(zoom, zoom);
                _panel.Children.Add(pinAdorner);
                e.Handled = true;
            }
        }

        static readonly Dictionary<ConnectOrientation, string> omap = new Dictionary<ConnectOrientation, string> 
        { { ConnectOrientation.Left, "LeftPin" }, { ConnectOrientation.Right, "RightPin" }, 
        { ConnectOrientation.Top, "TopPin" }, { ConnectOrientation.Bottom, "BottomPin" },{ ConnectOrientation.None, "" } };
        void pinAdorner_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var pinAdorner = sender as PinAdorner;
            if (pinAdorner.HitPin != null)
            {
                LinkableControl targetObject = pinAdorner.HitLinkableControl;
                LinkPin lp1 = pinAdorner.SourcePin;
                LinkPin lp2 = pinAdorner.HitPin;

                ConnectInfo info1 = lp1.GetInfo();
                ConnectInfo info2 = lp2.GetInfo();

                LinkLine line = new LinkLine(info1, info2);
                //Panel.SetZIndex(line, designCanvas.Children.Count);

                Binding bi1 = new Binding(omap[info1.Orient]);
                bi1.ElementName = designObject.Name;
                line.SetBinding(LinkLine.OriginInfoProperty, bi1);
                Binding bi2 = new Binding(omap[info2.Orient]);
                bi2.ElementName = targetObject.Name;
                line.SetBinding(LinkLine.TargetInfoProperty, bi2);

                var CanvasModelItem = AdornerProperties.GetModel(adorner).Parent;
                ModelItemCollection myControls = CanvasModelItem.Properties["Children"].Collection;
                ModelItem myLinkLine = myControls.Add(line);

            }
            if (pinAdorner.HitLinkableControl != null)
            {
                pinAdorner.HitLinkableControl.IsLinkDragOver = false;
            }

            hitPin = null;
            if (adorner.IsMouseCaptured) adorner.ReleaseMouseCapture();
            _panel.Children.Remove(pinAdorner);
        }

    }

    class LinkLineAdornerProvider : PrimarySelectionAdornerProvider
    {
        LinkLine myLinkLine;
        SolidColorBrush brush;

        protected override void Activate(ModelItem item)
        {
            myLinkLine = item.GetCurrentValue() as LinkLine;
            brush = myLinkLine.LineBrush;
            Panel.SetZIndex(myLinkLine, 1000);
            myLinkLine.LineBrush = Brushes.Yellow;
            base.Activate(item);
        }

        protected override void Deactivate()
        {
            myLinkLine.LineBrush = brush;
            Panel.SetZIndex(myLinkLine, 0);
            base.Deactivate();
        }
    }

    class TagComplexContextMenuProvider : PrimarySelectionContextMenuProvider
    {
        public TagComplexContextMenuProvider()
        {
            var lastFill = new MenuAction("Complex Editor");
            lastFill.Checkable = true;
            lastFill.Execute += (s, e) =>
                {
                    ModelItem item = e.Selection.PrimarySelection;
                    var tagReader = item.GetCurrentValue() as ITagReader;
                    TagComplexEditor frm = new TagComplexEditor(tagReader);
                    frm.ShowDialog();
                    string txt = frm.TagText;
                    if (!string.IsNullOrEmpty(txt)) { item.Properties["TagReadText"].SetValue(txt); }
                    else if (txt == string.Empty) item.Properties["TagReadText"].ClearValue();
                };

            Items.Add(lastFill);
        }
    }

    class TagWindowContextMenuProvider : PrimarySelectionContextMenuProvider
    {
        public TagWindowContextMenuProvider()
        {
            var lastFill = new MenuAction("Window Editor");
            lastFill.Checkable = true;
            lastFill.Execute += (s, e) =>
                {
                    ModelItem item = e.Selection.PrimarySelection;
                    ITagWindow tagReader = item.GetCurrentValue() as ITagWindow;
                    TagWindowEditor frm = new TagWindowEditor(tagReader);
                    frm.ShowDialog();
                    string txt = frm.GetText();
                    if (!string.IsNullOrEmpty(txt)) { item.Properties["TagWindowText"].SetValue(txt); }
                    else if (txt == string.Empty) item.Properties["TagWindowText"].ClearValue();
                    if (frm.IsModel)
                        item.Properties["IsModel"].SetValue(true);
                    else
                        item.Properties["IsModel"].ClearValue();
                    if (frm.IsUnique)
                        item.Properties["IsUnique"].SetValue(true);
                    else
                        item.Properties["IsUnique"].ClearValue();
                };
            Items.Add(lastFill);
        }
    }

    class TagWriterContextMenuProvider : PrimarySelectionContextMenuProvider
    {
        public TagWriterContextMenuProvider()
        {
            var lastFill = new MenuAction("Writer Editor");
            lastFill.Checkable = true;
            lastFill.Execute += (s, e) =>
            {
                ModelItem item = e.Selection.PrimarySelection;
                var tagWriter = item.GetCurrentValue() as ITagWriter;
                TagWriteEditor frm = new TagWriteEditor(tagWriter);
                frm.ShowDialog();
                if (frm.IsSaved)
                {
                    string txt = frm.TagText;
                    if (string.IsNullOrEmpty(txt) == false) { item.Properties["TagWriteText"].SetValue(txt); }
                    else if (txt == string.Empty) item.Properties["TagWriteText"].ClearValue();
                    if (frm.IsPulse)
                        item.Properties["IsPulse"].SetValue(true);
                    else
                        item.Properties["IsPulse"].ClearValue();
                }
            };
            Items.Add(lastFill);
        }
    }
}
