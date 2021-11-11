using System.IO;
using KSynthLib.K5000;
using Terminal.Gui;

namespace K5KTool
{
    public class App
    {
        private MenuBar _menu;
        private Toplevel _top;
        private StatusBar _statusBar;
        private Window _window;

        private ListView _listView;

        private string _fileName;

        private SinglePatch _singlePatch;

        private SinglePatchView _singlePatchView;

        public App()
        {
        }

        public void Run()
        {
            Application.Init();

            _menu = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("_File", new MenuItem[]
                {
                    new MenuItem("_Open", "", () => Open(), null, null, Key.O | Key.CtrlMask),
                    new MenuItem("_Quit", "", () => Quit(), null, null, Key.Q | Key.CtrlMask)
                }),
                new MenuBarItem("_Help", new MenuItem[]
                {
                    new MenuItem("_About...", "About this app", () =>  MessageBox.Query("About K5KTool", "K5KTool", "_OK"), null, null, Key.CtrlMask | Key.A),
                })
            });

            _statusBar = new StatusBar()
            {
                Visible = true,
            };
            _statusBar.Items = new StatusItem[]
            {
                new StatusItem(Key.Q | Key.CtrlMask, "~Ctrl-Q~ Quit", () =>
                {
                    Application.RequestStop();
                })
            };

            _top = Application.Top;
            _top.Add(_menu);
            _top.Add(_statusBar);

            _window = new Window("FooBar")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
            };

/*
            _listView = new ListView()
            {
                X = 1,
                Y = 2,
                Height = Dim.Fill(),
                Width = Dim.Fill(1),
                AllowsMarking = false,
                AllowsMultipleSelection = false,
            };
            _window.Add(_listView);
*/

            _singlePatchView = new SinglePatchView()
            {
                X = 1,
                Y = 2,
                Height = Dim.Fill(),
                Width = Dim.Fill(1),
            };

            _window.Add(_singlePatchView);

            _top.Add(_window);

            Application.Run(_top);

            Application.Shutdown();
        }

        private void Open()
        {
            var d = new OpenDialog("Open", "Open a file")
            {
                AllowsMultipleSelection = false,
            };
            Application.Run(d);

            if (!d.Canceled)
            {
                _fileName = d.FilePaths[0];

                LoadFile();
            }
        }

        private void LoadFile()
        {
            if (_fileName != null)
            {
                var data = System.IO.File.ReadAllBytes(_fileName);

                _singlePatch = new SinglePatch(data);

                _singlePatchView.Patch = _singlePatch;
            }
        }

        private void Quit()
        {
            Application.RequestStop();
        }
    }

    public class SinglePatchView: View
    {
        private SinglePatch _singlePatch;
        public SinglePatch Patch {
            get
            {
                return _singlePatch;
            }

            set
            {
                _singlePatch = value;
                SetNeedsDisplay();
            }
        }

        public SinglePatchView()
        {
        }

    }
}