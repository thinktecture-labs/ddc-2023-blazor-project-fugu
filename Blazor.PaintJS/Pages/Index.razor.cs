using Blazor.PaintJS.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Drawing;
using KristofferStrube.Blazor.FileSystem;

namespace Blazor.PaintJS.Pages
{
    public partial class Index
    {
        private const string _PAINT_CANVAS = "paint-canvas";
        private const string _IMAGE_TYPE = "image/png";
        private const string _IMAGE_ID = "image";

        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] private PaintService _paintService { get; set; } = default!;
        [Inject] private ImageService _imageService { get; set; } = default!;

        private IJSObjectReference? _module;
        private DotNetObjectReference<Index>? _selfReference;

        #region FileHandle Properties
        protected FileSystemFileHandle? _fileHandle;

        //EX#11-2
        //EX#11-3
        //EX#12-1
        #endregion

        #region SupportedProps
        private bool _fileSystemAccessSupported = false;
        private bool _clipBoardApiSupported = false;
        private bool _sharedApiSupported = false;
        private bool _badgeApiSupported = false;
        #endregion

        //EX#1
        private Point? _previousPoint;
        private int _hasChanges = 0;

        // Method which is JSInvokable must be public
        [JSInvokable]
        public async void OnPointerUp()
        {
            await InternalPointerUp();
        }

        [JSInvokable]
        public async Task DrawImageAsync()
        {
            //EX#16 - 4
        }

        protected override async Task OnInitializedAsync()
        {
            // OVERALL: Check supported state with nuget packages
            // EX#17-2

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    //EX#2

                    //EX#16 - 3
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        

        // EX#11 - 4|5: Save File with FilesystemAccess
        private async Task SaveFileLocal()
        {
        }

        // EX#12 1 - 2|3: Open File with FilesystemAccess
        private async Task OpenLocalFile()
        {
        }

        // EX#13 Async Clipboard API | Copy | "getCanvasBlob", "paint-canvas"
        private async void Copy()
        {
        }

        // EX#14 Async Clipboard API | Paste
        private async Task Paste()
        {
        }

        // EX#15 WebShareAPI
        private async Task Share()
        {
        }

        private async Task InternalPointerUp()
        {
            _previousPoint = null;
        }

        private async Task OnPointerMove(PointerEventArgs args)
        {
            //EX#4
            //EX#5
        }

        private async void OnPointerDown(PointerEventArgs args)
        {
           //EX#4 
        }

        private async void OnColorChange(ChangeEventArgs args)
        {
            //EX#6
        }

        private async Task DownloadFile()
        {
            //EX#18
        }

        private async Task OpenFile(InputFileChangeEventArgs args)
        {
            //EX#19
        }
       

        #region Helper Methods

        private async Task ResetCanvas()
        {
            //await using var context = await _canvas!.GetContext2DAsync();
            //await context.FillStyleAsync("white");
            //await context.FillRectAsync(0, 0, 600, 480);
            //await context.FillStyleAsync("black");
            //await UpdateBage(true);
        }     
        #endregion
    }
}