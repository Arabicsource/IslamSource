﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

public class WindowsRTFileIO : XMLRender.PortableFileIO
{
    public /*async*/ string[] GetDirectoryFiles(string Path)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFolder> t = Windows.Storage.StorageFolder.GetFolderFromPathAsync(Path).AsTask();
        t.Wait();
        Windows.Storage.StorageFolder folder = t.Result; //await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Path);
        System.Threading.Tasks.Task<IReadOnlyList<Windows.Storage.StorageFile>> tn = folder.GetFilesAsync().AsTask();
        t.Wait();
        IReadOnlyList<Windows.Storage.StorageFile> files = tn.Result; //await folder.GetFilesAsync();
        return new List<string>(files.Select(file => file.Name)).ToArray();
    }
    public /*async*/ Stream LoadStream(string FilePath)
    {
        Windows.ApplicationModel.Resources.Core.ResourceCandidate rc = Windows.ApplicationModel.Resources.Core.ResourceManager.Current.MainResourceMap.GetValue(FilePath.Replace(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "ms-resource:///Files").Replace("\\", "/"), Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView());
        System.Threading.Tasks.Task<Windows.Storage.StorageFile> t;
        if (rc != null && rc.IsMatch) {
            t = rc.GetValueAsFileAsync().AsTask();
        } else {
            t = Windows.Storage.StorageFile.GetFileFromPathAsync(FilePath).AsTask();
        }
        t.Wait();
        Windows.Storage.StorageFile file = t.Result; //await Windows.Storage.StorageFile.GetFileFromPathAsync(FilePath);
        System.Threading.Tasks.Task<Stream> tn = file.OpenStreamForReadAsync();
        tn.Wait();
        Stream Stream = tn.Result; //await file.OpenStreamForReadAsync();
        return Stream;
    }
    public /*async*/ void SaveStream(string FilePath, Stream Stream)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFolder> td = Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(FilePath)).AsTask();
        td.Wait();
        System.Threading.Tasks.Task<Windows.Storage.StorageFile> t = td.Result.CreateFileAsync(System.IO.Path.GetFileName(FilePath), Windows.Storage.CreationCollisionOption.ReplaceExisting).AsTask();
        t.Wait();
        Windows.Storage.StorageFile file = t.Result; //await (await Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(FilePath))).Result.CreateFileAsync(System.IO.Path.GetFileName(FilePath));
        System.Threading.Tasks.Task<Stream> tn = file.OpenStreamForWriteAsync();
        tn.Wait();
        Stream File = tn.Result; //await file.OpenStreamForWriteAsync();
        File.Seek(0, SeekOrigin.Begin);
        byte[] Bytes = new byte[4096];
        int Read;
        Stream.Seek(0, SeekOrigin.Begin);
        Read = Stream.Read(Bytes, 0, Bytes.Length);
        while (Read != 0)
        {
            File.Write(Bytes, 0, Read);
            Read = Stream.Read(Bytes, 0, Bytes.Length);
        }
        File.Dispose();
    }
    public string CombinePath(params string[] Paths)
    {
        return System.IO.Path.Combine(Paths);
    }
    public /*async*/ void DeleteFile(string FilePath)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFile> t = Windows.Storage.StorageFile.GetFileFromPathAsync(FilePath).AsTask();
        t.Wait();
        Windows.Storage.StorageFile file = t.Result; //await Windows.Storage.StorageFile.GetFileFromPathAsync(FilePath);
        file.DeleteAsync().AsTask().Wait();
        //await file.DeleteAsync();
    }
    public /*async*/ bool PathExists(string Path)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFolder> t = Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(Path)).AsTask();
        t.Wait();
#if WINDOWS_PHONE_APP
        System.Threading.Tasks.Task<IReadOnlyList<Windows.Storage.IStorageItem>> files = t.Result.GetItemsAsync().AsTask();
        files.Wait();
        return files.Result.FirstOrDefault(p => p.Name == Path) != null;
#else
        System.Threading.Tasks.Task<Windows.Storage.IStorageItem> tn = t.Result.TryGetItemAsync(System.IO.Path.GetFileName(Path)).AsTask();
        tn.Wait();
        return tn.Result != null;
