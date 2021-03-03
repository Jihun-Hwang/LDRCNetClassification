using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Pentacube.CAx.DFx.LDRC.Views;
using Pentacube.CAx.DFx.LDRCBase;
using Pentacube.CAx.DFx.LDRCBase.Display;
using Pentacube.CAx.DFx.LDRCBase.Model;
using Pentacube.CAx.DFx.LDRCBase.Service;
using Pentacube.CAx.DFx.LDRCBase.Service.Interfaces;
using Pentacube.CAx.ECAD.LcadIf.DataAccess;
using Pentacube.CAx.ECAD.LcadIf.Model.PLDB;
using Pentacube.CAx.ECAD.LcadIf.Model.PLDB.Draw.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LDRCNetClassification
{
    internal sealed class PLDBHelper
    {
        /// <summary>
        /// EDF 파일 경로를 받아 새 <see cref="Design"/>을 반환합니다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Design GetDesignFrom(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();
            var repo = container.Resolve<PLDBRepo>();

            return repo.LoadFromEdif(path).FirstOrDefault();
        }

        public static DRCConfigViewModel GetPLDBRepo()
        {
            var container = ServiceLocator.Current.GetInstance<IUnityContainer>();

            container.RegisterInstance<ITreeView>("NetListView", new TreeView());
            container.RegisterInstance<ITreeView>("PartListView", new TreeView());

            var mediator = container.Resolve<SingleLDRCMediator>(
                new ParameterOverride("canvasController", new SheetViewManager()));

            return container.Resolve<DRCConfigViewModel>(
                new ParameterOverride("mediator", mediator));
        }

        private sealed class TreeView : ITreeView
        {
            public bool IsActive { get; set; } = true;

            public event Action<object> ItemSelectingHandler;
            public event EventHandler IsActiveChanged;

            public void Clear()
            {
            }

            public void Select(Func<List<object>, object> selectFunc)
            {
            }

            public void Set(IEnumerable<ITreeSource<object>> sources)
            {
            }
        }

        private sealed class SheetViewManager : IPLDBCanvasController
        {
            public event Action<IHighlightable> SelectingEventHandler;
            public event Action UnselectingEventHandler;

            public void Activate(Sheet sheet)
            {
            }

            public IReadOnlyList<ISheetController> GetActivatedControllers()
            {
                return new List<ISheetController>().AsReadOnly();
            }

            public IReadOnlyList<ISheetController> GetControllers()
            {
                return new List<ISheetController>().AsReadOnly();
            }

            public int GetCountOfGraphics(IHighlightable item)
            {
                return 0;
            }

            public ISheetController GetLastActivatedSheetController()
            {
                return default;
            }

            public IEnumerable<HighlightItemBoundBox> GetSheetBoundBoxes(IHighlightable target)
            {
                return Enumerable.Empty<HighlightItemBoundBox>();
            }

            public IReadOnlyList<ISheetController> GetVisibleControllers()
            {
                return new List<ISheetController>().AsReadOnly();
            }

            public void Highlight(IHighlightingItem<IHighlightable> item)
            {
            }

            public void Load(Design design)
            {
            }

            public void Mark(Sheet sheet)
            {
            }

            public void Unhighlight(IHighlightable item)
            {
            }

            public void UnhighlightAll()
            {
            }

            public void Unload()
            {
            }

            public void Update(IHighlightingItem<IHighlightable> item)
            {
            }

            public void ZoomArea(Sheet sheet, System.Windows.Rect targetArea)
            {
            }
        }
    }
}
