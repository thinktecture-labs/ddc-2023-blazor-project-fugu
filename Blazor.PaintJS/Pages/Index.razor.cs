using Blazor.PaintJS.Services;
using Excubo.Blazor.Canvas;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Drawing;
using KristofferStrube.Blazor.FileSystemAccess;
using KristofferStrube.Blazor.FileSystem;
using Thinktecture.Blazor.AsyncClipboard;
using Thinktecture.Blazor.Badging;
using Thinktecture.Blazor.AsyncClipboard.Models;
using Thinktecture.Blazor.WebShare.Models;
using Thinktecture.Blazor.WebShare;

namespace Blazor.PaintJS.Pages
{
    public partial class Index
    {
        private const string _PAINT_CANVAS = "paint-canvas";
        private const string _IMAGE_TYPE = "image/png";
        private const string _IMAGE_ID = "image";

        [Inject] private PaintService _paintService { get; set; } = default!;
        [Inject] private ImageService _imageService { get; set; } = default!;
        [Inject] private AsyncClipboardService _asyncClipboardService { get; set; } = default!;
        [Inject] private WebShareService _shareService { get; set; } = default!;
        [Inject] private BadgingService _badgingService { get; set; } = default!;
        [Inject] private IFileSystemAccessService _fileSystemAccessService { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        private Canvas? _canvas;
        private IJSObjectReference? _module;
        private DotNetObjectReference<Index>? _selfReference;

        #region FileHandle Properties
        protected FileSystemFileHandle? _fileHandle;

        //EX#11-2
        private static FilePickerAcceptType[] _acceptedTypes = new FilePickerAcceptType[]
        {
            new FilePickerAcceptType
            {
                Accept = new Dictionary<string, string[]>
                {
                    { "image/png", new[] {".png" } }
                }
            }
        };

        //EX#11-3
        private SaveFilePickerOptionsStartInWellKnownDirectory _savePickerOptions = new SaveFilePickerOptionsStartInWellKnownDirectory
        {
            StartIn = WellKnownDirectory.Pictures,
            Types = _acceptedTypes
        };

        //EX#12-1
        private OpenFilePickerOptionsStartInWellKnownDirectory _openFilePickerOptions = new OpenFilePickerOptionsStartInWellKnownDirectory
        {
            Multiple = false,
            StartIn = WellKnownDirectory.Pictures,
            Types = _acceptedTypes
        };
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
            await using var context = await _canvas!.GetContext2DAsync();
            await context.DrawImageAsync("image", 0, 0);
        }

        protected override async Task OnInitializedAsync()
        {
            // OVERALL: Check supported state with nuget packages
            // EX#17-2
            _fileSystemAccessSupported = await _fileSystemAccessService.IsSupportedAsync();
            _clipBoardApiSupported = await _asyncClipboardService.IsSupportedAsync();
            _sharedApiSupported = await _shareService.IsSupportedAsync();
            _badgeApiSupported = await _badgingService.IsSupportedAsync();

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    //EX#2
                    await using var context = await _canvas!.GetContext2DAsync();
                    await context.FillStyleAsync("white");
                    await context.FillRectAsync(0, 0, 600, 480);
                    await context.FillStyleAsync("black");

                    //EX#16 - 3
                    _selfReference = DotNetObjectReference.Create(this);
                    if (_module == null)
                    {
                        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./Pages/Index.razor.js");
                        await _module.InvokeVoidAsync("initializeLaunchQueue", _selfReference);
                    }
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
            try
            {
                if (_fileHandle == null)
                {
                    _fileHandle = await _fileSystemAccessService.ShowSaveFilePickerAsync(_savePickerOptions);
                }

                var writeable = await _fileHandle.CreateWritableAsync();
                var image = await _imageService.GetImageDataAsync(_PAINT_CANVAS);
                await writeable.WriteAsync(image);
                await writeable.CloseAsync();

                await _fileHandle.JSReference.DisposeAsync();
                _fileHandle = null;
            }
            catch (Exception)
            {
                Console.WriteLine("Save file failed");
            }
            finally
            {
                _fileHandle = null;
            }
        }