#endif
        //return (await (await Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(Path))).TryGetItemAsync(System.IO.Path.GetFileName(Path))) != null;
    }
    public /*async*/ void CreateDirectory(string Path)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFolder> t = Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(Path)).AsTask();
        t.Wait();
        t.Result.CreateFolderAsync(System.IO.Path.GetFileName(Path), Windows.Storage.CreationCollisionOption.OpenIfExists).AsTask().Wait();
        //await (await Windows.Storage.StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(Path))).CreateFolderAsync(System.IO.Path.GetFileName(Path), Windows.Storage.CreationCollisionOption.FailIfExists);
    }
    public /*async*/ DateTime PathGetLastWriteTimeUtc(string Path)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFile> t = Windows.Storage.StorageFile.GetFileFromPathAsync(Path).AsTask();
        t.Wait();
        Windows.Storage.StorageFile file = t.Result; //await Windows.Storage.StorageFile.GetFileFromPathAsync(Path);
        System.Threading.Tasks.Task<Windows.Storage.FileProperties.BasicProperties> tn = file.GetBasicPropertiesAsync().AsTask();
        tn.Wait();
        return tn.Result.DateModified.UtcDateTime;
        //return (await file.GetBasicPropertiesAsync()).DateModified;
    }
    public /*async*/ void PathSetLastWriteTimeUtc(string Path, DateTime Time)
    {
        System.Threading.Tasks.Task<Windows.Storage.StorageFile> t = Windows.Storage.StorageFile.GetFileFromPathAsync(Path).AsTask();
        t.Wait();
        Windows.Storage.StorageFile file = t.Result; //await Windows.Storage.StorageFile.GetFileFromPathAsync(Path);
        System.Threading.Tasks.Task<Windows.Storage.StorageStreamTransaction> tn = file.OpenTransactedWriteAsync().AsTask();
        tn.Wait();
        tn.Result.CommitAsync().AsTask().Wait();
        //await(await file.OpenTransactedWriteAsync()).CommitAsync();
    }
}
public class WindowsRTSettings : XMLRender.PortableSettings
{
    public string CacheDirectory
    {
        get {
            //Windows.Storage.ApplicationData.Current.LocalFolder.InstalledLocation;
            return Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        }
    }
    public KeyValuePair<string, string[]>[] Resources
    {
        get {
            return (new List<KeyValuePair<string, string[]>>(System.Linq.Enumerable.Select("HostPageUtility=Acct,lang,unicode;IslamResources=Hadith,IslamInfo,IslamSource".Split(';'), Str => new KeyValuePair<string, string[]>(Str.Split('=')[0], Str.Split('=')[1].Split(','))))).ToArray();
        }
    }
    public string[] FuncLibs
    {
        get
        {
            return new string[] {"IslamMetadata"};
        }
    }
    public string GetTemplatePath() {
        return GetFilePath("metadata\\IslamSource.xml");
    }
    public string GetFilePath(string Path)
    {
        return System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, Path);
    }
    public string GetUName(char Character)
    {
        return "";
    }
    public static async System.Threading.Tasks.Task SavePathImageAsFile(int Width, int Height, string fileName, FrameworkElement element, bool UseRenderTarget = true)
    {
#if WINDOWS_APP && STORETOOLKIT
        if (!UseRenderTarget)
        {
            double oldWidth = element.Width;
            double oldHeight = element.Height;
            //engine takes the Ceiling so make sure its below or sometimes off by 1 rounding up from ActualWidth/Height
            element.Width = Math.Floor((float)Width);
            element.Height = Math.Floor((float)Height);
            element.UpdateLayout();
            //await element.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => element.Dispatcher.ProcessEvents(Windows.UI.Core.CoreProcessEventsOption.ProcessAllIfPresent)));
            await WinRTXamlToolkit.AwaitableUI.EventAsync.FromEvent<object>(eh => element.LayoutUpdated += eh, eh => element.LayoutUpdated -= eh);
            if (element.ActualWidth > element.Width || element.ActualHeight > element.Height)
            {
                if (element.ActualWidth > element.Width) element.Width -= 1;
                if (element.ActualHeight > element.Height) element.Height -= 1;
                element.UpdateLayout();
                await WinRTXamlToolkit.AwaitableUI.EventAsync.FromEvent<object>(eh => element.LayoutUpdated += eh, eh => element.LayoutUpdated -= eh);
            }
            System.IO.MemoryStream memstream = await WinRTXamlToolkit.Composition.WriteableBitmapRenderExtensions.RenderToPngStream(element);
            element.Width = oldWidth;
            element.Height = oldHeight;
            element.UpdateLayout();
            await WinRTXamlToolkit.AwaitableUI.EventAsync.FromEvent<object>(eh => element.LayoutUpdated += eh, eh => element.LayoutUpdated -= eh);
            Windows.Storage.StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(fileName + ".png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            await stream.WriteAsync(memstream.GetWindowsRuntimeBuffer());
            stream.Dispose();
        }
        else
        {
#endif
            //Canvas cvs = new Canvas();
            //cvs.Width = Width;
            //cvs.Height = Height;
            //Windows.UI.Xaml.Shapes.Path path = new Windows.UI.Xaml.Shapes.Path();
            //object val;
            //Resources.TryGetValue((object)"PathString", out val);
            //Binding b = new Binding
            //{
            //    Source = (string)val
            //};
            //BindingOperations.SetBinding(path, Windows.UI.Xaml.Shapes.Path.DataProperty, b);
            //cvs.Children.Add(path);
            float dpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
            Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap wb = new Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap();
            await wb.RenderAsync(element, (int)((float)Width * 96 / dpi), (int)((float)Height * 96 / dpi));
            Windows.Storage.Streams.IBuffer buf = await wb.GetPixelsAsync();
            Windows.Storage.StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(fileName + ".png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            //Windows.Graphics.Imaging.BitmapPropertySet propertySet = new Windows.Graphics.Imaging.BitmapPropertySet();
            //propertySet.Add("ImageQuality", new Windows.Graphics.Imaging.BitmapTypedValue(1.0, Windows.Foundation.PropertyType.Single)); // Maximum quality
            Windows.Graphics.Imaging.BitmapEncoder be = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);//, propertySet);
            be.SetPixelData(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied, (uint)Width, (uint)Height, 96, 96, buf.ToArray());
            await be.FlushAsync();
            await stream.GetOutputStreamAt(0).FlushAsync();
            stream.Dispose();
#if WINDOWS_APP && STORETOOLKIT
        }
#endif
    }
}
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IslamSourceQuranViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            XMLRender.PortableMethods.FileIO = new WindowsRTFileIO();
            XMLRender.PortableMethods.Settings = new WindowsRTSettings();
            this.DataContext = this;
            this.ViewModel = new MyTabViewModel();
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            this.NavigationCacheMode = NavigationCacheMode.Required;
#endif
        }
        public MyTabViewModel ViewModel { get; set; }

        private void sectionListBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WordForWordUC), new {Division = ViewModel.SelectedItem.Index, Selection = ViewModel.ListSelectedItem.Index + 1});
        }

        private void RenderPngs_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ExtSplashScreen));
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(Settings));
        }
    }
    public class MyTabViewModel : INotifyPropertyChanged
    {
        public MyTabViewModel()
        {
            Items = System.Linq.Enumerable.Select(IslamMetadata.TanzilReader.GetDivisionTypes(), (Arr, idx) => new MyTabItem { Title = Arr, Index = idx });
        }

        public IEnumerable<MyTabItem> Items { get; set; }

        public IEnumerable<MyListItem> _ListItems;
        public IEnumerable<MyListItem> ListItems
        {
            get
            {
                return _ListItems;
            }
            private set
            {
                _ListItems = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ListItems"));
            }
        }

        public MyListItem ListSelectedItem
        {
            get { return _selectedItem == null ? null : _selectedItem.SelectedItem; }
            set
            {
                _selectedItem.SelectedItem = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ListSelectedItem"));
            }
        }

        private MyTabItem _selectedItem;

        public MyTabItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
                ListItems = _selectedItem.Items;
                ListSelectedItem = ListItems.First();
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class MyTabItem
    {
        public string Title { get; set; }
        public int Index { get; set; }
        private IEnumerable<MyListItem> _Items;
        public IEnumerable<MyListItem> Items
        {
            get
            {
                if (_Items == null) { _Items = System.Linq.Enumerable.Select(IslamMetadata.TanzilReader.GetSelectionNames(Index.ToString(), XMLRender.ArabicData.TranslitScheme.RuleBased, "PlainRoman"), (Arr, Idx) => new MyListItem { Name = (string)(Arr.Cast<object>()).First(), Index = Idx }); }
                return _Items;
            }
        }

        private MyListItem _selectedItem;

        public MyListItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
            }
        }
    }

    public class MyListItem
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
