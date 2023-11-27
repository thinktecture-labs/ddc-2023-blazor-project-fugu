using Microsoft.AspNetCore.Components;

namespace Blazor.PaintJS.Pages
{
    public partial class About
    {

        [Inject] private NavigationManager _navigationManager { get; set; } = default!;

        private void GoToPaint()
        {
            _navigationManager.NavigateTo("/");
        }
    }
}