        // EX#12 1 - 2|3: Open File with FilesystemAccess
        private async Task OpenLocalFile()
        {
            try
            {
                var fileHandles = await _fileSystemAccessService.ShowOpenFilePickerAsync(_openFilePickerOptions);
                _fileHandle = fileHandles.Single();
            }
            catch (JSException ex)
            {
                // Handle Exception or cancelation of File Access prompt
                Console.WriteLine(ex);
            }
            finally
            {
                if (_fileHandle is not null)
                {
                    var file = await _fileHandle.GetFileAsync();
                    await _imageService.OpenFileAccessAsync(file.JSReference);
                    await using var context = await _canvas!.GetContext2DAsync();
                    await context.DrawImageAsync("image", 0, 0);
                }
            }
        }

        // EX#13 Async Clipboard API | Copy | "getCanvasBlob", "paint-canvas"
        private async void Copy()
        {
            var imagePromise = _asyncClipboardService.GetObjectReference(_module!, "getCanvasBlob", _PAINT_CANVAS);
            var clipboardItem = new ClipboardItem(new Dictionary<string, IJSObjectReference>
                        {
                            { _IMAGE_TYPE, imagePromise }
                        });
            await _asyncClipboardService.WriteAsync(new[] { clipboardItem });
        }

        // EX#14 Async Clipboard API | Paste
        private async Task Paste()
        {
            var clipboardItems = await _asyncClipboardService.ReadAsync();
            var pngItem = clipboardItems.FirstOrDefault(c => c.Types.Contains(_IMAGE_TYPE));
            if (pngItem is not null)
            {
                var blob = await pngItem.GetTypeAsync(_IMAGE_TYPE);
                await _imageService.OpenFileAccessAsync(blob);
                await using var context = await _canvas!.GetContext2DAsync();
                await context.DrawImageAsync(_IMAGE_ID, 0, 0);
            }
        }

        // EX#15 WebShareAPI
        private async Task Share()
        {
            var fileReference = await _imageService.GenerateFileReferenceAsync(await _canvas!.ToDataURLAsync());
            await _shareService.ShareAsync(new WebShareDataModel
            {
                Files = [fileReference]
            }
            );
        }



        //LEKTION 5: Update Badge with API
        private async Task UpdateBage(bool reset = false)
        {
            if (_badgeApiSupported)
            {
                _hasChanges = reset ? 0 : _hasChanges + 1;
                await _badgingService.SetAppBadgeAsync(_hasChanges);
            }
        }

        private async Task InternalPointerUp()
        {
            _previousPoint = null;
            await UpdateBage();
        }

        private async Task OnPointerMove(PointerEventArgs args)
        {
            //EX#4
            //EX#5
            if (_previousPoint != null)
            {
                var currentPoint = new Point
                {
                    X = (int)Math.Floor(args.OffsetX),
                    Y = (int)Math.Floor(args.OffsetY)
                };

                var points = _paintService.BrensenhamLine(_previousPoint.Value, currentPoint);
                await using var context = await _canvas!.GetContext2DAsync();
                foreach (var point in points)
                {
                    await context.FillRectAsync(point.X, point.Y, 2, 2);
                }

                _previousPoint = currentPoint;
            }
        }

        private async void OnPointerDown(PointerEventArgs args)
        {
            //EX#4 
            if (_module != null && _canvas!.AdditionalAttributes.TryGetValue("id", out var id))
            {
                await _module.InvokeVoidAsync("registerEvents", id, _selfReference);
            }

            _previousPoint = new Point
            {
                X = (int)Math.Floor(args.OffsetX),
                Y = (int)Math.Floor(args.OffsetY)
            };
        }

        private async void OnColorChange(ChangeEventArgs args)
        {
            //EX#6
            await using var context = await _canvas!.GetContext2DAsync();
            await context.FillStyleAsync(args.Value?.ToString());
        }

        private async Task DownloadFile()
        {
            //EX#18
            await _imageService.DownloadAsync(await _canvas!.ToDataURLAsync());
        }

        private async Task OpenFile(InputFileChangeEventArgs args)
        {
            //EX#19
            await using var context = await _canvas!.GetContext2DAsync();
            await _imageService.OpenAsync(args.File.OpenReadStream(1024 * 15 * 1000));
            await context.DrawImageAsync("image", 0, 0);
        }
       

        #region Helper Methods

        private async Task ResetCanvas()
        {
            await using var context = await _canvas!.GetContext2DAsync();
            await context.FillStyleAsync("white");
            await context.FillRectAsync(0, 0, 600, 480);
            await context.FillStyleAsync("black");
            await UpdateBage(true);
        }     
        #endregion
    }
}