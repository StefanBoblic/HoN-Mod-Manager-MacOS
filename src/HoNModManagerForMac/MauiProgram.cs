using HonModManagerForMac.Interfaces;

namespace HonModManagerForMac;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


//#if WINDOWS
//        builder.Services.AddTransient<IFolderPicker, Platforms.Windows.FolderPicker>();
// change this to #elif when done implementing windows part
#if MACCATALYST
        builder.Services.AddTransient<IFolderPicker, Platforms.MacCatalyst.FolderPicker>();
#endif
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<App>();
        // Note: end added part

        return builder.Build();
    }
}
