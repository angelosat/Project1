using Project1.Core.UI;
using Project1.Core.UI;

namespace Project1.Core.Gui
{
    static class GuiHelper
    {
        //static public ScrollableBoxTest MakeScrollable(this GroupBox control, int viewportW, int viewportH, ScrollModes mode = ScrollModes.Both, bool ensureFit = true)
        //{
        //    if (ensureFit)
        //        return ScrollableBoxTest.FromContentsSize(viewportW, viewportH, mode)
        //           .AddControls(control) as ScrollableBoxTest;
        //    else
        //        return new ScrollableBoxTest(control, viewportW, viewportH, mode);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="w">input -1 to fit content horizontally</param>
        /// <param name="h">input -1 to fit content vertically</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        static public ScrollableBoxNewNewNew MakeScrollable(this GroupBox control, int w, int h)
        {
            if (w == -1 && h == -1)
                throw new System.Exception();
            if(w == -1)
            {
                return ScrollableBoxNewNewNew.FromContentsSize(control.Width, h, ScrollModes.Vertical)
                   .AddControls(control) as ScrollableBoxNewNewNew;
            }
            else if(h == -1)
            {
                return ScrollableBoxNewNewNew.FromContentsSize(w, control.Height, ScrollModes.Horizontal)
                  .AddControls(control) as ScrollableBoxNewNewNew;
            }
            else
                return new ScrollableBoxNewNewNew(w, h).AddControls(control) as ScrollableBoxNewNewNew;
        }
    }
}
